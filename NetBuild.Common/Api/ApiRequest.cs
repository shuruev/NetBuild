using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace NetBuild.Common
{
	public class ApiRequest : IRestRequest
	{
		/// <summary>
		/// Gets or sets request method.
		/// </summary>
		public HttpMethod Method { get; set; }

		/// <summary>
		/// Gets or sets base URL.
		/// </summary>
		public string BaseUrl { get; set; }

		/// <summary>
		/// Gets or sets request path.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets request parameters.
		/// </summary>
		public NameValueCollection Parameters { get; set; }

		/// <summary>
		/// Gets a method used to populate request body.
		/// </summary>
		public Action<Stream> WriteBody { get; set; }

		/// <summary>
		/// Gets or sets Json.NET serializer.
		/// </summary>
		public JsonSerializer Serializer { get; set; }

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ApiRequest()
		{
			Parameters = new NameValueCollection();
		}

		/// <summary>
		/// Gets request URL.
		/// </summary>
		public string Url
		{
			get
			{
				var sb = new StringBuilder();

				sb.Append(BaseUrl.TrimEnd('/'));
				sb.Append('/');
				sb.Append(Path.Trim('/'));

				if (Parameters.Count > 0)
				{
					sb.Append("?");
					sb.Append(
						String.Join(
							"&",
							Parameters.AllKeys.Select(key => $"{key}={WebUtility.UrlEncode(Parameters[key])}")));
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// Allows to setup any custom properties for native HTTP request.
		/// </summary>
		public void SetRequestProperties(HttpWebRequest request)
		{
			request.ContentType = "application/json; charset=utf-8";
			request.Accept = "application/json";
		}

		/// <summary>
		/// Sets body for the request using JSON serializer.
		/// </summary>
		public ApiRequest WithBody(object data)
		{
			WriteBody = stream =>
			{
				using (var sw = new StreamWriter(stream))
				{
					using (var jtw = new JsonTextWriter(sw))
					{
						Serializer.Serialize(jtw, data);
					}
				}
			};

			return this;
		}

		/// <summary>
		/// Sets empty body for POST requests.
		/// </summary>
		public ApiRequest WithEmptyBody()
		{
			WriteBody = stream =>
			{
				using (var sw = new StreamWriter(stream))
				{
					sw.Write(String.Empty);
				}
			};

			return this;
		}

		/// <summary>
		/// Adds new required parameter for HTTP query.
		/// Throws exception if parameter value is null or empty.
		/// </summary>
		public ApiRequest WithParam(string name, string value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			if (String.IsNullOrEmpty(value))
				throw new ArgumentNullException(nameof(value));

			Parameters.Add(name, value);
			return this;
		}

		/// <summary>
		/// Adds new required parameter for HTTP query.
		/// Throws exception if parameter value is null or empty.
		/// </summary>
		public ApiRequest WithParam(string name, object value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return WithParam(name, value.ToString());
		}

		/// <summary>
		/// Adds new optional parameter for HTTP query.
		/// Does not add parameter if its value is null or empty.
		/// </summary>
		public ApiRequest WithOptionalParam(string name, string value)
		{
			if (String.IsNullOrEmpty(value))
				return this;

			return WithParam(name, value);
		}

		/// <summary>
		/// Adds new optional parameter for HTTP query.
		/// Does not add parameter if its value is null or empty.
		/// </summary>
		public ApiRequest WithOptionalParam(string name, object value)
		{
			if (value == null)
				return this;

			return WithOptionalParam(name, value.ToString());
		}
	}
}
