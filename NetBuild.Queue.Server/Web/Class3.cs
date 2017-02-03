using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace NetBuild.Queue.Server.Web
{
	public class Class3 : DelegatingHandler
	{
		private readonly ILogger m_logger;

		public Class3(ILogger logger)
		{
			m_logger = logger;
		}

		/// <summary>
		/// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
		/// </summary>
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var correlationId = $"{DateTime.Now.Ticks}{Thread.CurrentThread.ManagedThreadId}";
			var requestInfo = $"{request.Method} {request.RequestUri}";

			var requestMessage = await request.Content.ReadAsByteArrayAsync();

			await IncomingMessageAsync(correlationId, requestInfo, requestMessage);

			var response = await base.SendAsync(request, cancellationToken);

			byte[] responseMessage;

			if (response.IsSuccessStatusCode)
				responseMessage = await response.Content.ReadAsByteArrayAsync();
			else
				responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

			await OutgoingMessageAsync(correlationId, requestInfo, responseMessage);

			return response;
		}

		protected async Task IncomingMessageAsync(string correlationId, string requestInfo, byte[] message)
		{
			await Task.Run(() =>
				m_logger.Debug($"{correlationId} - Request: {requestInfo}\r\n{Encoding.UTF8.GetString(message)}"));
		}

		protected async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
		{
			await Task.Run(() =>
				m_logger.Debug($"{correlationId} - Response: {requestInfo}\r\n{Encoding.UTF8.GetString(message)}"));
		}
	}
}
