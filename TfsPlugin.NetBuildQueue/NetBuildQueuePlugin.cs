using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Server;
using NetBuild.Queue.Client;
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
			sb.AppendLine("---");

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

					ProcessSignal(signal);
				});

			total.Stop();

			Log.Info(
				$@"Checkin processed in {total.ElapsedMilliseconds} ms.
---
{sb}");
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

		private void ProcessSignal(SourceChangedSignal signal)
		{
			var queue = new QueueClient(Config.ServerUrl)
			{
				Timeout = Config.Timeout
			};

			var sw = Stopwatch.StartNew();
			var items = queue.ProcessSignal(signal);
			sw.Stop();

			Log.Debug(
				$@"Signal processed in {sw.ElapsedMilliseconds} ms.
---
{JsonConvert.SerializeObject(signal, Formatting.Indented)}
---
{String.Join(Environment.NewLine, items.Select(item => "> " + item))}");
		}
	}
}
