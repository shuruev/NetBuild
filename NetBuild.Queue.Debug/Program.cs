using System;
using System.Collections.Generic;
using System.Diagnostics;
using Atom.Toolbox;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Debug
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var config = new AppConfigReader();

			var dbConnection = config.Get<string>("NetBuild.DbConnection");
			if (dbConnection.Contains("{password}"))
			{
				var thumbprint = config.Get<string>("Security.Thumbprint");
				var secure = new LocalEncryptor(thumbprint);

				var dbPassword = secure.DecryptUtf8(config.Get<string>("NetBuild.DbPassword"));
				dbConnection = dbConnection.Replace("{password}", dbPassword);
			}

			var db = new QueueDb(dbConnection, TimeSpan.FromSeconds(10));
			db.Log = new ConsoleLog();

			var engine = new QueueEngine(db);

			var rows = db.ShouldBuild("Project2");

			db.SubmitItem("V3.Storage");

			/*for (int i = 0; i < 10; i++)
			{
				var sw = Stopwatch.StartNew();
				db.SetTriggers("V3.Storage", "ReferenceItem", new[] { "Project3", "Project4", "Project5" });
				db.SetTriggers("V3.Storage", "ReferenceItem", new List<string>());
				sw.Stop();
				Console.WriteLine(sw.ElapsedMilliseconds);
			}*/

			//db.ProcessSignal("BuildComplete", "{ \"item\": \"Project3\" }");
			engine.ProcessSignal(new BuildCompleteSignal { ProjectItem = "Project3" });
			engine.ProcessSignal(new SourceChangedSignal
			{
				ChangeId = "11584",
				ChangePath = "$/Main/Production/Metro/Services/Metro.Assessment/Metro.Assessment.Client/AssessmentClient.cs",
				ChangeAuthor = "Shuruev, Oleg",
				ChangeType = "edit",
				ChangeComment = "Implemented initial version",
				ChangeDate = DateTime.UtcNow
			});

			Console.WriteLine("Done.");
			Console.ReadKey();
		}
	}
}
