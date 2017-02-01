using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Common;

namespace NetBuild.Queue.Engine
{
	public class Triggers
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

		public bool Set<T>(string item, IEnumerable<T> triggers) where T : ITrigger
		{
			var input = triggers.ToList();

			var types = input.Select(trigger => trigger.GetType().Name).Distinct().ToList();
			if (types.Count > 1)
				throw new InvalidOperationException($"Only triggers of the same type should be passed into this method, but {types.Count} different types were found: {String.Join(", ", types)}.");

			var type = types.FirstOrDefault();
			if (type == null)
			{
				// when we want to delete all existing triggers of a given type
				// we use a generic type which was used to call this method
				type = typeof(T).Name;
			}

			// check if any modifications needed
			lock (m_all)
			{
				var existing = GetInternal<T>(item);
				if (Util.CompareLists(existing, input))
					return false;
			}

			// set triggers in the storage
			lock (m_storage)
			{
				m_storage.Set(item, type, input.Cast<object>());
			}

			// set triggers within in-memory collection
			lock (m_all)
			{
				SetInternal(item, typeof(T), input.Cast<ITrigger>());
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

		private List<T> GetInternal<T>(string item) where T : ITrigger
		{
			List<ITrigger> list;
			if (!m_all.TryGetValue(item, out list))
				return new List<T>();

			return list
				.Where(i => i.GetType() == typeof(T))
				.Cast<T>()
				.ToList();
		}
	}
}
