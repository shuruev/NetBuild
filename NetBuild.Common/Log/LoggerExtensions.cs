using System;

namespace NetBuild.Common
{
	public static class LoggerExtensions
	{
		public static void LogDebug(this ILogger logger, string messageTemplate, params object[] propertyValues)
		{
			if (logger == null)
				return;

			logger.Log(LogLevel.Debug, null, messageTemplate, propertyValues);
		}

		public static void LogError(this ILogger logger, string messageTemplate, params object[] propertyValues)
		{
			if (logger == null)
				return;

			logger.Log(LogLevel.Error, null, messageTemplate, propertyValues);
		}

		public static void LogError(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
		{
			if (logger == null)
				return;

			logger.Log(LogLevel.Error, exception, messageTemplate, propertyValues);
		}

		public static void LogError(this ILogger logger, Exception exception)
		{
			if (logger == null)
				return;

			logger.Log(LogLevel.Error, exception, "An error occurred.");
		}
	}
}
