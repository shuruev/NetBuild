using System;
using System.Collections.Generic;
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
		/// For a specified item, updates a complete set of its triggers with a given type.
		/// </summary>
		public void SetTriggers(string itemCode, string triggerType, IEnumerable<string> triggerValues)
		{
			using (m_limiter.Wait())
			{
				m_db.SetTriggers(itemCode, triggerType, triggerValues);
			}
		}

		/// <summary>
		/// Processes any external signal, which potentially can trigger new builds.
		/// Can use known DTOs for their JSON representation.
		/// </summary>
		public void ProcessSignal<TSignal>(TSignal signal) where TSignal : ISignal
		{
			if (signal == null)
				throw new ArgumentNullException(nameof(signal));

			using (m_limiter.Wait())
			{
				m_db.ProcessSignal(signal.SignalType, JsonConvert.SerializeObject(signal, m_settings));
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
