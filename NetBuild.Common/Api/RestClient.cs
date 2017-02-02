using System;
using System.IO;
using System.Net;

namespace NetBuild.Common
{
	public abstract class RestClient
	{
		/// <summary>
		/// Base URL for building all the queries.
		/// </summary>
		protected readonly string m_baseUrl;

		/// <summary>
		/// Gets or sets request timeout.
		/// Default is 30 seconds.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Gets or sets logging instance.
		/// </summary>
		public ILogger Logger { get; set; }

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		protected RestClient(string baseUrl)
		{
			if (String.IsNullOrEmpty(baseUrl))
				throw new ArgumentNullException(nameof(baseUrl));

			m_baseUrl = baseUrl;

			Timeout = TimeSpan.FromSeconds(30);
		}

		/// <summary>
		/// Executes HTTP request and returns response for the further processing.
		/// </summary>
		protected HttpWebResponse ExecuteRequest(IRestRequest query)
		{
			var request = WebRequest.CreateHttp(query.Url);
			request.Method = query.Method.Method;
			request.Timeout = Convert.ToInt32(Timeout.TotalMilliseconds);

			try
			{
				if (query.WriteBody != null)
				{
					using (var stream = request.GetRequestStream())
					{
						query.WriteBody(stream);
					}
				}

				query.SetRequestProperties(request);

				return (HttpWebResponse)request.GetResponse();
			}
			catch (WebException e) when (e.Response is HttpWebResponse)
			{
				var response = (HttpWebResponse)e.Response;
				var content = ReadResponseContent(response);

				if (IsKnownError(response.StatusCode, content, response))
					return response;

				Logger.LogError(
					e,
					"An error occured while calling {RequestUrl}: returned {StatusCode}\r\n{ResponseContent}",
					request.RequestUri,
					$"{(int)response.StatusCode} {response.StatusCode}",
					content);

				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(
					e,
					"An error occured while calling {RequestUrl}: {ErrorMessage}",
					request.RequestUri,
					e.Message);

				throw;
			}
		}

		/// <summary>
		/// Checks whether specified response is known and should not throw exceptions.
		/// </summary>
		protected virtual bool IsKnownError(HttpStatusCode code, string content, HttpWebResponse response)
		{
			return false;
		}

		/// <summary>
		/// Safely reads response content as a string.
		/// </summary>
		private static string ReadResponseContent(HttpWebResponse response)
		{
			try
			{
				using (var stream = response.GetResponseStream())
				{
					using (var sr = new StreamReader(stream))
					{
						return sr.ReadToEnd();
					}
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
