using System;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Outputs diagnostics messages directly to the console.
	/// </summary>
	public class ConsoleLog : ILog
	{
		/// <summary>
		/// Writes diagnostics message.
		/// </summary>
		public void Log(string message)
		{
			Console.WriteLine(message);
		}
	}
}
