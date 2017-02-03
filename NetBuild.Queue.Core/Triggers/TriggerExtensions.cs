using System;
using System.Collections.Generic;
using System.Linq;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Provide more convenient overloads for triggers.
	/// </summary>
	public static class TriggerExtensions
	{
		/// <summary>
		/// For a specified item, updates a complete set of its triggers of a given type.
		/// </summary>
		public static bool SetTriggers<T>(this ITriggerSetup setup, string item, IEnumerable<T> triggers) where T : ITrigger
		{
			var input = triggers.ToList();

			var types = input.Select(trigger => trigger.GetType()).Distinct().ToList();
			if (types.Count > 1)
				throw new InvalidOperationException($"Only triggers of the same type should be passed into this method, but {types.Count} different types were found: {String.Join(", ", types.Select(t => t.Name))}.");

			var type = types.FirstOrDefault();
			if (type == null)
			{
				// when we want to delete all existing triggers of a given type
				// we use a generic type which was used to call this method
				type = typeof(T);
			}

			return setup.Set(item, type, input.Cast<ITrigger>());
		}
	}
}
