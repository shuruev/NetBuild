using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Wraps underlying DB operations to provide typed calls and limit concurrency.
	/// </summary>
	public class QueueEngine
	{
		private const int c_retryCount = 3;
		private const int c_maxParallelRequests = 10;

		private static readonly ActionRetry s_retry;
		private static readonly ActionLimit s_limit;

		private readonly QueueDb m_db;
		private readonly JsonSerializerSettings m_settings;

		static QueueEngine()
		{
			s_retry = new ActionRetry(c_retryCount);
			s_limit = new ActionLimit(c_maxParallelRequests);
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QueueEngine(QueueDb db)
		{
			if (db == null)
				throw new ArgumentNullException(nameof(db));

			m_db = db;
			m_settings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};
		}

		/// <summary>
		/// Creates specified item if it does not exist yet.
		/// </summary>
		public void SubmitItem(string itemCode)
		{
			s_retry.Do(() =>
			{
				s_limit.Do(() =>
				{
					m_db.SubmitItem(itemCode);
				});
			});
		}

		/// <summary>
		/// For a specified item, updates a complete set of its triggers of a given type.
		/// </summary>
		public void SetTriggers<TTrigger>(string itemCode, IEnumerable<TTrigger> triggers) where TTrigger : ITrigger, new()
		{
			var input = triggers.ToList();

			var types = input.Select(trigger => trigger.TriggerType).Distinct().ToList();
			if (types.Count > 1)
				throw new InvalidOperationException($"Only triggers of the same type should be passed into this method, but found {types.Count} different types: {String.Join(", ", types.Select(name => "'" + name + "'"))}.");

			var type = types.FirstOrDefault();
			if (type == null)
			{
				// when we want to delete all existing triggers of a given type
				// we use C# type information to determine corresponding trigger type
				type = new TTrigger().TriggerType;
			}

			s_retry.Do(() =>
			{
				s_limit.Do(() =>
				{
					m_db.SetTriggers(itemCode, type, input.Select(trigger => JsonConvert.SerializeObject(trigger, m_settings)));
				});
			});
		}

		/// <summary>
		/// Processes any external signal which can trigger new builds, and returns a list of all potentially affected items.
		/// Can use known DTOs for their JSON representation.
		/// </summary>
		public List<string> ProcessSignal<TSignal>(TSignal signal) where TSignal : ISignal
		{
			if (signal == null)
				throw new ArgumentNullException(nameof(signal));

			var result = new List<string>();

			s_limit.Do(() =>
			{
				result = m_db.ProcessSignal(signal.SignalType, JsonConvert.SerializeObject(signal, m_settings));
			});

			return result;
		}

		/// <summary>
		/// Checks whether specified item should be built, and returns all corresponding modifications related to this build.
		/// </summary>
		public List<ModificationRow> ShouldBuild(string itemCode)
		{
			var result = new List<ModificationRow>();

			s_retry.Do(() =>
			{
				s_limit.Do(() =>
				{
					result = m_db.ShouldBuild(itemCode);
				});
			});

			return result;
		}

		/// <summary>
		/// Starts build process for specified item, marking all the current modifications with specified build code.
		/// </summary>
		public void StartBuild(string itemCode, string buildCode)
		{
			s_limit.Do(() =>
			{
				m_db.StartBuild(itemCode, buildCode);
			});
		}

		/// <summary>
		/// Marks specified build as completed, removing all corresponding modifications and updating related timestamps.
		/// </summary>
		public void CompleteBuild(string itemCode, string buildCode)
		{
			s_limit.Do(() =>
			{
				m_db.CompleteBuild(itemCode, buildCode);
			});
		}
	}
}
