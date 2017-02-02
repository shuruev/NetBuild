using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// A signal which is sent after build is complete, so other build items could react.
	/// </summary>
	public class BuildCompleteSignal : ISignal
	{
		/// <summary>
		/// Gets or sets an item which was built.
		/// </summary>
		[JsonProperty("item")]
		public string BuildItem { get; set; }
	}
}
