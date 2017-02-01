using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NetBuild.Common
{
	public static class Util
	{
		public static bool CompareLists(List<string> first, List<string> second)
		{
			if (first.Count != second.Count)
				return false;

			Func<List<string>, string> render = list => String.Join(
				Environment.NewLine,
				list.OrderBy(i => i));

			var data1 = render(first);
			var data2 = render(second);

			return data1 == data2;
		}

		public static bool CompareLists<T>(List<T> first, List<T> second)
		{
			Func<T, string> json = item => JsonConvert.SerializeObject(item);

			return CompareLists(
				first.Select(json).ToList(),
				second.Select(json).ToList());
		}
	}
}
