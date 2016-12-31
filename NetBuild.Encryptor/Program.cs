using System;
using Lean.Configuration;
using NetBuild.Common;

namespace NetBuild.Encryptor
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var config = new AppConfigReader();

			var thumbprint = config.Get<string>("Security.Thumbprint");
			var secure = new LocalEncryptor(thumbprint);

			var test1 = secure.EncryptUtf8("test");
			var test2 = secure.EncryptBase64("kNHUfTsWj5v3FkJrsTGFrzuE6yRr+RxNJwVBBV4uviUChfw2O1ijjTYGjK/i8qAFogRf0V4RcAotyTFKOAnDVA==");

			Console.WriteLine("Done.");
			Console.ReadKey();
		}
	}
}
