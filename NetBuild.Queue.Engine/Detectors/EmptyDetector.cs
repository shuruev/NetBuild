using System;
using System.Collections.Generic;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public abstract class EmptyDetector : IDetector
	{
		protected readonly object m_sync;

		protected EmptyDetector()
		{
			m_sync = new object();
		}

		public virtual void SetTriggers(string item, Type type, IEnumerable<ITrigger> triggers)
		{
		}

		public virtual void AddModifications(IEnumerable<ItemModification> modifications)
		{
		}

		public virtual List<ItemModification> DetectChanges<T>(T signal) where T : ISignal
		{
			return new List<ItemModification>();
		}

		public virtual bool ShouldIgnore(string item)
		{
			return false;
		}

		public virtual void StartBuild(string item, string label)
		{
		}

		public virtual void CompleteBuild(string item, string label)
		{
		}

		public virtual void StopBuild(string item, string label)
		{
		}
	}
}
