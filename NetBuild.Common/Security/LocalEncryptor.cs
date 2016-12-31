using System.Security.Cryptography.X509Certificates;

namespace NetBuild.Common
{
	/// <summary>
	/// Helps storing sensitive data in a secure way.
	/// </summary>
	public class LocalEncryptor : CertificateEncryptor
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public LocalEncryptor(string thumbprint)
			: base(new CertificateProvider(new X509Store(StoreLocation.CurrentUser)).ForLocalEncryption(thumbprint))
		{
		}
	}
}
