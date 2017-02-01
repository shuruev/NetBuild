using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetBuild.Common;
using Serilog.Core;

namespace NetBuild.Queue.Debug
{
	public class SerilogAdapter : ILogger
	{
		private readonly Logger m_logger;

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
