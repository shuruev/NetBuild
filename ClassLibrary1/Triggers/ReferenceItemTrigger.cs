using Newtonsoft.Json;

namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// A trigger which can issue builds based on projects dependencies.
	/// </summary>
	public class ReferenceItemTrigger : ITrigger
	{
		/// <summary>
		/// Gets or sets referenced build item (e.g. 'V3.Storage').
		/// </summary>
		[JsonProperty("item")]
		public string ReferenceItem { get; set; }
	}
}
