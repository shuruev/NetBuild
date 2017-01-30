using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Server;
using NetBuild.Queue.Core;
using Newtonsoft.Json;
using Changeset = Microsoft.TeamFoundation.VersionControl.Client.Changeset;

namespace TfsPlugin.NetBuildQueue
{
	public class NetBuildQueuePlugin : ISubscriber
	{
		private const int c_maxDegreeOfParallelism = 5;

		public string Name => "TfsPlugin.NetBuildQueue";
		public SubscriberPriority Priority => SubscriberPriority.Normal;
		public Type[] SubscribedTypes() => new[] { typeof(CheckinNotification) };

		public NetBuildQueuePlugin()
		{
			Log.Name = Name;
			Log.Info($"{Name} loaded.");
		}

		public EventNotificationStatus ProcessEvent(
			TeamFoundationRequestContext requestContext,
			NotificationType notificationType,
			object notificationEventArgs,
			out int statusCode,
			out string statusMessage,
			out ExceptionPropertyCollection properties)
		{
			statusCode = 0;
			statusMessage = String.Empty;
			properties = null;

			try
			{
				if (Config.Load(requestContext.ServiceHost.PlugInDirectory))
				{
					Log.Info($"{Name} configured.");
					Log.Debug("Debug mode is enabled.");
				}

				if (notificationType == NotificationType.Notification)
				{
					var args = notificationEventArgs as CheckinNotification;
					if (args != null)
					{
						ProcessCheckinEvent(args, requestContext);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}

			return EventNotificationStatus.ActionPermitted;
		}

		private void ProcessCheckinEvent(CheckinNotification args, TeamFoundationRequestContext context)
		{
			var total = Stopwatch.StartNew();

			// read detailed changeset information
			var cs = ReadChangeset(args.Changeset, context);

			var id = cs.ChangesetId;
			var author = cs.CommitterDisplayName;
			var comment = cs.Comment;
			var date = cs.CreationDate.ToUniversalTime();

			// start building detailed log for debug mode
			var sb = new StringBuilder();
			sb.AppendLine($"Change ID: {id}");
			sb.AppendLine($"Change author: {author}");
			sb.AppendLine($"Change comment: {comment}");
			sb.AppendLine($"Change date (UTC): {date}");
			sb.AppendLine();

			var bag = new ConcurrentBag<string>();

			// create and process a separate signal for every change within a changeset
			Parallel.ForEach(
				cs.Changes,
				new ParallelOptions { MaxDegreeOfParallelism = c_maxDegreeOfParallelism },
				change =>
				{
					var path = change.Item.ServerItem;
					var type = change.ChangeType.ToString().ToLowerInvariant();

					lock (sb)
					{
						sb.AppendLine($"{path} [{type}]");
					}

					var signal = new SourceChangedSignal
					{
						ChangeId = id.ToString(),
						ChangeAuthor = author,
						ChangeComment = comment,
						ChangeDate = date,
						ChangePath = path,
						ChangeType = type
					};

					var items = ProcessSignal(signal);
					foreach (var item in items)
						bag.Add(item);
				});

			// reset local cache for potentially affected items
			if (!String.IsNullOrEmpty(Config.LocalCache))
			{
				var cache = new QueueCache(Config.LocalCache);
				foreach (var item in bag.Distinct().ToList())
				{
					cache.RemoveCache(item);
					Log.Debug($"Reset local cache for {item}.");
				}
			}

			total.Stop();
			Log.Info($"Checkin processed in {total.ElapsedMilliseconds} ms.\r\n\r\n{sb}");
		}

		private Changeset ReadChangeset(int changesetId, TeamFoundationRequestContext context)
		{
			var sw = Stopwatch.StartNew();
			var location = context.GetService<TeamFoundationLocationService>();
			var uri = location.GetSelfReferenceUri(context, location.GetServerAccessMapping(context));
			var collection = new TfsTeamProjectCollection(uri);
			var server = collection.GetService<VersionControlServer>();
			var changeset = server.GetChangeset(changesetId);
			sw.Stop();

			Log.Debug($"Changeset information read in {sw.ElapsedMilliseconds} ms.\r\n{uri.AbsoluteUri}");
			return changeset;
		}

		private List<string> ProcessSignal(SourceChangedSignal signal)
		{
			var log = new StringBuilder();

			var db = new QueueDb(Config.DbConnection, Config.DbTimeout);
			if (Config.DebugMode)
			{
				db.Log = new StringLog(log);
			}

			var engine = new QueueEngine(db, c_maxDegreeOfParallelism);

			var sw = Stopwatch.StartNew();
			var items = engine.ProcessSignal(signal);
			sw.Stop();

			Log.Debug(
				$@"Signal processed in {sw.ElapsedMilliseconds} ms.

{JsonConvert.SerializeObject(signal, Formatting.Indented)}

{log}
{String.Join(Environment.NewLine, items.Select(item => "- " + item))}");

			return items;
		}
	}
}
