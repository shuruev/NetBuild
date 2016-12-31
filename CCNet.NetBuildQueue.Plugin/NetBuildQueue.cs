using System;
using System.Collections.Generic;
using System.Linq;
using Exortech.NetReflector;
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

		private string m_itemCode;
		private QueueDb m_db;

		public Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
		{
			if (m_db == null)
			{
				Init(from.ProjectName);
			}

			Log.Debug("[NETBUILD] Getting changes for 'm_itemCode'...");
			var modifications = m_db.ShouldBuild(m_itemCode).Select(Convert).ToArray();

			Log.Debug($"{modifications.Length} change(s) found.");
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

			Log.Debug("[NETBUILD] Creating database connection using configuration values...");
			m_db = new QueueDb(DbConnection, TimeSpan.FromSeconds(15));

			Log.Debug($"[NETBUILD] Submitting item '{m_itemCode}' into database...");
			m_db.SubmitItem(m_itemCode);

			var triggers = new List<string>();
			if (!String.IsNullOrEmpty(SourcePath))
				triggers.Add(SourcePath);

			Log.Debug($"[NETBUILD] Setting source control trigger for item '{m_itemCode}'...");
			m_db.SetTriggers(m_itemCode, "SourcePath", triggers);
		}

		public void Purge(IProject project)
		{
		}
	}
}
