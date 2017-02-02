using System;
using Serilog.Core;

namespace NetBuild.Common
{
	/// <summary>
	/// Adapts internal logger to Serilog.
	/// </summary>
	public class SerilogAdapter : ILogger
	{
		private readonly Logger m_logger;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public SerilogAdapter(Logger logger)
		{
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			m_logger = logger;
		}

		/// <summary>
		/// Writes a log entry.
		/// </summary>
		public void Log(LogLevel logLevel, Exception exception, string messageTemplate, params object[] propertyValues)
		{
			switch (logLevel)
			{
				case LogLevel.Debug:
					m_logger.Debug(exception, messageTemplate, propertyValues);
					break;

				case LogLevel.Information:
					m_logger.Information(exception, messageTemplate, propertyValues);
					break;

				case LogLevel.Warning:
					m_logger.Warning(exception, messageTemplate, propertyValues);
					break;

				case LogLevel.Error:
					m_logger.Error(exception, messageTemplate, propertyValues);
					break;
			}
		}
	}
}
