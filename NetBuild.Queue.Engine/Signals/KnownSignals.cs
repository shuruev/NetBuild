using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public static class KnownSignals
	{
		private static readonly List<Type> s_knownTypes;

		static KnownSignals()
		{
			s_knownTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.GetInterfaces().Contains(typeof(ISignal)))
				.ToList();
		}

		public static Type Get(string signalType)
		{
			var type = s_knownTypes.FirstOrDefault(t => t.Name == signalType);
			if (type == null)
				throw new InvalidOperationException($"Unknown signal type '{signalType}'.");

			return type;
		}
	}
}
