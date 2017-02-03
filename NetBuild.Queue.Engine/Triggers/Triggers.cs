using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public class Triggers : ITriggerSetup
	{
		private readonly Dictionary<string, List<ITrigger>> m_all;
		private readonly TriggerStorage m_storage;

		public Triggers(TriggerStorage storage)
		{
			if (storage == null)
				throw new ArgumentNullException(nameof(storage));

			m_all = new Dictionary<string, List<ITrigger>>();
			m_storage = storage;
		}

		public List<ItemTrigger> Load()
		{
			lock (m_storage)
			{
				// load all existing data from storage
				var all = m_storage.Load();

				// update in-memory collection
				foreach (var item in all.GroupBy(i => i.Item))
				{
					foreach (var type in item.GroupBy(i => i.Trigger.GetType()))
					{
						SetInternal(item.Key, type.Key, type.Select(i => i.Trigger));
					}
				}

				// returns all the data so external detectors could use them as well
				return all;
			}
		}

		public bool Set(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			var input = triggers.ToList();

			// check if any modifications needed
			lock (m_all)
			{
				var existing = GetInternal(item, type);
				if (Util.CompareLists(existing, input))
					return false;
			}

			// set triggers in the storage
			lock (m_storage)
			{
				m_storage.Set(item, type.Name, input);
			}

			// set triggers within in-memory collection
			lock (m_all)
			{
				SetInternal(item, type, input);
			}

			return true;
		}

		private void SetInternal(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			List<ITrigger> list;
			if (!m_all.TryGetValue(item, out list))
			{
				list = new List<ITrigger>();
				m_all.Add(item, list);
			}

			list.RemoveAll(i => i.GetType() == type);
			list.AddRange(triggers);
		}

		private List<ITrigger> GetInternal(string item, Type type)
		{
			List<ITrigger> list;
			if (!m_all.TryGetValue(item, out list))
				return new List<ITrigger>();

			return list
				.Where(i => i.GetType() == type)
				.ToList();
		}
	}
}
