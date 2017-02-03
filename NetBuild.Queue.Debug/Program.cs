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
			//DebugEngine();
			//DebugTriggers();
			//DebugModifications();

			Console.WriteLine("Done.");
			Console.ReadKey();
		}

		public static void DebugClient()
		{
			var logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.LiterateConsole()
				.CreateLogger();

			//var client = new QueueClient("http://rufc-devbuild.cneu.cnwk:8001")
			var client = new QueueClient("http://localhost:8000")
			{
				Logger = new SerilogAdapter(logger)
			};

			/*bool updated;
			updated = client.SetTriggers("Project1", new[] { new ReferenceItemTrigger { ReferenceItem = "Project2" }, new ReferenceItemTrigger { ReferenceItem = "Project3" } });
			updated = client.SetTriggers("Project2", new[] { new ReferenceItemTrigger { ReferenceItem = "Project4" } });
			updated = client.SetTriggers("Project3", new[] { new ReferenceItemTrigger { ReferenceItem = "Project4" } });
			updated = client.SetTriggers("Project4", new[] { new ReferenceItemTrigger { ReferenceItem = "Project5" } });*/

			Console.WriteLine($"Project0: {client.ShouldBuild("Project0").Count > 0}");
			Console.WriteLine($"Project1: {client.ShouldBuild("Project1").Count > 0}");
			Console.WriteLine($"Project2: {client.ShouldBuild("Project2").Count > 0}");
			Console.WriteLine($"Project3: {client.ShouldBuild("Project3").Count > 0}");
			Console.WriteLine($"Project4: {client.ShouldBuild("Project4").Count > 0}");
			Console.WriteLine($"Project5: {client.ShouldBuild("Project5").Count > 0}");

			/*var items = client.ProcessSignal(new SourceChangedSignal
			{
				ChangeId = "11584",
				ChangePath = "$/main/Production/Metro/Services/Metro.Assessment/Metro.Assessment.Client/AssessmentClient.cs",
				ChangeAuthor = "Shuruev, Oleg",
				ChangeType = "edit",
				ChangeComment = "Implemented initial version",
				ChangeDate = DateTime.UtcNow
			});*/

			//client.ProcessSignal(new RebuildAllSignal());

			Console.WriteLine($"Project0: {client.ShouldBuild("Project0").Count > 0}");
			Console.WriteLine($"Project1: {client.ShouldBuild("Project1").Count > 0}");
			Console.WriteLine($"Project2: {client.ShouldBuild("Project2").Count > 0}");
			Console.WriteLine($"Project3: {client.ShouldBuild("Project3").Count > 0}");
			Console.WriteLine($"Project4: {client.ShouldBuild("Project4").Count > 0}");
			Console.WriteLine($"Project5: {client.ShouldBuild("Project5").Count > 0}");

			/*client.CompleteBuild("Project0", "debug");
			client.CompleteBuild("Project1", "debug");
			client.CompleteBuild("Project2", "debug");
			client.CompleteBuild("Project3", "debug");
			client.CompleteBuild("Project4", "debug");
			client.CompleteBuild("Project5", "debug");*/
		}

		public static void DebugEngine()
		{
			var triggers = new Triggers(new TriggerStorage(s_connection, TimeSpan.FromSeconds(10)));
			var modifications = new Modifications(new ModificationStorage(s_connection, TimeSpan.FromSeconds(10)));

			var engine = new QueueEngine(triggers, modifications);
			engine.AddDetector(new SourceChangedDetector());
			engine.AddDetector(new BuildCompleteDetector());
			engine.AddDetector(new RebuildAllDetector());
			engine.AddDetector(new ConcurrentBuildDetector(2));

			engine.Load();

			/*engine.SetTriggers(
				"Project1",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project2" },
					new ReferenceItemTrigger { ReferenceItem = "Project3" }
				});

			engine.SetTriggers(
				"Project2",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project4" }
				});

			engine.SetTriggers(
				"Project3",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project4" }
				});

			engine.SetTriggers(
				"Project4",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project5" }
				});

			engine.SetTriggers(
				"Project1",
				new[]
				{
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project2",
				new[]
				{
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project3",
				new[]
				{
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project4",
				new[]
				{
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services" }
				});

			engine.SetTriggers(
				"Project5",
				new[]
				{
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services" }
				});*/

			/*engine.ProcessSignal(new SourceChangedSignal
			{
				ChangeId = "11584",
				ChangePath = "$/main/Production/Metro/Services/Metro.Assessment/Metro.Assessment.Client/AssessmentClient.cs",
				ChangeAuthor = "Shuruev, Oleg",
				ChangeType = "edit",
				ChangeComment = "Implemented initial version",
				ChangeDate = DateTime.UtcNow
			});*/

			engine.StartBuild("Project1", "debug");
			engine.StopBuild("Project1", "debug");
			engine.StartBuild("Project1", "debug");
			engine.StartBuild("Project1", "debug");
			engine.CompleteBuild("Project1", "debug");
			engine.StopBuild("Project1", "debug");

			var x1 = engine.ShouldBuild("Project1");
			var x2 = engine.ShouldBuild("Project2");
			var x3 = engine.ShouldBuild("Project3");
			var x4 = engine.ShouldBuild("Project4");
			var x5 = engine.ShouldBuild("Project5");
		}

		public static void DebugTriggers()
		{
			var storage = new TriggerStorage(s_connection, TimeSpan.FromSeconds(10));
			var triggers = new Triggers(storage);
			triggers.Load();

			triggers.SetTriggers(
				"V3.Storage",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project3" },
					new ReferenceItemTrigger { ReferenceItem = "Project4" },
					new ReferenceItemTrigger { ReferenceItem = "Project5" }
				});

			triggers.SetTriggers(
				"V3.Storage1",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project3" },
					//new ReferenceItemTrigger { ReferenceItem = "Project4" },
					new ReferenceItemTrigger { ReferenceItem = "Project5" }
				});

			triggers.SetTriggers(
				"V3.Storage1",
				new[]
				{
					new SourcePathTrigger { SourcePath = "Path #3" },
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services/Metro.Assessment" },
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services" }
				});

			triggers.SetTriggers(
				"V3.Storage2",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project3" },
					//new ReferenceItemTrigger { ReferenceItem = "Project4" },
					new ReferenceItemTrigger { ReferenceItem = "Project5" }
				});

			triggers.SetTriggers(
				"Main",
				new[]
				{
					new SourcePathTrigger { SourcePath = "$/Main" }
				});
		}

		public static void DebugModifications()
		{
			var storage = new ModificationStorage(s_connection, TimeSpan.FromSeconds(10));
			var modifications = new Modifications(storage);
			modifications.Load();

			/*var items = new List<ItemModification>();
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

			modifications.Add(items);*/

			modifications.Reserve("Project2", "debug");

			modifications.Add(new[]
			{
				new ItemModification
				{
					Item = "Project2",
					Modification = new Modification
					{
						Code = "new",
						Item = "$/New/Project2",
						Author = "Shuruev, Oleg",
						Type = "edit",
						Comment = "New modification for Project2",
						Date = DateTime.UtcNow
					}
				}
			});

			modifications.Release("Project1", "debug");
			modifications.Release("Project2", "debug");

			modifications.Reserve("Project2", "debug");
			modifications.Release("Project2", "debug");
		}
	}
}
