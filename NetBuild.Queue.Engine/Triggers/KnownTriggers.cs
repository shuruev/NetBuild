using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public static class KnownTriggers
	{
		private static readonly List<Type> s_knownTypes;

		static KnownTriggers()
		{
			s_knownTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.GetInterfaces().Contains(typeof(ITrigger)))
				.ToList();
		}

		public static Type Get(string triggerType)
		{
			var type = s_knownTypes.FirstOrDefault(t => t.Name == triggerType);
			if (type == null)
				throw new InvalidOperationException($"Unknown trigger type '{triggerType}'.");

			return type;
		}
	}
}
