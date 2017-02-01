using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// A trigger which can issue builds based on projects dependencies.
	/// </summary>
	public class ReferenceItemTrigger : ITrigger
	{
		/// <summary>
		/// Gets trigger type.
		/// </summary>
		public string TriggerType => "ReferenceItem";

		/// <summary>
		/// Gets or sets referenced project item (e.g. 'V3.Storage').
		/// </summary>
		[JsonProperty("item")]
		public string ProjectItem { get; set; }
	}
}
