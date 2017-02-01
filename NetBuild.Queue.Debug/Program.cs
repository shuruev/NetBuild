using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Atom.Toolbox;
using NetBuild.Common;
using NetBuild.Queue.Client;
using NetBuild.Queue.Core;
using NetBuild.Queue.Engine;
using Serilog;
using BuildCompleteSignal = NetBuild.Queue.Core.BuildCompleteSignal;
using QueueEngine = NetBuild.Queue.Core.QueueEngine;
using ReferenceItemTrigger = NetBuild.Queue.Core.ReferenceItemTrigger;
using QueueEngine2 = NetBuild.Queue.Engine.QueueEngine;
using SourcePathTrigger2 = NetBuild.Queue.Engine.SourcePathTrigger;
using ReferenceItemTrigger2 = NetBuild.Queue.Engine.ReferenceItemTrigger;
using SourceChangedSignal = NetBuild.Queue.Core.SourceChangedSignal;
using SourceChangedSignal2 = NetBuild.Queue.Engine.SourceChangedSignal;

namespace NetBuild.Queue.Debug
{
	public class Program
	{
		private static string s_connection;

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

		public static void Main(string[] args)
		{
			s_connection = ReadConnection();

			DebugClient();
			//Debug2Engine();
			//Debug2Triggers();
			//Debug2Modifications();
			//DebugHttp();
			//DebugCache();
			//DebugEngine();
			//MultiThreadTest();

			Console.WriteLine("Done.");
			Console.ReadKey();
		}

		public static void DebugClient()
		{
			var logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.LiterateConsole()
				.CreateLogger();

			var client = new QueueClient("http://localhost:8000")
			{
				Logger = new SerilogAdapter(logger)
			};

			Console.WriteLine(client.Test("Project1"));
		}

		public static void Debug2Engine()
		{
			var triggers = new Triggers(new TriggerStorage(s_connection, TimeSpan.FromSeconds(10)));
			var modifications = new Modifications(new ModificationStorage(s_connection, TimeSpan.FromSeconds(10)));

			var engine = new QueueEngine2(triggers, modifications);
			engine.AddDetector(new SourceChangedDetector());
			engine.AddDetector(new BuildCompleteDetector());

			engine.Load();

			engine.SetTriggers(
				"Project1",
				new[]
				{
					new ReferenceItemTrigger2 { ReferenceItem = "Project2" },
					new ReferenceItemTrigger2 { ReferenceItem = "Project3" }
				});

			engine.SetTriggers(
				"Project2",
				new[]
				{
					new ReferenceItemTrigger2 { ReferenceItem = "Project4" }
				});

			engine.SetTriggers(
				"Project3",
				new[]
				{
					new ReferenceItemTrigger2 { ReferenceItem = "Project4" }
				});

			engine.SetTriggers(
				"Project4",
				new[]
				{
					new ReferenceItemTrigger2 { ReferenceItem = "Project5" }
				});

			engine.SetTriggers(
				"Project1",
				new[]
				{
					new SourcePathTrigger2 { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project2",
				new[]
				{
					new SourcePathTrigger2 { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project3",
				new[]
				{
					new SourcePathTrigger2 { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project4",
				new[]
				{
					new SourcePathTrigger2 { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project5",
				new[]
				{
					new SourcePathTrigger2 { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.ProcessSignal(new SourceChangedSignal2
			{
				ChangeId = "11584",
				ChangePath = "$/main/Production/Metro/Services/Metro.Assessment/Metro.Assessment.Client/AssessmentClient.cs",
				ChangeAuthor = "Shuruev, Oleg",
				ChangeType = "edit",
				ChangeComment = "Implemented initial version",
				ChangeDate = DateTime.UtcNow
			});

			var x1 = engine.ShouldBuild("Project1");
			var x2 = engine.ShouldBuild("Project2");
			var x3 = engine.ShouldBuild("Project3");
			var x4 = engine.ShouldBuild("Project4");
			var x5 = engine.ShouldBuild("Project5");
		}

		public static void Debug2Triggers()
		{
			var storage = new TriggerStorage(s_connection, TimeSpan.FromSeconds(10));
			var triggers = new Triggers(storage);
			triggers.Load();

			triggers.Set(
				"V3.Storage",
				new[]
				{
					new ReferenceItemTrigger2 { ReferenceItem = "Project3" },
					new ReferenceItemTrigger2 { ReferenceItem = "Project4" },
					new ReferenceItemTrigger2 { ReferenceItem = "Project5" }
				});

			triggers.Set(
				"V3.Storage1",
				new[]
				{
					new ReferenceItemTrigger2 { ReferenceItem = "Project3" },
					//new ReferenceItemTrigger2 { ReferenceItem = "Project4" },
					new ReferenceItemTrigger2 { ReferenceItem = "Project5" }
				});

			triggers.Set(
				"V3.Storage1",
				new[]
				{
					new SourcePathTrigger2 { SourcePath = "Path #3" },
					new SourcePathTrigger2 { SourcePath = "$/Main/Production/Metro/Services/Metro.Assessment" },
					new SourcePathTrigger2 { SourcePath = "$/Main/Production/Metro/Services" }
				});

			triggers.Set(
				"V3.Storage2",
				new[]
				{
					new ReferenceItemTrigger2 { ReferenceItem = "Project3" },
					//new ReferenceItemTrigger2 { ReferenceItem = "Project4" },
					new ReferenceItemTrigger2 { ReferenceItem = "Project5" }
				});

			triggers.Set(
				"Main",
				new[]
				{
					new SourcePathTrigger2 { SourcePath = "$/Main" }
				});
		}

		public static void Debug2Modifications()
		{
			var storage = new ModificationStorage(s_connection, TimeSpan.FromSeconds(10));
			var modifications = new Modifications(storage);
			modifications.Load();

			var items = new List<ItemModification>();
			for (int p = 0; p < 5; p++)
			{
				for (int i = 0; i < 10; i++)
				{
					var modification = new Modification
					{
						Code = $"{i}",
						Item = "$/Main/Production/Metro/Services/Metro.Assessment/Metro.Assessment.Client/AssessmentClient.cs",
						Author = "Shuruev, Oleg",
						Type = "edit",
						Comment = "Implemented initial version",
						Date = DateTime.UtcNow
					};

					items.Add(new ItemModification { Item = $"Project{p}", Modification = modification });
				}
			}

			modifications.Add(items);
		}

		public static void DebugHttp()
		{
			for (int i = 0; i < 100; i++)
			{
				var sw1 = Stopwatch.StartNew();
				var client = new HttpClient();
				var result1 = client.GetStringAsync("http://dev-metro.cnetcontent.com:2021/transform/status").Result;
				sw1.Stop();

				var sw2 = Stopwatch.StartNew();
				var db = new QueueDb(s_connection, TimeSpan.FromSeconds(10));
				var engine = new QueueEngine(db);
				var result2 = engine.ShouldBuild("V3.Storage");
				sw2.Stop();

				Console.WriteLine($"{result1}\t{sw1.ElapsedMilliseconds} ms\t\t{result2.Count}\t{sw2.ElapsedMilliseconds} ms");
			}
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

			var engine = new QueueEngine(db);

			var rows = db.ShouldBuild("Project2");

			db.SubmitItem("V3.Storage");

			for (int i = 0; i < 10; i++)
			{
				var sw = Stopwatch.StartNew();

				//db.SetTriggers("V3.Storage", "ReferenceItem", new[] { "Project3", "Project4", "Project5" });
				engine.SetTriggers(
					"V3.Storage",
					new[]
					{
						new ReferenceItemTrigger { ProjectItem = "Project3" },
						new ReferenceItemTrigger { ProjectItem = "Project4" },
						new ReferenceItemTrigger { ProjectItem = "Project5" }
					});

				//db.SetTriggers("V3.Storage", "ReferenceItem", new List<string>());
				engine.SetTriggers("V3.Storage", new List<ReferenceItemTrigger>());

				sw.Stop();
				Console.WriteLine(sw.ElapsedMilliseconds);
			}

			//db.ProcessSignal("BuildComplete", "{ \"item\": \"Project3\" }");
			var x = engine.ProcessSignal(new BuildCompleteSignal { ProjectItem = "Project3" });
			var y = engine.ProcessSignal(new SourceChangedSignal
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
			var threads = new List<Thread>();

			//ThreadPool.SetMaxThreads(50, 50);
			for (int i = 0; i < 1000; i++)
			{
				var thread = new Thread(arg => DoJob((int)arg));
				thread.Start(i);

				threads.Add(thread);
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}
		}

		private static ActionLimit Limit = new ActionLimit(2);

		public static void DoJob(int index)
		{
			try
			{
				var db = new QueueDb(s_connection, TimeSpan.FromSeconds(10));
				/*Limit.Do(() =>
				{
					db.SubmitItem("V3.Storage");
					Thread.Sleep(1000);
				});*/

				var engine = new QueueEngine(db);
				engine.SubmitItem("V3.Storage");

				Console.WriteLine($"Hello from #{index} (.NET thread {Thread.CurrentThread.ManagedThreadId})");
				//Console.WriteLine($"DONE #{index} (.NET thread {Thread.CurrentThread.ManagedThreadId})");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
