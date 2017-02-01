using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// A trigger which can issue builds based on source control changes.
	/// </summary>
	public class SourcePathTrigger : ITrigger
	{
		/// <summary>
		/// Gets trigger type.
		/// </summary>
		public string TriggerType => "SourcePath";

		/// <summary>
		/// Gets or sets source control path (e.g. '"path": "$/Main/ContentCast/V3/V3.Storage/Client/V3Client.cs"').
		/// </summary>
		[JsonProperty("path")]
		public string SourcePath { get; set; }
	}
}
