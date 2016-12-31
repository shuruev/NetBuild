using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Works with queue database.
	/// </summary>
	public class QueueDb
	{
		protected readonly string m_connectionString;
		protected readonly int m_commandTimeoutInSeconds;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QueueDb(string connectionString, TimeSpan commandTimeout)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			if (commandTimeout <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(commandTimeout));

			m_connectionString = connectionString;
			m_commandTimeoutInSeconds = Convert.ToInt32(commandTimeout.TotalSeconds);
		}

		/// <summary>
		/// Creates specified item if it does not exist yet.
		/// </summary>
		public void SubmitItem(string itemCode)
		{
			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				conn.Execute(
					"Queue.SubmitItem",
					new { itemCode },
					null,
					m_commandTimeoutInSeconds,
					CommandType.StoredProcedure);
			}
		}

		/// <summary>
		/// For a specified item, updates a complete set of its triggers with a given type.
		/// </summary>
		public void SetTriggers(string itemCode, string triggerType, IEnumerable<string> triggerValues)
		{
			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				var table = new DataTable();
				table.Columns.Add("Value", typeof(string));

				foreach (var triggerValue in triggerValues)
				{
					table.Rows.Add(triggerValue);
				}

				conn.Execute(
					"Queue.SetTriggers",
					new { itemCode, triggerType, triggerValues = table },
					null,
					m_commandTimeoutInSeconds,
					CommandType.StoredProcedure);
			}
		}

		/// <summary>
		/// Checks whether specified item should be built, and returns all corresponding modifications related to this build.
		/// </summary>
		public List<BuildDto> ShouldBuild(string itemCode)
		{
			using (var conn = new SqlConnection(m_connectionString))
			{
				conn.Open();

				return conn.Query<BuildDto>(
					"Queue.ShouldBuild",
					new { itemCode },
					null,
					true,
					m_commandTimeoutInSeconds,
					CommandType.StoredProcedure)
					.ToList();
			}
		}
	}
}
