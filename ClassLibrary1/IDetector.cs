﻿using System;
using System.Collections.Generic;

namespace NetBuild.Queue.Engine
{
	public interface IDetector
	{
		void SetTriggers(string item, Type type, IEnumerable<ITrigger> triggers);
		void AddModifications(IEnumerable<ItemModification> modifications);
		List<ItemModification> DetectChanges<T>(T signal) where T : ISignal;
		bool ShouldIgnore(string item);
	}
}
