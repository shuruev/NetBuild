using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.Framework.Server;

namespace TfsPlugin.NetBuildQueue
{
	/// <summary>
	/// Small static helper for easier logging.
	/// </summary>
	public static class Log
	{
		private const int c_eventId = 2505;

		/// <summary>
		/// Gets or sets plugin name.
		/// </summary>
		public static string Name { get; set; } = "UNKNOWN";

		/// <summary>
		/// Logs debug message, only if debug mode is enabled.
		/// </summary>
		public static void Debug(string message)
		{
			if (!Config.DebugMode)
				return;

			Info(message);
		}

		/// <summary>
		/// Logs information message.
		/// </summary>
		public static void Info(string message)
		{
			TeamFoundationApplicationCore.Log($"{Name}: {message}", c_eventId, EventLogEntryType.Information);
		}

		/// <summary>
		/// Logs error.
		/// </summary>
		public static void Error(Exception error)
		{
			TeamFoundationApplicationCore.LogException($"An error occured in {Name}:", error);
		}
	}
}
