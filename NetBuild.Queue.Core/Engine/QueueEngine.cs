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
		private readonly QueueDb m_db;
		private readonly Limiter m_limiter;

		private readonly JsonSerializerSettings m_settings;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QueueEngine(QueueDb db, int maxParallelRequests)
		{
			if (db == null)
				throw new ArgumentNullException(nameof(db));

			m_db = db;
			m_limiter = new Limiter(GetType().FullName, maxParallelRequests);

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
			using (m_limiter.Wait())
			{
				m_db.SubmitItem(itemCode);
			}
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

			using (m_limiter.Wait())
			{
				m_db.SetTriggers(itemCode, type, input.Select(trigger => JsonConvert.SerializeObject(trigger, m_settings)));
			}
		}

		/// <summary>
		/// Processes any external signal which can trigger new builds, and returns a list of all potentially affected items.
		/// Can use known DTOs for their JSON representation.
		/// </summary>
		public List<string> ProcessSignal<TSignal>(TSignal signal) where TSignal : ISignal
		{
			if (signal == null)
				throw new ArgumentNullException(nameof(signal));

			using (m_limiter.Wait())
			{
				return m_db.ProcessSignal(signal.SignalType, JsonConvert.SerializeObject(signal, m_settings));
			}
		}

		/// <summary>
		/// Checks whether specified item should be built, and returns all corresponding modifications related to this build.
		/// </summary>
		public List<BuildDto> ShouldBuild(string itemCode)
		{
			using (m_limiter.Wait())
			{
				return m_db.ShouldBuild(itemCode);
			}
		}

		/// <summary>
		/// Starts build process for specified item, marking all the current modifications with specified reserve code.
		/// </summary>
		public void StartBuild(string itemCode, string reserveCode)
		{
			using (m_limiter.Wait())
			{
				m_db.StartBuild(itemCode, reserveCode);
			}
		}

		/// <summary>
		/// Marks specified build as completed, removing all corresponding modifications and updating related timestamps.
		/// </summary>
		public void CompleteBuild(string itemCode, string reserveCode)
		{
			using (m_limiter.Wait())
			{
				m_db.CompleteBuild(itemCode, reserveCode);
			}
		}
	}
}
