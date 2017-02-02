using System.Collections.Generic;
using NetBuild.Common;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Client
{
	/// <summary>
	/// Provides client features of a build queue server.
	/// </summary>
	public class QueueClient : ApiClient
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
		public void SetTriggers<T>(string item, IEnumerable<T> triggers) where T : ITrigger
		{
		}

		/// <summary>
		/// Processes any external signal, which can trigger new builds.
		/// Returns a list of items which were potentially affected by this change.
		/// </summary>
		public List<string> ProcessSignal<T>(T signal) where T : ISignal
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
		/// Starts build process for specified item, marking all the current modifications with specified build code.
		/// </summary>
		public void StartBuild(string itemCode, string buildCode)
		{
		}

		/// <summary>
		/// Marks specified build as completed, removing all corresponding modifications and updating related timestamps.
		/// </summary>
		public void CompleteBuild(string itemCode, string buildCode)
		{
		}
	}
}
