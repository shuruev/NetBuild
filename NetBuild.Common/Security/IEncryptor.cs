namespace NetBuild.Common
{
	/// <summary>
	/// Describes basic encryption interface.
	/// </summary>
	public interface IEncryptor
	{
		/// <summary>
		/// Encrypts specified data.
		/// </summary>
		byte[] Encrypt(byte[] data);

		/// <summary>
		/// Decrypts specified data.
		/// </summary>
		byte[] Decrypt(byte[] data);
	}
}
