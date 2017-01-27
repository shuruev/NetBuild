using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

			MultiThreadTest(dbConnection);

			/*var db = new QueueDb(dbConnection, TimeSpan.FromSeconds(10));
			db.Log = new ConsoleLog();

			var engine = new QueueEngine(db);

			var rows = db.ShouldBuild("Project2");

			db.SubmitItem("V3.Storage");

			for (int i = 0; i < 10; i++)
			{
				var sw = Stopwatch.StartNew();
				db.SetTriggers("V3.Storage", "ReferenceItem", new[] { "Project3", "Project4", "Project5" });
				db.SetTriggers("V3.Storage", "ReferenceItem", new List<string>());
				sw.Stop();
				Console.WriteLine(sw.ElapsedMilliseconds);
			}

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
			});*/

			Console.WriteLine("Done.");
			Console.ReadKey();
		}

		public static void MultiThreadTest(string dbConnection)
		{
			//ThreadPool.SetMaxThreads(50, 50);
			for (int i = 0; i < 10000; i++)
			{
				var thread = new Thread(args => DoJob((string)((object[])args)[0], (int)((object[])args)[1]));
				thread.Start(new object[] { dbConnection, i });
			}
		}

		public static void DoJob(string dbConnection, int index)
		{
			try
			{
				//var db = new QueueDb(dbConnection, TimeSpan.FromSeconds(10));
				//var engine = new QueueEngine(db, 5);
				Console.WriteLine($"Hello from #{index} (.NET thread {Thread.CurrentThread.ManagedThreadId})");
				Thread.Sleep(50000);
				Console.WriteLine($"DONE #{index} (.NET thread {Thread.CurrentThread.ManagedThreadId})");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
