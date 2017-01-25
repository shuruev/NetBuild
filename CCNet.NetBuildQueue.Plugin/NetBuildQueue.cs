using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exortech.NetReflector;
using NetBuild.Common;
using NetBuild.Queue.Core;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace CCNet.NetBuildQueue.Plugin
{
	[ReflectorType("netBuildQueue")]
	public class NetBuildQueue : ISourceControl
	{
		[ReflectorProperty("item", Required = false)]
		public string ItemName { get; set; }

		[ReflectorProperty("path", Required = false)]
		public string SourcePath { get; set; }

		[ReflectorProperty("db")]
		public string DbConnection { get; set; }

		[ReflectorProperty("thumbprint", Required = false)]
		public string SecurityThumbprint { get; set; }

		[ReflectorProperty("password", Required = false)]
		public string EncryptedPassword { get; set; }

		private string m_itemCode;
		private QueueDb m_db;

		public Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
		{
			if (m_db == null)
			{
				Init(from.ProjectName);
			}

			Log.Debug("[NETBUILD] Getting changes for 'm_itemCode'...");
			var sw = Stopwatch.StartNew();
			var modifications = m_db.ShouldBuild(m_itemCode).Select(Convert).ToArray();
			sw.Stop();

			Log.Debug($"{modifications.Length} change(s) found in {sw.ElapsedMilliseconds} ms.");
			return modifications;
		}

		private Modification Convert(BuildDto dto)
		{
			var result = new Modification
			{
				Type = dto.ModificationType,
				FileName = dto.ModificationItem,
				ModifiedTime = dto.ModificationDate ?? dto.Created,
				UserName = dto.ModificationAuthor,
				Comment = dto.ModificationComment
			};

			if (dto.ModificationType == "reference")
			{
				result.Url = $"http://rufc-devbuild.cneu.cnwk/build/server/Build/project/{dto.ModificationItem}/ViewProjectReport.aspx";
			}
			else if (!String.IsNullOrEmpty(dto.ModificationCode))
			{
				result.Url = $"http://rufc-devbuild.cneu.cnwk:8080/tfs/web/cs.aspx?pcguid=ef39c31a-27bd-493c-8464-da7053c74167&cs={dto.ModificationCode}";
			}

			return result;
		}

		public void LabelSourceControl(IIntegrationResult result)
		{
		}

		public void GetSource(IIntegrationResult result)
		{
		}

		public void Initialize(IProject project)
		{
			Init(project.Name);
		}

		private void Init(string projectName)
		{
			m_itemCode = String.IsNullOrEmpty(ItemName) ? projectName : ItemName;

			m_db = InitDb();

			Log.Debug($"[NETBUILD] Submitting item '{m_itemCode}' into database...");
			m_db.SubmitItem(m_itemCode);

			var triggers = new List<string>();
			if (!String.IsNullOrEmpty(SourcePath))
				triggers.Add(SourcePath);

			Log.Debug($"[NETBUILD] Setting source control trigger for item '{m_itemCode}'...");
			m_db.SetTriggers(m_itemCode, "SourcePath", triggers);
		}

		private QueueDb InitDb()
		{
			// will put into configuration once there is a need
			var timeout = TimeSpan.FromSeconds(15);

			Log.Debug("[NETBUILD] Creating database connection using configuration values...");
			if (!DbConnection.Contains("{password}"))
			{
				// not using the encrypted password
				return new QueueDb(DbConnection, timeout);
			}

			// using the encrypted password
			Log.Debug($"[NETBUILD] Using certificate '{SecurityThumbprint}' to decrypt database password...");

			if (String.IsNullOrEmpty(SecurityThumbprint))
				throw new InvalidOperationException("Configuration parameter <thumbprint> should be defined if <db> is using encrypted '{password}' syntax.");

			if (String.IsNullOrEmpty(EncryptedPassword))
				throw new InvalidOperationException("Configuration parameter <password> should be defined if <db> is using encrypted '{password}' syntax.");

			var secure = new LocalEncryptor(SecurityThumbprint);
			var password = secure.DecryptUtf8(EncryptedPassword);
			var connection = DbConnection.Replace("{password}", password);

			return new QueueDb(connection, timeout);
		}

		public void Purge(IProject project)
		{
		}
	}
}
