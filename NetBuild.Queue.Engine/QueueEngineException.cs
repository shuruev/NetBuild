using System;

namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// Generic error which can occur during queue engine execution.
	/// </summary>
	public class QueueEngineException : Exception
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QueueEngineException(string message)
			: base(message)
		{
		}
	}
}
