using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	public class SourcePathTrigger : ITrigger
	{
		public string TriggerType => "SourcePath";

		[JsonProperty("path")]
		public string SourcePath { get; set; }
	}
}
