using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lean.Configuration;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Debug
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var config = new AppConfigReader();

			var thumbprint = config.Get<string>("Security.Thumbprint");
			var secure = new LocalEncryptor(thumbprint);

			var dbConnection = config.Get<string>("NetBuild.DbConnection");
			if (dbConnection.Contains("{password}"))
			{
				var dbPassword = secure.DecryptUtf8(config.Get<string>("NetBuild.DbPassword"));
				dbConnection = dbConnection.Replace("{password}", dbPassword);
			}

			var db = new QueueDb(dbConnection, TimeSpan.FromSeconds(10));
			var rows = db.ShouldBuild("Project2");

			db.SubmitItem("V3.Storage");

			for (int i = 0; i < 100; i++)
			{
				var sw = Stopwatch.StartNew();
				db.SetTriggers("V3.Storage", "ReferenceItem", new[] { "Project3", "Project4", "Project5" });
				db.SetTriggers("V3.Storage", "ReferenceItem", new List<string>());
				sw.Stop();
				Console.WriteLine(sw.ElapsedMilliseconds);
			}

			Console.WriteLine("Done.");
			Console.ReadKey();
		}
	}
}
