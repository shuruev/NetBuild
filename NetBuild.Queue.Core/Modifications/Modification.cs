using System;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Represents a single modification which leads to the build.
	/// </summary>
	public class Modification
	{
		/// <summary>
		/// Gets or sets human friendly modification code or ID.
		/// E.g. for TFS we may store changeset ID here, like "11584".
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Gets or sets modification type (e.g. "add" or "edit, rename").
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Gets or sets modification author (e.g. "Shuruev, Oleg").
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Gets or sets modification item.
		/// E.g. for TFS we may store a source path, like "$/Main/ContentCast/V3/V3.Storage/Client/V3Client.cs".
		/// </summary>
		public string Item { get; set; }

		/// <summary>
		/// Gets or sets modification comment (e.g. "Implemented initial version").
		/// </summary>
		public string Comment { get; set; }

		/// <summary>
		/// Gets or sets external modification date in UTC, which is calculated by a signal detector.
		/// </summary>
		public DateTime Date { get; set; }
	}
}
