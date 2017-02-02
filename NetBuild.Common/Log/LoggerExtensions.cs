using System;

namespace NetBuild.Common
{
	public static class LoggerExtensions
	{
		public static void LogDebug(this ILogger logger, string messageTemplate, params object[] propertyValues)
		{
			logger?.Log(LogLevel.Debug, null, messageTemplate, propertyValues);
		}

		public static void LogInformation(this ILogger logger, string messageTemplate, params object[] propertyValues)
		{
			logger?.Log(LogLevel.Information, null, messageTemplate, propertyValues);
		}

		public static void LogWarning(this ILogger logger, string messageTemplate, params object[] propertyValues)
		{
			logger?.Log(LogLevel.Warning, null, messageTemplate, propertyValues);
		}

		public static void LogWarning(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
		{
			logger?.Log(LogLevel.Warning, exception, messageTemplate, propertyValues);
		}

		public static void LogWarning(this ILogger logger, Exception exception)
		{
			logger?.Log(LogLevel.Warning, exception, "An Warning occurred.");
		}

		public static void LogError(this ILogger logger, string messageTemplate, params object[] propertyValues)
		{
			logger?.Log(LogLevel.Error, null, messageTemplate, propertyValues);
		}

		public static void LogError(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
		{
			logger?.Log(LogLevel.Error, exception, messageTemplate, propertyValues);
		}

		public static void LogError(this ILogger logger, Exception exception)
		{
			logger?.Log(LogLevel.Error, exception, "An error occurred.");
		}
	}
}
