using System;
using Exortech.NetReflector;
using NetBuild.Queue.Client;
using ThoughtWorks.CruiseControl.Core.Util;

namespace CCNet.NetBuildQueue.Plugin
{
	public abstract class NetBuildPlugin
	{
		protected string m_itemCode;
		protected QueueClient m_queue;

		[ReflectorProperty("item", Required = false)]
		public string ItemName { get; set; }

		[ReflectorProperty("server")]
		public string ServerUrl { get; set; }

		protected virtual void Init(string projectName)
		{
			m_itemCode = String.IsNullOrEmpty(ItemName) ? projectName : ItemName;
			m_queue = new QueueClient(ServerUrl);

			Log.Debug($"[NETBUILD] Initialized queue client for '{m_itemCode}'.");
		}
	}
}
