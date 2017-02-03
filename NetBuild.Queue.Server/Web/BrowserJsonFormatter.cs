using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetBuild.Queue.Server.Web
{
	/// <summary>
	/// Simple custom formatter that accepts text/html requests and returns application/json responses.
	/// </summary>
	public class BrowserJsonFormatter : JsonMediaTypeFormatter
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public BrowserJsonFormatter()
		{
			SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

			SerializerSettings.Formatting = Formatting.None;
			SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;
			SerializerSettings.NullValueHandling = NullValueHandling.Include;
			SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
			SerializerSettings.Error = (sender, args) => { throw args.ErrorContext.Error; };

			SerializerSettings.Converters.Add(new StringEnumConverter
			{
				AllowIntegerValues = false,
				CamelCaseText = true
			});
		}

		/// <summary>
		/// Sets the default headers for content that will be formatted using this formatter.
		/// </summary>
		public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
		{
			base.SetDefaultContentHeaders(type, headers, mediaType);
			headers.ContentType = new MediaTypeHeaderValue("application/json");
		}
	}
}
