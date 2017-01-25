using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			var cs = ReadChangeset(args.Changeset, context);

			List<Task> tasks = new List<Task>();

			var id = cs.ChangesetId;
			var author = cs.CommitterDisplayName;
			var comment = cs.Comment;
			var date = cs.CreationDate.ToUniversalTime();

			var sb = new StringBuilder();
			sb.AppendLine($"Change ID: {id}");
			sb.AppendLine($"Change author: {author}");
			sb.AppendLine($"Change comment: {comment}");
			sb.AppendLine($"Change date (UTC): {date}");
			sb.AppendLine();

			foreach (var change in cs.Changes)
			{
				var path = change.Item.ServerItem;
				var type = change.ChangeType.ToString().ToLowerInvariant();

				sb.AppendLine($"{path} [{type}]");

				var signal = new SourceChangedSignal
				{
					ChangeId = id.ToString(),
					ChangeAuthor = author,
					ChangeComment = comment,
					ChangeDate = date,
					ChangePath = path,
					ChangeType = type
				};

				tasks.Add(Task.Factory.StartNew(() => ProcessSignal(signal)));
			}

			Task.WaitAll(tasks.ToArray());

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

		private void ProcessSignal(SourceChangedSignal signal)
		{
			var log = new StringBuilder();

			var db = new QueueDb(Config.DbConnection, Config.DbTimeout);
			db.Log = new StringLog(log);

			var engine = new QueueEngine(db);

			var sw = Stopwatch.StartNew();
			engine.ProcessSignal(signal);
			sw.Stop();

			Log.Debug(
				$@"Signal processed in {sw.ElapsedMilliseconds} ms.

{JsonConvert.SerializeObject(signal, Formatting.Indented)}

{log}");
		}
	}
}
