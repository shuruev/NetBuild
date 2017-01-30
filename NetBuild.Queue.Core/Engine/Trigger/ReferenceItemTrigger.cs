using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	public class ReferenceItemTrigger : ITrigger
	{
		public string TriggerType => "ReferenceItem";

		[JsonProperty("item")]
		public string ProjectItem { get; set; }
	}
}
