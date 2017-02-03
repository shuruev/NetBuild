using System;
using System.Collections.Generic;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Contains trigger setting method, in order to provide more convenient overload via extensions.
	/// </summary>
	public interface ITriggerSetup
	{
		/// <summary>
		/// For a specified item, updates a complete set of its triggers of a given type.
		/// </summary>
		bool Set(string item, Type type, IEnumerable<ITrigger> triggers);
	}
}
