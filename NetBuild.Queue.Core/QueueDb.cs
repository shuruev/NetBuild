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
		/// Simple logging instance to access diagnostics messages.
		/// </summary>
		public ILog Log { get; set; }

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
			using (var conn = Open())
			{
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
			using (var conn = Open())
			{
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
		/// Processes any external signal, which potentially can trigger new builds.
		/// </summary>
		public void ProcessSignal(string signalType, string signalValue)
		{
			using (var conn = Open())
			{
				conn.Execute(
					"Queue.ProcessSignal",
					new { signalType, signalValue },
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
			using (var conn = Open())
			{
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

		/// <summary>
		/// Starts build process for specified item, marking all the current modifications with specified reserve code.
		/// </summary>
		public void StartBuild(string itemCode, string reserveCode)
		{
			using (var conn = Open())
			{
				conn.Execute(
					"Queue.StartBuild",
					new { itemCode, reserveCode },
					null,
					m_commandTimeoutInSeconds,
					CommandType.StoredProcedure);
			}
		}

		/// <summary>
		/// Marks specified build as completed, removing all corresponding modifications and updating related timestamps.
		/// </summary>
		public void CompleteBuild(string itemCode, string reserveCode)
		{
			using (var conn = Open())
			{
				conn.Execute(
					"Queue.CompleteBuild",
					new { itemCode, reserveCode },
					null,
					m_commandTimeoutInSeconds,
					CommandType.StoredProcedure);
			}
		}

		/// <summary>
		/// Creates and opens new SQL connection, attaching logging instance if needed.
		/// </summary>
		private SqlConnection Open()
		{
			var conn = new SqlConnection(m_connectionString);
			conn.Open();

			if (Log != null)
				conn.InfoMessage += InfoMessage;

			return conn;
		}

		/// <summary>
		/// Writes diagnostics message from SQL server.
		/// </summary>
		private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			Log.Log(e.Message);
		}
	}
}
