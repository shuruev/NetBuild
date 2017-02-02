using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public class BuildCompleteDetector : IDetector
	{
		public readonly Dictionary<string, List<string>> m_referencesTo;
		public readonly Dictionary<string, List<string>> m_referencedBy;
		public readonly Dictionary<string, List<string>> m_allDependants;

		public readonly HashSet<string> m_pending;

		public BuildCompleteDetector()
		{
			m_referencesTo = new Dictionary<string, List<string>>();
			m_referencedBy = new Dictionary<string, List<string>>();
			m_allDependants = new Dictionary<string, List<string>>();

			m_pending = new HashSet<string>();
		}

		public void SetTriggers(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			if (type != typeof(ReferenceItemTrigger))
				return;

			var references = triggers.Cast<ReferenceItemTrigger>().Select(i => i.ReferenceItem).ToList();

			lock (m_referencesTo)
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

			lock (m_referencedBy)
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

			lock (m_allDependants)
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

		public void AddModifications(IEnumerable<ItemModification> modifications)
		{
			lock (m_pending)
			{
				foreach (var item in modifications.Select(i => i.Item).Distinct())
				{
					m_pending.Add(item);
				}
			}
		}

		public List<ItemModification> DetectChanges<T>(T signal) where T : ISignal
		{
			var result = new List<ItemModification>();

			/*var local = signal as SourceChangedSignal;
			if (local == null)
				return result;

			if (String.IsNullOrEmpty(local.ChangePath))
				throw new InvalidOperationException("Source path is required for source control changes.");

			var comment = BuildComment(local);

			var items = m_paths
				.SelectMany(item => item.Value.Select(i => new KeyValuePair<string, string>(item.Key, i)))
				.Where(i => local.ChangePath.StartsWith(i.Value, StringComparison.OrdinalIgnoreCase))
				.Select(i => i.Key)
				.Distinct()
				.ToList();

			foreach (var item in items)
			{
				result.Add(new ItemModification
				{
					Item = item,
					Modification = new Modification
					{
						Code = local.ChangeId,
						Type = local.ChangeType,
						Author = local.ChangeAuthor,
						Item = local.ChangePath,
						Comment = comment,
						Date = local.ChangeDate.ToUniversalTime()
					}
				});
			}*/

			return result;
		}

		public bool ShouldIgnore(string item)
		{
			// we should postpone builds for the specified item, if some of their referenced items
			// are already in the build queue (i.e. current item will be triggered anyway after they build
			return m_pending.SelectMany(GetAllDependants).Contains(item);
		}
	}
}
