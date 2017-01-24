using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	public class BuildCompleteSignal : ISignal
	{
		public string SignalType => "BuildComplete";

		[JsonProperty("item")]
		public string ProjectItem { get; set; }
	}
}
