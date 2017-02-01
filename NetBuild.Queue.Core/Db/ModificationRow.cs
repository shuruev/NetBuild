using System;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Represents a single modification which leads to the build.
	/// </summary>
	public class ModificationRow
	{
		/// <summary>
		/// Gets or sets internal modification ID.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Gets or sets human friendly modification code or ID.
		/// E.g. for TFS we may store changeset ID here, like "11584".
		/// </summary>
		public string ModificationCode { get; set; }

		/// <summary>
		/// Gets or sets modification type (e.g. "edit").
		/// </summary>
		public string ModificationType { get; set; }

		/// <summary>
		/// Gets or sets modification author (e.g. "Shuruev, Oleg").
		/// </summary>
		public string ModificationAuthor { get; set; }

		/// <summary>
		/// Gets or sets modification item.
		/// E.g. for TFS we may store a source path, like "$/Main/ContentCast/V3/V3.Storage/Client/V3Client.cs".
		/// </summary>
		public string ModificationItem { get; set; }

		/// <summary>
		/// Gets or sets modification comment (e.g. "Implemented initial version").
		/// </summary>
		public string ModificationComment { get; set; }

		/// <summary>
		/// Gets or sets external modification date in UTC, which is calculated by a signal detector.
		/// </summary>
		public DateTime? ModificationDate { get; set; }

		/// <summary>
		/// Gets or sets internal modification date in UTC, which is calculated in the database.
		/// </summary>
		public DateTime Created { get; set; }
	}
}
