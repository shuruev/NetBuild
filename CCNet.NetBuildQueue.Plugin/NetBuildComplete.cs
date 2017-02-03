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
		public void Run(IIntegrationResult result)
		{
			if (m_queue == null)
			{
				Init(result.ProjectName);
			}

			result.BuildProgressInformation.SignalStartRunTask($"Completing build '{result.Label}' for '{m_itemCode}'...");

			var sw = Stopwatch.StartNew();
			m_queue.CompleteBuild(m_itemCode, result.Label);
			sw.Stop();

			Log.Info($"[NETBUILD] Build '{result.Label}' for '{m_itemCode}' completed in {sw.ElapsedMilliseconds} ms.");

			result.BuildProgressInformation.AddTaskInformation("Notifying other projects...");

			sw = Stopwatch.StartNew();
			m_queue.ProcessSignal(new BuildCompleteSignal { BuildItem = m_itemCode });
			sw.Stop();

			Log.Info($"[NETBUILD] Build complete signal for '{m_itemCode}' processed in {sw.ElapsedMilliseconds} ms.");
		}
	}
}
