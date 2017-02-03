using Newtonsoft.Json;

namespace NetBuild.Common
{
	/// <summary>
	/// API exception data object.
	/// </summary>
	public class ApiExceptionData
	{
		/// <summary>
		/// Gets or sets error code.
		/// </summary>
		[JsonProperty("code")]
		public int Code { get; set; }

		/// <summary>
		/// Gets or sets error message.
		/// </summary>
		[JsonProperty("message")]
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets error type.
		/// </summary>
		[JsonProperty("type")]
		public string Type { get; set; }

		/// <summary>
		/// Gets or sets debug information.
		/// </summary>
		[JsonProperty("debug")]
		public string Debug { get; set; }
	}
}
