using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Filters;
using NetBuild.Common;
using NetBuild.Queue.Engine;
using Newtonsoft.Json;

namespace NetBuild.Queue.Server.Web
{
	/// <summary>
	/// Filters exceptions raised while serving HTTP requests.
	/// </summary>
	public class ExceptionHandlingAttribute : ExceptionFilterAttribute
	{
		private readonly JsonSerializerSettings m_settings;
		private readonly Serilog.ILogger m_logger;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ExceptionHandlingAttribute(Serilog.ILogger logger)
		{
			m_settings = new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};

			m_logger = logger;
		}

		/// <summary>
		/// Raises the exception event.
		/// </summary>
		public override void OnException(HttpActionExecutedContext context)
		{
			var exception = context.Exception;
			var data = new ApiExceptionData
			{
				Code = GetHttpCode(exception),
				Message = exception.Message,
				Type = exception.GetType().Name
			};

			if (context.ActionContext.RequestContext.IncludeErrorDetail)
			{
				data.Debug = exception.ToString();
			}

			var json = JsonConvert.SerializeObject(data, Formatting.None, m_settings);
			var message = new HttpResponseMessage((HttpStatusCode)data.Code)
			{
				Content = new StringContent(json, Encoding.UTF8, "application/json")
			};

			if (data.Code == 500)
				SendLog(context, exception);

			throw new HttpResponseException(message);
		}

		/// <summary>
		/// Sends log message.
		/// </summary>
		private void SendLog(HttpActionExecutedContext context, Exception exception)
		{
			var request = context.Request.GetOwinContext().Request;

			m_logger.Error(
				exception,
				"An error occured while processing {RequestUrl}: {ErrorMessage}",
				request.Uri.AbsoluteUri,
				exception.Message);
		}

		/// <summary>
		/// Gets HTTP code for server response.
		/// </summary>
		private int GetHttpCode(Exception exception)
		{
			if (exception is QueueEngineException)
				return 400;

			return 500;
		}
	}
}
