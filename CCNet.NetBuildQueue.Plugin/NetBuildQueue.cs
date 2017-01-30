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
		private const int c_minTimeoutInSeconds = 15 * 60; // 15 min
		private const int c_maxTimeoutInSeconds = 45 * 60; // 45 min

		private Random m_random;
		private QueueCache m_cache;

		[ReflectorProperty("path", Required = false)]
		public string SourcePath { get; set; }

		[ReflectorProperty("cache")]
		public string CachePath { get; set; }

		public Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
		{
			if (m_db == null)
			{
				Init(from.ProjectName);
			}

			Log.Debug($"[NETBUILD] Checking cache for '{m_itemCode}'...");
			if (m_cache.IsCached(m_itemCode))
			{
				Log.Debug($"[NETBUILD] No changes for '{m_itemCode}' due to local cache.");
				return new Modification[0];
			}

			Log.Debug($"[NETBUILD] Getting changes for '{m_itemCode}'...");
			var sw = Stopwatch.StartNew();
			var modifications = m_db.ShouldBuild(m_itemCode).Select(Convert).ToArray();
			sw.Stop();

			var seconds = m_random.Next(c_minTimeoutInSeconds, c_maxTimeoutInSeconds);
			var timeout = TimeSpan.FromSeconds(seconds);
			Log.Debug($"[NETBUILD] Setting local cache for '{m_itemCode}' to {timeout}...");
			m_cache.SetCache(m_itemCode, timeout);

			Log.Info($"[NETBUILD] {modifications.Length} change(s) found in {sw.ElapsedMilliseconds} ms.");
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

		protected override void Init(string projectName)
		{
			base.Init(projectName);

			Log.Debug($"[NETBUILD] Submitting item '{m_itemCode}' into database...");
			var sw = Stopwatch.StartNew();
			m_db.SubmitItem(m_itemCode);

			sw.Stop();
			Log.Info($"[NETBUILD] Item '{m_itemCode}' submitted in {sw.ElapsedMilliseconds} ms.");

			var triggers = new List<SourcePathTrigger>();
			if (!String.IsNullOrEmpty(SourcePath))
				triggers.Add(new SourcePathTrigger { SourcePath = SourcePath });

			Log.Debug($"[NETBUILD] Setting source control trigger for item '{m_itemCode}'...");
			sw = Stopwatch.StartNew();
			m_db.SetTriggers(m_itemCode, triggers);

			sw.Stop();
			Log.Info($"[NETBUILD] {triggers.Count} trigger(s) for '{m_itemCode}' submitted in {sw.ElapsedMilliseconds} ms.");

			m_random = new Random();
			m_cache = new QueueCache(CachePath);
			Log.Info($"[NETBUILD] Started source control tracking for {m_itemCode}.");
		}

		public void Purge(IProject project)
		{
		}
	}
}
