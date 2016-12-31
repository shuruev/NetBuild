using System;
using System.Text;

namespace NetBuild.Common
{
	/// <summary>
	/// Extension methods for various encryption operations.
	/// </summary>
	public static class EncryptorExtensions
	{
		/// <summary>
		/// Encrypts specified text using UTF-8 as its binary representation.
		/// </summary>
		public static string EncryptUtf8(this IEncryptor encryptor, string text)
		{
			var input = Encoding.UTF8.GetBytes(text);
			var output = encryptor.Encrypt(input);
			return Convert.ToBase64String(output);
		}

		/// <summary>
		/// Decrypts text from its specified UTF-8 binary representation.
		/// </summary>
		public static string DecryptUtf8(this IEncryptor encryptor, string text)
		{
			var input = Convert.FromBase64String(text);
			var output = encryptor.Decrypt(input);
			return Encoding.UTF8.GetString(output);
		}

		/// <summary>
		/// Encrypts specified data specified by its base-64 representation.
		/// </summary>
		public static string EncryptBase64(this IEncryptor encryptor, string text)
		{
			var input = Convert.FromBase64String(text);
			var output = encryptor.Encrypt(input);
			return Convert.ToBase64String(output);
		}

		/// <summary>
		/// Decrypts data from its specified base-64 representation.
		/// </summary>
		public static string DecryptBase64(this IEncryptor encryptor, string text)
		{
			var input = Convert.FromBase64String(text);
			var output = encryptor.Decrypt(input);
			return Convert.ToBase64String(output);
		}
	}
}
