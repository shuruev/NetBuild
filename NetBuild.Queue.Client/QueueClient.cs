﻿using System;
using System.Collections.Generic;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Client
{
	/// <summary>
	/// Provides client features of a build queue server.
	/// </summary>
	public class QueueClient : ApiClient, ITriggerSetup
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QueueClient(string baseUrl)
			: base(baseUrl)
		{
		}

		/// <summary>
		/// For a specified item, updates a complete set of its triggers of a given type.
		/// </summary>
		bool ITriggerSetup.Set(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			return Execute<bool>(HttpPost("set")
				.WithParam("item", item)
				.WithParam("trigger", type.Name)
				.WithBody(triggers));
		}

		/// <summary>
		/// Processes any external signal, which can trigger new builds.
		/// Returns a list of items which were potentially affected by this change.
		/// </summary>
		public List<string> ProcessSignal(ISignal signal)
		{
			return Execute<List<string>>(HttpPost("process")
				.WithParam("signal", signal.GetType().Name)
				.WithBody(signal));
		}

		/// <summary>
		/// Checks whether specified item should be built, and returns all corresponding modifications related to this build.
		/// </summary>
		public List<Modification> ShouldBuild(string item)
		{
			return Execute<List<Modification>>(HttpGet("check")
				.WithParam("item", item));
		}

		/// <summary>
		/// Starts build process for specified item, marking all the current modifications with specified build label.
		/// </summary>
		public void StartBuild(string item, string label)
		{
			Execute(HttpPost("start")
				.WithParam("item", item)
				.WithParam("label", label)
				.WithEmptyBody());
		}

		/// <summary>
		/// Marks specified build as completed, removing all corresponding modifications and updating related timestamps.
		/// </summary>
		public void CompleteBuild(string item, string label)
		{
			Execute(HttpPost("complete")
				.WithParam("item", item)
				.WithParam("label", label)
				.WithEmptyBody());
		}

		/// <summary>
		/// Stops build process, which may or may not be successfully completed.
		/// </summary>
		public void StopBuild(string item, string label)
		{
			Execute(HttpPost("stop")
				.WithParam("item", item)
				.WithParam("label", label)
				.WithEmptyBody());
		}
	}
}
