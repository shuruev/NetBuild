﻿using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace NetBuild.Queue.Client
{
	public class ApiClient : RestClient
	{
		/// <summary>
		/// Gets or sets Json.NET serializer.
		/// </summary>
		protected JsonSerializer Serializer { get; set; }

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ApiClient(string baseUrl)
			: base(baseUrl)
		{
			Serializer = new JsonSerializer
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};
		}

		/// <summary>
		/// Executes HTTP request using JSON for serialization.
		/// </summary>
		protected T Execute<T>(ApiRequest request)
		{
			var response = ExecuteRequest(request);

			using (var stream = response.GetResponseStream())
			{
				using (var sr = new StreamReader(stream))
				{
					using (var jtr = new JsonTextReader(sr))
					{
						return Serializer.Deserialize<T>(jtr);
					}
				}
			}
		}

		/// <summary>
		/// Executes HTTP request using JSON for serialization.
		/// </summary>
		protected void Execute(ApiRequest request)
		{
			ExecuteRequest(request);
		}

		/// <summary>
		/// Creates new GET request.
		/// </summary>
		protected ApiRequest HttpGet(string path)
		{
			return new ApiRequest
			{
				Serializer = Serializer,
				BaseUrl = m_baseUrl,
				Method = HttpMethod.Get,
				Path = path
			};
		}
	}
}
