using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetBuild.Queue.Engine
{
	public class QueueEngine
	{
		private readonly Triggers m_triggers;
		private readonly Modifications m_modifications;

		private readonly List<IDetector> m_detectors;

		public QueueEngine(Triggers triggers, Modifications modifications)
		{
			if (triggers == null)
				throw new ArgumentNullException(nameof(triggers));

			if (modifications == null)
				throw new ArgumentNullException(nameof(modifications));

			m_triggers = triggers;
			m_modifications = modifications;

			m_detectors = new List<IDetector>();
		}

		public void AddDetector(IDetector detector)
		{
			if (detector == null)
				throw new ArgumentNullException(nameof(detector));

			m_detectors.Add(detector);
		}

		public void Load()
		{
			var triggers = m_triggers.Load();
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

			var modifications = m_modifications.Load();
			Parallel.ForEach(m_detectors, detector =>
			{
				detector.AddModifications(modifications);
			});
		}

		public void SetTriggers<T>(string item, IEnumerable<T> triggers) where T : ITrigger
		{
			var input = triggers.ToList();

			var updated = m_triggers.Set(item, input);
			if (!updated)
				return;

			Parallel.ForEach(m_detectors, detector =>
			{
				detector.SetTriggers(item, typeof(T), input.Cast<ITrigger>());
			});
		}

		public void ProcessSignal<T>(T signal) where T : ISignal
		{
			if (signal == null)
				throw new ArgumentNullException(nameof(signal));

			var changes = new List<ItemModification>();
			foreach (var detector in m_detectors)
			{
				var local = detector.DetectChanges(signal);
				changes.AddRange(local);
			}

			m_modifications.Add(changes);
			Parallel.ForEach(m_detectors, detector =>
			{
				detector.AddModifications(changes);
			});
		}

		/// <summary>
		/// Checks whether specified item should be built, and returns all corresponding modifications related to this build.
		/// </summary>
		public List<Modification> ShouldBuild(string item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			var changes = m_modifications.Get(item);
			if (changes.Count == 0)
				return new List<Modification>();

			foreach (var detector in m_detectors)
			{
				if (detector.ShouldIgnore(item))
					return new List<Modification>();
			}

			return changes;
		}
	}
}
