using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetBuild.Common
{
	public static class ObjectSerializer
	{
		private static readonly JsonSerializer s_serializer;

		static ObjectSerializer()
		{
			s_serializer = new JsonSerializer
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};
		}

		public static string Serialize(object obj)
		{
			var sb = new StringBuilder();

			using (var tw = new StringWriter(sb))
			{
				s_serializer.Serialize(tw, obj);
			}

			return sb.ToString();
		}

		public static object Deserialize(Type type, string json)
		{
			using (var sr = new StringReader(json))
			{
				return s_serializer.Deserialize(sr, type);
			}
		}

		public static object Deserialize(Type type, JObject json)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			return json.ToObject(type, s_serializer);
		}
	}
}
