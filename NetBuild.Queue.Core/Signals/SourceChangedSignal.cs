using System;
using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// A signal representing some change in source control files.
	/// </summary>
	public class SourceChangedSignal : ISignal
	{
		/// <summary>
		/// Gets or sets some ID representing the change.
		/// E.g. for TFS we can pass changeset ID here, like "11584".
		/// </summary>
		[JsonProperty("id")]
		public string ChangeId { get; set; }

		/// <summary>
		/// Gets or sets a path where the change occured.
		/// E.g. for TFS we can pass something like "$/Main/ContentCast/V3/V3.Storage/Client/V3Client.cs".
		/// </summary>
		[JsonProperty("path")]
		public string ChangePath { get; set; }

		/// <summary>
		/// Gets or sets change author (e.g. "Shuruev, Oleg").
		/// </summary>
		[JsonProperty("author")]
		public string ChangeAuthor { get; set; }

		/// <summary>
		/// Gets or sets change type (e.g. "add" or "edit, rename").
		/// </summary>
		[JsonProperty("type")]
		public string ChangeType { get; set; }

		/// <summary>
		/// Gets or sets change comment (e.g. "Implemented initial version").
		/// </summary>
		[JsonProperty("comment")]
		public string ChangeComment { get; set; }

		/// <summary>
		/// Gets or sets change date in UTC.
		/// </summary>
		[JsonProperty("date")]
		public DateTime ChangeDate { get; set; }
	}
}
