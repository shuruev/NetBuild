using System;

namespace NetBuild.Common
{
	public interface ILogger
	{
		/// <summary>
		/// Writes a log entry.
		/// </summary>
		void Log(LogLevel logLevel, Exception exception, string messageTemplate, params object[] propertyValues);
	}
}
