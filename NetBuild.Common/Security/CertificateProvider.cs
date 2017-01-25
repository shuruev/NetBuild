using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace NetBuild.Common
{
	/// <summary>
	/// Helps finding the required certificate.
	/// </summary>
	public class CertificateProvider
	{
		private readonly X509Store m_store;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CertificateProvider(X509Store store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			m_store = store;
		}

		/// <summary>
		/// Tries to find certificate by specified thumbprint.
		/// </summary>
		private X509Certificate2 Find(string thumbprint, bool validByTime, bool validBySignature)
		{
			try
			{
				m_store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

				var found = m_store.Certificates;

				if (validByTime)
					found = found.Find(X509FindType.FindByTimeValid, DateTime.Now, validBySignature);

				found = found.Find(X509FindType.FindByThumbprint, thumbprint, validBySignature);

				return found.Cast<X509Certificate2>().FirstOrDefault();
			}
			finally
			{
				m_store.Close();
			}
		}

		/// <summary>
		/// Tries to find certificate by specified thumbprint, which should be valid by time but not necessarily
		/// having valid signature (i.e. may be not trusted or self-signed). Also the returned certificate is
		/// required to have a private key.
		/// </summary>
		public X509Certificate2 ForLocalEncryption(string thumbprint)
		{
			var certificate = Find(thumbprint, true, false);

			if (certificate == null)
				throw new InvalidOperationException($"Cannot find the required certificate '{thumbprint}'.");

			if (!certificate.HasPrivateKey)
				throw new InvalidOperationException($"The required certificate '{thumbprint}' has no private key.");

			return certificate;
		}
	}
}
