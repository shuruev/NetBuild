using Newtonsoft.Json;

namespace NetBuild.Queue.Engine
{
	public class BuildCompleteSignal : ISignal
	{
		[JsonProperty("item")]
		public string BuildItem { get; set; }
	}
}
