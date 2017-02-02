using System.Diagnostics;
using Exortech.NetReflector;
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
		}
	}
}
