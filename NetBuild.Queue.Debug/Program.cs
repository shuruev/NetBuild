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
		private static string s_connection;

		public static void Main(string[] args)
		{
			s_connection = ReadConnection();

			DebugCache();
			//DebugEngine();
			//MultiThreadTest();

			Console.WriteLine("Done.");
			Console.ReadKey();
		}

		public static string ReadConnection()
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

			return dbConnection;
		}

		public static void DebugCache()
		{
			var cache = new QueueCache(@"C:\All\QueueCache");
			var x = cache.IsCached("asasd");
			cache.SetCache("asasd", TimeSpan.FromSeconds(30));
			var y = cache.IsCached("asasd");
			cache.SetCache("asasd", TimeSpan.FromSeconds(30));
			var z = cache.IsCached("asasd");
			z = cache.IsCached("asasd");
			z = cache.IsCached("asasd");
		}

		public static void DebugEngine()
		{
			var db = new QueueDb(s_connection, TimeSpan.FromSeconds(10));
			db.Log = new ConsoleLog();

			var engine = new QueueEngine(db, 5);

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
			});
		}

		public static void MultiThreadTest()
		{
			//ThreadPool.SetMaxThreads(50, 50);
			for (int i = 0; i < 10000; i++)
			{
				var thread = new Thread(arg => DoJob((int)arg));
				thread.Start(i);
			}
		}

		public static void DoJob(int index)
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
