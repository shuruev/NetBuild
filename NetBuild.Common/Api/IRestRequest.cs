using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace NetBuild.Common
{
	/// <summary>
	/// Base interface responsible for building the entire HTTP request.
	/// Can be overridden to introduce customized request behavior,
	/// such as JSON support or HTML form submission parameters.
	/// </summary>
	public interface IRestRequest
	{
		/// <summary>
		/// Gets request method.
		/// </summary>
		HttpMethod Method { get; }

		/// <summary>
		/// Gets request URL.
		/// </summary>
		string Url { get; }

		/// <summary>
		/// Gets a method used to populate request body.
		/// </summary>
		Action<Stream> WriteBody { get; set; }

		/// <summary>
		/// Allows to setup any custom properties for native HTTP request.
		/// </summary>
		void SetRequestProperties(HttpWebRequest request);
	}
}
