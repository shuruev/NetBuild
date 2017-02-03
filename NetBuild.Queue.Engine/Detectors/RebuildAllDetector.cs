using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// Forces all existing projects to rebuild.
	/// </summary>
	public class RebuildAllDetector : IDetector
	{
		private readonly HashSet<string> m_items;

		public RebuildAllDetector()
		{
			m_items = new HashSet<string>();
		}

		public void SetTriggers(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			if (type != typeof(SourcePathTrigger))
				return;

			lock (m_items)
			{
				m_items.Add(item);
			}
		}

		public void AddModifications(IEnumerable<ItemModification> modifications)
		{
		}

		public List<ItemModification> DetectChanges<T>(T signal) where T : ISignal
		{
			var result = new List<ItemModification>();

			var local = signal as RebuildAllSignal;
			if (local == null)
				return result;

			// rebuild all known projects
			var items = m_items.ToList();

			foreach (var item in items)
			{
				result.Add(new ItemModification
				{
					Item = item,
					Modification = new Modification
					{
						Type = "force",
						Item = item,
						Comment = "Project was internally forced to rebuild",
						Date = DateTime.UtcNow
					}
				});
			}

			return result;
		}

		public bool ShouldIgnore(string item)
		{
			return false;
		}

		public void StartBuild(string item, string label)
		{
		}

		public void CompleteBuild(string item, string label)
		{
		}
	}
}
