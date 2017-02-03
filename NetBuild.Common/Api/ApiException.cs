using System.Net;

namespace NetBuild.Common
{
	/// <summary>
	/// Represents known API exception.
	/// </summary>
	public class ApiException : WebException
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ApiException(string message, WebException exception)
			: base(message, exception)
		{
		}
	}
}
