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

			//DebugClient();
			//DebugEngine();
			//DebugTriggers();
			DebugModifications();

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

			/*var items = client.ProcessSignal(new SourceChangedSignal
			{
				ChangeId = "11584",
				ChangePath = "$/main/Production/Metro/Services/Metro.Assessment/Metro.Assessment.Client/AssessmentClient.cs",
				ChangeAuthor = "Shuruev, Oleg",
				ChangeType = "edit",
				ChangeComment = "Implemented initial version",
				ChangeDate = DateTime.UtcNow
			});*/

			var items = client.ProcessSignal(new SourceChangedSignal
			{
				ChangeId = "13253",
				ChangePath = "$/Sandbox/olshuruev/Temp/test.txt",
				ChangeAuthor = "Shuruev, Oleg",
				ChangeType = "edit",
				ChangeComment = "Implemented initial version",
				ChangeDate = DateTime.UtcNow
			});

			Console.WriteLine(client.ShouldBuild("Project1").Count);
		}

		public static void DebugEngine()
		{
			var triggers = new Triggers(new TriggerStorage(s_connection, TimeSpan.FromSeconds(10)));
			var modifications = new Modifications(new ModificationStorage(s_connection, TimeSpan.FromSeconds(10)));

			var engine = new QueueEngine(triggers, modifications);
			engine.AddDetector(new SourceChangedDetector());
			engine.AddDetector(new BuildCompleteDetector());

			engine.Load();

			engine.SetTriggers(
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
				});

			engine.ProcessSignal(new SourceChangedSignal
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

		public static void DebugTriggers()
		{
			var storage = new TriggerStorage(s_connection, TimeSpan.FromSeconds(10));
			var triggers = new Triggers(storage);
			triggers.Load();

			triggers.Set(
				"V3.Storage",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project3" },
					new ReferenceItemTrigger { ReferenceItem = "Project4" },
					new ReferenceItemTrigger { ReferenceItem = "Project5" }
				});

			triggers.Set(
				"V3.Storage1",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project3" },
					//new ReferenceItemTrigger { ReferenceItem = "Project4" },
					new ReferenceItemTrigger { ReferenceItem = "Project5" }
				});

			triggers.Set(
				"V3.Storage1",
				new[]
				{
					new SourcePathTrigger { SourcePath = "Path #3" },
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services/Metro.Assessment" },
					new SourcePathTrigger { SourcePath = "$/Main/Production/Metro/Services" }
				});

			triggers.Set(
				"V3.Storage2",
				new[]
				{
					new ReferenceItemTrigger { ReferenceItem = "Project3" },
					//new ReferenceItemTrigger { ReferenceItem = "Project4" },
					new ReferenceItemTrigger { ReferenceItem = "Project5" }
				});

			triggers.Set(
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

			modifications.Reserve("Project2");

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

			modifications.Release("Project1");
			modifications.Release("Project2");

			modifications.Reserve("Project2");
			modifications.Release("Project2");
		}
	}
}
