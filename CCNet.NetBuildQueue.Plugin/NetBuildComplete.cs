using System.Diagnostics;
using Exortech.NetReflector;
using NetBuild.Queue.Core;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace CCNet.NetBuildQueue.Plugin
{
	[ReflectorType("netBuildComplete")]
	public class NetBuildComplete : NetBuildPlugin, ITask
	{
		private QueueCache m_cache;

		[ReflectorProperty("cache")]
		public string CachePath { get; set; }

		public void Run(IIntegrationResult result)
		{
			if (m_db == null)
			{
				Init(result.ProjectName);
			}

			result.BuildProgressInformation.SignalStartRunTask($"Completing build '{result.Label}' for '{m_itemCode}'...");

			var sw = Stopwatch.StartNew();
			m_db.CompleteBuild(m_itemCode, result.Label);
			sw.Stop();

			Log.Info($"[NETBUILD] Build '{result.Label}' for '{m_itemCode}' completed in {sw.ElapsedMilliseconds} ms.");

			if (m_cache == null)
			{
				m_cache = new QueueCache(CachePath);
			}

			result.BuildProgressInformation.AddTaskInformation("Notifying other projects...");

			sw = Stopwatch.StartNew();
			var items = m_db.ProcessSignal(new BuildCompleteSignal { ProjectItem = m_itemCode });
			sw.Stop();

			Log.Info($"[NETBUILD] Build complete signal for '{m_itemCode}' processed in {sw.ElapsedMilliseconds} ms.");

			foreach (var item in items)
			{
				m_cache.RemoveCache(item);
			}
		}
	}
}
