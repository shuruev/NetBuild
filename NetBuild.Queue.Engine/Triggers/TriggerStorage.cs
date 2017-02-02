using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public class TriggerStorage
	{
		private readonly string m_connectionString;
		private readonly int m_commandTimeoutInSeconds;

		public TriggerStorage(string connectionString, TimeSpan commandTimeout)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			if (commandTimeout <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(commandTimeout));

			m_connectionString = connectionString;
			m_commandTimeoutInSeconds = Convert.ToInt32(commandTimeout.TotalSeconds);
		}

		public List<ItemTrigger> Load()
		{
			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				return conn.Query(
					@"
SELECT
	BuildItem,
	TriggerType,
	TriggerValue
FROM Queue2.[Trigger]
",
					commandTimeout: m_commandTimeoutInSeconds)
					.Select(i => new ItemTrigger
					{
						Item = i.BuildItem,
						Trigger = (ITrigger)ObjectSerializer.Deserialize(KnownTriggers.Get(i.TriggerType), i.TriggerValue)
					})
					.ToList();
			}
		}

		public void Set(string item, string triggerType, IEnumerable<object> triggerValues)
		{
			var table = new DataTable();
			table.Columns.Add("TriggerValue", typeof(string));

			foreach (var trigger in triggerValues)
			{
				var row = table.NewRow();
				row["TriggerValue"] = ObjectSerializer.Serialize(trigger);

				table.Rows.Add(row);
			}

			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				conn.Execute(
					"CREATE TABLE #Trigger (TriggerValue NVARCHAR(MAX) NOT NULL)",
					commandTimeout: m_commandTimeoutInSeconds);

				using (var copy = new SqlBulkCopy(conn))
				{
					copy.BulkCopyTimeout = m_commandTimeoutInSeconds;
					copy.DestinationTableName = "#Trigger";
					copy.WriteToServer(table);
				}

				// add missing triggers to specified item
				conn.Execute(
					@"
INSERT INTO Queue2.[Trigger] (
	BuildItem,
	TriggerType,
	TriggerValue,
	TriggerDate)
SELECT
	@item,
	@triggerType,
	T.TriggerValue,
	GETUTCDATE()
FROM #Trigger T
	LEFT JOIN Queue2.[Trigger] QT
	ON QT.BuildItem = @item
		AND QT.TriggerType = @triggerType
		AND QT.TriggerValue = T.TriggerValue
WHERE
	QT.Id IS NULL
",
					new { item, triggerType },
					commandTimeout: m_commandTimeoutInSeconds);

				// remove unnecessary triggers from specified item
				conn.Execute(
					@"
DELETE QT
FROM Queue2.[Trigger] QT
	LEFT JOIN #Trigger T
	ON QT.TriggerValue = T.TriggerValue
WHERE
	QT.BuildItem = @item
	AND QT.TriggerType = @triggerType
	AND T.TriggerValue IS NULL
",
					new { item, triggerType },
					commandTimeout: m_commandTimeoutInSeconds);

				conn.Execute(
					"DROP TABLE #Trigger",
					commandTimeout: m_commandTimeoutInSeconds);
			}
		}
	}
}
