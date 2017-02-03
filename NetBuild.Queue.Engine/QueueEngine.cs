﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetBuild.Common;
using NetBuild.Queue.Core;
using Newtonsoft.Json.Linq;

namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// An engine which provide a certain strategy of queueing and starting the builds.
	/// </summary>
	public class QueueEngine : ITriggerSetup
	{
		private readonly object m_sync;

		private readonly Triggers m_triggers;
		private readonly Modifications m_modifications;

		private readonly ILogger m_logger;
		private readonly List<IDetector> m_detectors;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QueueEngine(Triggers triggers, Modifications modifications, ILogger logger = null)
		{
			if (triggers == null)
				throw new ArgumentNullException(nameof(triggers));

			if (modifications == null)
				throw new ArgumentNullException(nameof(modifications));

			m_sync = new object();

			m_triggers = triggers;
			m_modifications = modifications;

			m_logger = logger;
			m_detectors = new List<IDetector>();
		}

		/// <summary>
		/// Adds new detector, which defines queueing strategy.
		/// </summary>
		public void AddDetector(IDetector detector)
		{
			if (detector == null)
				throw new ArgumentNullException(nameof(detector));

			m_detectors.Add(detector);
			m_logger.LogDebug("Added detector {DetectorType}.", detector.GetType().Name);
		}

		/// <summary>
		/// Loads all data from database and populates in-memory collections.
		/// </summary>
		public void Load()
		{
			// load triggers
			var triggers = m_triggers.Load();

			// notify detectors about added triggers
			Parallel.ForEach(m_detectors, detector =>
			{
				foreach (var item in triggers.GroupBy(i => i.Item))
				{
					foreach (var type in item.GroupBy(i => i.Trigger.GetType()))
					{
						detector.SetTriggers(item.Key, type.Key, type.Select(i => i.Trigger));
					}
				}
			});

			// load modifications
			var modifications = m_modifications.Load();

			// notify detectors about added modifications
			Parallel.ForEach(m_detectors, detector =>
			{
				detector.AddModifications(modifications);
			});
		}

		/// <summary>
		/// For a specified item, updates a complete set of its triggers of a given type.
		/// </summary>
		bool ITriggerSetup.Set(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			var input = triggers.ToList();

			// update triggers if needed
			var updated = m_triggers.Set(item, type, input);
			if (!updated)
				return false;

			// notify detectors about changes
			Parallel.ForEach(m_detectors, detector =>
			{
				detector.SetTriggers(item, type, input);
			});

			return true;
		}

		/// <summary>
		/// For a specified item, updates a complete set of its triggers of a given type.
		/// </summary>
		public bool SetTriggers(string item, string triggerType, List<JObject> triggerValues)
		{
			var type = KnownTriggers.Get(triggerType);
			var triggers = triggerValues
				.Select(trigger => (ITrigger)ObjectSerializer.Deserialize(type, trigger))
				.ToList();

			return (this as ITriggerSetup).Set(item, type, triggers);
		}

		/// <summary>
		/// Processes any external signal, which can trigger new builds.
		/// Returns a list of items which were potentially affected by this change.
		/// </summary>
		public List<string> ProcessSignal<T>(T signal) where T : ISignal
		{
			if (signal == null)
				throw new ArgumentNullException(nameof(signal));

			// calculate if signal leads to any changes
			var changes = new List<ItemModification>();
			foreach (var detector in m_detectors)
			{
				var local = detector.DetectChanges(signal);
				changes.AddRange(local);
			}

			// add modifications
			m_modifications.Add(changes);

			// notify detectors about changes
			Parallel.ForEach(m_detectors, detector =>
			{
				detector.AddModifications(changes);
			});

			return changes
				.Select(i => i.Item)
				.Distinct()
				.OrderBy(name => name)
				.ToList();
		}

		/// <summary>
		/// Processes any external signal, which can trigger new builds.
		/// Returns a list of items which were potentially affected by this change.
		/// </summary>
		public List<string> ProcessSignal(string signalType, JObject signalValue)
		{
			var type = KnownSignals.Get(signalType);
			var signal = (ISignal)ObjectSerializer.Deserialize(type, signalValue);
			return ProcessSignal(signal);
		}

		/// <summary>
		/// Checks whether specified item should be built, and returns all corresponding modifications related to this build.
		/// </summary>
		public List<Modification> ShouldBuild(string item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			// check if any modifications exist
			var changes = m_modifications.Get(item);
			if (changes.Count == 0)
				return new List<Modification>();

			// check if build should be ignored by some reason
			foreach (var detector in m_detectors)
			{
				if (detector.ShouldIgnore(item))
					return new List<Modification>();
			}

			if (changes.Count > 0)
			{
				// If we tell some item to start build (eventually), let's wait a couple of seconds
				// just to increase chances of getting start build signal so that concurrent build
				// detector could react. Otherwise a lot of parallel items could receive changes here
				// and start their builds, and even if they will send their start build signals later,
				// concurrent build detector would not work for this items already.
				lock (m_sync)
				{
					Thread.Sleep(2000);
				}
			}

			return changes;
		}

		/// <summary>
		/// Starts build process for specified item, marking all the current modifications with specified build label.
		/// </summary>
		public void StartBuild(string item, string label)
		{
			// reserve modifications which exist at this moment for this item
			m_modifications.Reserve(item);

			// notify detectors
			Parallel.ForEach(m_detectors, detector =>
			{
				detector.StartBuild(item, label);
			});
		}

		/// <summary>
		/// Marks specified build as completed, removing all corresponding modifications and updating related timestamps.
		/// </summary>
		public void CompleteBuild(string item, string label)
		{
			// release all previously reserved modifications for this item
			m_modifications.Release(item);

			// notify detectors
			Parallel.ForEach(m_detectors, detector =>
			{
				detector.CompleteBuild(item, label);
			});
		}
	}
}