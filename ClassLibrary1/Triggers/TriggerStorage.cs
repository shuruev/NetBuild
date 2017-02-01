using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Newtonsoft.Json;

namespace NetBuild.Queue.Engine
{
	public class TriggerStorage
	{
		private static readonly List<Type> s_knownTypes;

		private readonly string m_connectionString;
		private readonly int m_commandTimeoutInSeconds;

		private readonly JsonSerializerSettings m_settings;

		static TriggerStorage()
		{
			s_knownTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.GetInterfaces().Contains(typeof(ITrigger)))
				.ToList();
		}

		public TriggerStorage(string connectionString, TimeSpan commandTimeout)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			if (commandTimeout <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(commandTimeout));

			m_connectionString = connectionString;
			m_commandTimeoutInSeconds = Convert.ToInt32(commandTimeout.TotalSeconds);

			m_settings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};
		}

		public void Set(string item, string triggerType, IEnumerable<object> triggerValues)
		{
			var table = new DataTable();
			table.Columns.Add("TriggerValue", typeof(string));

			foreach (var trigger in triggerValues)
			{
				var row = table.NewRow();
				row["TriggerValue"] = JsonConvert.SerializeObject(trigger, m_settings);

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
						Trigger = Deserialize(i.TriggerType, i.TriggerValue)
					})
					.ToList();
			}
		}

		private ITrigger Deserialize(string triggerType, string triggerValue)
		{
			var type = s_knownTypes.FirstOrDefault(t => t.Name == triggerType);
			if (type == null)
				throw new InvalidOperationException($"Unknown trigger type '{triggerType}'.");

			return (ITrigger)JsonConvert.DeserializeObject(triggerValue, type, m_settings);
		}
	}
}
