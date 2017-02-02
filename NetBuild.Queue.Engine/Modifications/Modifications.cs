using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public class Modifications
	{
		private readonly Dictionary<string, List<Modification>> m_all;
		private readonly Dictionary<string, List<Modification>> m_reserved;

		private readonly ModificationStorage m_storage;

		public Modifications(ModificationStorage storage)
		{
			if (storage == null)
				throw new ArgumentNullException(nameof(storage));

			m_all = new Dictionary<string, List<Modification>>();
			m_reserved = new Dictionary<string, List<Modification>>();

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
			lock (m_all)
			{
				return GetInternal(item);
			}
		}

		public void Reserve(string item)
		{
			// update modifications in the storage
			lock (m_storage)
			{
				m_storage.Reserve(item);
			}

			// update modifications within in-memory collection
			lock (m_all)
			{
				ReserveInternal(item);
			}
		}

		public void Release(string item)
		{
			// update modifications in the storage
			lock (m_storage)
			{
				m_storage.Release(item);
			}

			// update modifications within in-memory collection
			lock (m_all)
			{
				ReleaseInternal(item);
			}
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

		private List<Modification> GetInternal(string item)
		{
			List<Modification> list;
			if (!m_all.TryGetValue(item, out list))
				return new List<Modification>();

			return list.ToList();
		}

		private void ReserveInternal(string item)
		{
			m_reserved[item] = GetInternal(item);
		}

		private void ReleaseInternal(string item)
		{
			List<Modification> reserved;
			if (!m_reserved.TryGetValue(item, out reserved))
				return;

			List<Modification> current;
			if (!m_all.TryGetValue(item, out current))
				return;

			foreach (var modification in reserved)
			{
				current.Remove(modification);
			}

			m_reserved.Remove(item);
		}
	}
}
