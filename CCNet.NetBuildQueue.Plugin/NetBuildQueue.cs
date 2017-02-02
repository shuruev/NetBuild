using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exortech.NetReflector;
using NetBuild.Queue.Core;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace CCNet.NetBuildQueue.Plugin
{
	[ReflectorType("netBuildQueue")]
	public class NetBuildQueue : NetBuildPlugin, ISourceControl
	{
		private bool m_initialized;

		[ReflectorProperty("path", Required = false)]
		public string SourcePath { get; set; }

		public ThoughtWorks.CruiseControl.Core.Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
		{
			// make sure queue client is initialized and all required objects are configured properly
			Init(from.ProjectName);

			Log.Debug($"[NETBUILD] Getting changes for '{m_itemCode}'...");
			var sw = Stopwatch.StartNew();
			var modifications = m_queue.ShouldBuild(m_itemCode).Select(Convert).ToArray();
			sw.Stop();

			Log.Info($"[NETBUILD] {modifications.Length} change(s) found in {sw.ElapsedMilliseconds} ms.");
			return modifications;
		}

		private ThoughtWorks.CruiseControl.Core.Modification Convert(NetBuild.Queue.Core.Modification modification)
		{
			var result = new ThoughtWorks.CruiseControl.Core.Modification
			{
				Type = modification.Type,
				FileName = modification.Item,
				ModifiedTime = modification.Date,
				UserName = modification.Author,
				Comment = modification.Comment
			};

			if (modification.Type == "reference")
			{
				result.Url = $"http://rufc-devbuild.cneu.cnwk/build/server/Build/project/{modification.Item}/ViewProjectReport.aspx";
			}
			else if (!String.IsNullOrEmpty(modification.Code))
			{
				result.Url = $"http://rufc-devbuild.cneu.cnwk:8080/tfs/web/cs.aspx?pcguid=ef39c31a-27bd-493c-8464-da7053c74167&cs={modification.Code}";
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

		protected override void Init(string projectName)
		{
			if (m_initialized)
				return;

			// initialize item code and create queue client
			base.Init(projectName);

			var triggers = new List<SourcePathTrigger>();
			if (!String.IsNullOrEmpty(SourcePath))
				triggers.Add(new SourcePathTrigger { SourcePath = SourcePath });

			Log.Debug($"[NETBUILD] Setting source control trigger for item '{m_itemCode}'...");
			var sw = Stopwatch.StartNew();
			m_queue.SetTriggers(m_itemCode, triggers);

			sw.Stop();
			Log.Info($"[NETBUILD] {triggers.Count} trigger(s) for '{m_itemCode}' submitted in {sw.ElapsedMilliseconds} ms.");

			m_initialized = true;
		}

		public void Purge(IProject project)
		{
		}
	}
}
