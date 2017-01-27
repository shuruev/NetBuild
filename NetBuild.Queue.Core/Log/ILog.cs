namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Simple logging interface to access diagnostics messages.
	/// </summary>
	public interface ILog
	{
		/// <summary>
		/// Writes diagnostics message.
		/// </summary>
		void Log(string message);
	}
}
