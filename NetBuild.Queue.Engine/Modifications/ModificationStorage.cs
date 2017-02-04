using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public class ModificationStorage
	{
		private readonly string m_connectionString;
		private readonly int m_commandTimeoutInSeconds;

		public ModificationStorage(string connectionString, TimeSpan commandTimeout)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			if (commandTimeout <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(commandTimeout));

			m_connectionString = connectionString;
			m_commandTimeoutInSeconds = Convert.ToInt32(commandTimeout.TotalSeconds);
		}

		public List<ItemModification> Load()
		{
			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				return conn.Query(
					@"
SELECT
	BuildItem,
	ModificationCode,
	ModificationType,
	ModificationAuthor,
	ModificationItem,
	ModificationComment,
	ModificationDate
FROM [Queue].Modification
",
					commandTimeout: m_commandTimeoutInSeconds)
					.Select(i => new ItemModification
					{
						Item = i.BuildItem,
						Modification = new Modification
						{
							Code = i.ModificationCode,
							Type = i.ModificationType,
							Author = i.ModificationAuthor,
							Item = i.ModificationItem,
							Comment = i.ModificationComment,
							Date = i.ModificationDate
						}
					})
					.ToList();
			}
		}

		public void Save(IEnumerable<ItemModification> modifications)
		{
			var table = new DataTable();

			table.Columns.Add("BuildItem", typeof(string));
			table.Columns.Add("ModificationCode", typeof(string));
			table.Columns.Add("ModificationType", typeof(string));
			table.Columns.Add("ModificationAuthor", typeof(string));
			table.Columns.Add("ModificationItem", typeof(string));
			table.Columns.Add("ModificationComment", typeof(string));
			table.Columns.Add("ModificationDate", typeof(DateTime));

			foreach (var modification in modifications)
			{
				var row = table.NewRow();

				row["BuildItem"] = modification.Item;
				row["ModificationCode"] = modification.Modification.Code;
				row["ModificationType"] = modification.Modification.Type;
				row["ModificationAuthor"] = modification.Modification.Author;
				row["ModificationItem"] = modification.Modification.Item;
				row["ModificationComment"] = modification.Modification.Comment;
				row["ModificationDate"] = modification.Modification.Date;

				table.Rows.Add(row);
			}

			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				using (var copy = new SqlBulkCopy(conn))
				{
					copy.DestinationTableName = "Queue.Modification";
					copy.ColumnMappings.Add("BuildItem", "BuildItem");
					copy.ColumnMappings.Add("ModificationCode", "ModificationCode");
					copy.ColumnMappings.Add("ModificationType", "ModificationType");
					copy.ColumnMappings.Add("ModificationAuthor", "ModificationAuthor");
					copy.ColumnMappings.Add("ModificationItem", "ModificationItem");
					copy.ColumnMappings.Add("ModificationComment", "ModificationComment");
					copy.ColumnMappings.Add("ModificationDate", "ModificationDate");
					copy.WriteToServer(table);
				}
			}
		}

		public void Reserve(string item, string label)
		{
			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				conn.Execute(
					@"
UPDATE [Queue].Modification
SET BuildLabel = @label
WHERE BuildItem = @item

INSERT INTO [Queue].Build (Item, [Action], Label)
VALUES (@item, 'Start', @label)
",
					new { item, label },
					commandTimeout: m_commandTimeoutInSeconds);
			}
		}

		public void Release(string item, string label)
		{
			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				conn.Execute(
					@"
DELETE FROM [Queue].Modification
WHERE
	BuildItem = @item
	AND BuildLabel IS NOT NULL

INSERT INTO [Queue].Build (Item, [Action], Label)
VALUES (@item, 'Complete', @label)
",
					new { item, label },
					commandTimeout: m_commandTimeoutInSeconds);
			}
		}
	}
}
