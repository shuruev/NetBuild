using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// Forces all existing projects to rebuild.
	/// </summary>
	public class RebuildAllDetector : EmptyDetector
	{
		private readonly HashSet<string> m_items;

		public RebuildAllDetector()
		{
			m_items = new HashSet<string>();
		}

		public override void SetTriggers(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			if (type != typeof(SourcePathTrigger))
				return;

			lock (m_sync)
			{
				m_items.Add(item);
			}
		}

		public override List<ItemModification> DetectChanges<T>(T signal)
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
	}
}
