using System;
using System.Collections.Generic;
using System.Linq;

namespace NetBuild.Queue.Engine
{
	public class Modifications
	{
		private readonly Dictionary<string, List<Modification>> m_all;
		private readonly ModificationStorage m_storage;

		public Modifications(ModificationStorage storage)
		{
			if (storage == null)
				throw new ArgumentNullException(nameof(storage));

			m_all = new Dictionary<string, List<Modification>>();
			m_storage = storage;
		}

		public List<ItemModification> Load()
		{
			lock (m_storage)
			{
				// load all existing data from storage
				var all = m_storage.Load();

				// update in-memory collection
				AddInternal(all);

				// returns all the data so external detectors could use them as well
				return all;
			}
		}

		public void Add(IEnumerable<ItemModification> modifications)
		{
			var items = modifications.ToList();
			if (items.Count == 0)
				return;

			// add new modifications to the storage
			lock (m_storage)
			{
				m_storage.Save(items);
			}

			// add new modifications to in-memory collection
			lock (m_all)
			{
				AddInternal(items);
			}
		}

		public List<Modification> Get(string item)
		{
			List<Modification> list;
			lock (m_all)
			{
				if (!m_all.TryGetValue(item, out list))
					return new List<Modification>();
			}

			return list.ToList();
		}

		private void AddInternal(IEnumerable<ItemModification> modifications)
		{
			foreach (var group in modifications.GroupBy(i => i.Item))
			{
				List<Modification> list;
				if (!m_all.TryGetValue(group.Key, out list))
				{
					list = new List<Modification>();
					m_all.Add(group.Key, list);
				}

				list.AddRange(group.Select(i => i.Modification));
			}
		}
	}
}
