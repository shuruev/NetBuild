using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public class BuildCompleteDetector : EmptyDetector
	{
		private readonly Dictionary<string, List<string>> m_referencesTo;
		private readonly Dictionary<string, List<string>> m_referencedBy;
		private readonly Dictionary<string, List<string>> m_allDependants;

		private readonly HashSet<string> m_pending;

		public BuildCompleteDetector()
		{

			m_referencesTo = new Dictionary<string, List<string>>();
			m_referencedBy = new Dictionary<string, List<string>>();
			m_allDependants = new Dictionary<string, List<string>>();

			m_pending = new HashSet<string>();
		}

		public override void SetTriggers(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			if (type != typeof(ReferenceItemTrigger))
				return;

			var references = triggers.Cast<ReferenceItemTrigger>().Select(i => i.ReferenceItem).ToList();

			lock (m_sync)
			{
				// check if anything changed compared to the current state
				if (m_referencesTo.ContainsKey(item))
				{
					var existing = m_referencesTo[item];
					if (Util.CompareLists(existing, references))
						return;
				}

				// add direct references
				m_referencesTo[item] = references;
			}

			lock (m_sync)
			{
				// rebuild reverse dependencies
				m_referencedBy.Clear();

				foreach (var all in m_referencesTo)
				{
					foreach (var reference in all.Value)
					{
						List<string> list;
						if (!m_referencedBy.TryGetValue(reference, out list))
						{
							list = new List<string>();
							m_referencedBy.Add(reference, list);
						}

						list.Add(all.Key);
					}
				}
			}

			lock (m_sync)
			{
				// rebuild full recursive dependants
				m_allDependants.Clear();

				foreach (var key in m_referencedBy.Keys)
				{
					m_allDependants[key] = CalculateAllDependants(key)
						.Distinct()
						.OrderBy(name => name)
						.ToList();
				}
			}
		}

		private List<string> CalculateAllDependants(string item, int deep = 0)
		{
			// avoiding cyclic references by limiting recursion deep
			if (deep > 25)
				return new List<string>();

			var result = new List<string>();
			foreach (var reference in GetReferencedBy(item))
			{
				result.Add(reference);
				result.AddRange(CalculateAllDependants(reference, deep++));
			}

			return result;
		}

		private List<string> GetReferencedBy(string item)
		{
			if (!m_referencedBy.ContainsKey(item))
				return new List<string>();

			return m_referencedBy[item];
		}

		private List<string> GetAllDependants(string item)
		{
			if (!m_allDependants.ContainsKey(item))
				return new List<string>();

			return m_allDependants[item];
		}

		public override void AddModifications(IEnumerable<ItemModification> modifications)
		{
			lock (m_sync)
			{
				// mark all new modifications as pending, so they would be considered
				// when calculating builds based on dependency graph
				foreach (var item in modifications.Select(i => i.Item).Distinct())
				{
					m_pending.Add(item);
				}
			}
		}

		public override List<ItemModification> DetectChanges<T>(T signal)
		{
			var result = new List<ItemModification>();

			var local = signal as BuildCompleteSignal;
			if (local == null)
				return result;

			if (String.IsNullOrEmpty(local.BuildItem))
				throw new InvalidOperationException("Build item is required for build complete signal.");

			// trigger builds for all items which directly depend on this one
			List<string> dependants;
			lock (m_sync)
			{
				dependants = GetReferencedBy(local.BuildItem);
			}

			foreach (var item in dependants)
			{
				result.Add(new ItemModification
				{
					Item = item,
					Modification = new Modification
					{
						Type = "reference",
						Item = local.BuildItem,
						Comment = "Referenced project was built",
						Date = DateTime.UtcNow
					}
				});
			}

			return result;
		}

		public override bool ShouldIgnore(string item)
		{
			// we should postpone builds for the specified item, if some of their referenced items
			// are already in the build queue (i.e. current item will be triggered anyway after they build
			lock (m_sync)
			{
				return m_pending.SelectMany(GetAllDependants).Contains(item);
			}
		}

		public override void CompleteBuild(string item, string label)
		{
			lock (m_sync)
			{
				// remove all pending modifications for this project
				m_pending.Remove(item);
			}
		}
	}
}
