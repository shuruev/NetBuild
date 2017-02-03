using System;
using System.Collections.Generic;
using System.Linq;

namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// Allows only limited number of projects to be built at the same time.
	/// </summary>
	public class ConcurrentBuildDetector : EmptyDetector
	{
		// timeout which is used to reset builds which were started, but were not completed
		private static readonly TimeSpan s_buildTimeout = TimeSpan.FromMinutes(10);

		// how often to check for the expired builds
		private static readonly TimeSpan s_checkEvery = TimeSpan.FromMinutes(2);

		private readonly int m_maxConcurrentBuilds;
		private readonly Dictionary<string, DateTime> m_building;

		private DateTime m_lastChecked;

		public ConcurrentBuildDetector(int maxConcurrentBuilds)
		{
			if (maxConcurrentBuilds <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxConcurrentBuilds));

			m_maxConcurrentBuilds = maxConcurrentBuilds;
			m_building = new Dictionary<string, DateTime>();

			m_lastChecked = DateTime.UtcNow;
		}

		public override bool ShouldIgnore(string item)
		{
			CleanupExpiredBuilds();

			return m_building.Count >= m_maxConcurrentBuilds;
		}

		/// <summary>
		/// Removes builds which were started, but were not completed by some reason.
		/// </summary>
		private void CleanupExpiredBuilds()
		{
			var now = DateTime.UtcNow;
			if (now.Subtract(m_lastChecked) < s_checkEvery)
				return;

			lock (m_sync)
			{
				foreach (var item in m_building.Where(i => now.Subtract(i.Value) > s_buildTimeout).ToList())
				{
					m_building.Remove(item.Key);
				}

				m_lastChecked = now;
			}
		}

		public override void StartBuild(string item, string label)
		{
			var max = m_maxConcurrentBuilds * 2;
			lock (m_sync)
			{
				if (m_building.Count >= max)
					throw new InvalidOperationException($"There are {m_building.Count} builds currently executing, but maximum {max} is allowed.");

				m_building[item] = DateTime.UtcNow;
			}
		}

		public override void StopBuild(string item, string label)
		{
			lock (m_sync)
			{
				m_building.Remove(item);
			}
		}
	}
}
