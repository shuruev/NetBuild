using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Server;

namespace TfsPlugin.NetBuildQueue
{
	public class NetBuildQueuePlugin : ISubscriber
	{
		private const int c_eventId = 2505;

		public string Name => "TfsPlugin.NetBuildQueue";
		public SubscriberPriority Priority => SubscriberPriority.Normal;
		public Type[] SubscribedTypes() => new[] { typeof(CheckinNotification) };

		public NetBuildQueuePlugin()
		{
			TeamFoundationApplicationCore.Log($"{Name} loaded.", c_eventId, EventLogEntryType.Information);
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
				var assemblyName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
				var assemblyPath = Path.Combine(requestContext.ServiceHost.PlugInDirectory, assemblyName);

				if (Config.Load(assemblyPath))
				{
					TeamFoundationApplicationCore.Log($"{Name} configured.", c_eventId, EventLogEntryType.Information);
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
				TeamFoundationApplicationCore.LogException($"An error occured in {Name}.", e);
			}

			return EventNotificationStatus.ActionPermitted;
		}

		private void ProcessCheckinEvent(CheckinNotification args, TeamFoundationRequestContext context)
		{
			var location = context.GetService<TeamFoundationLocationService>();
			var uri = location.GetSelfReferenceUri(context, location.GetServerAccessMapping(context));
			var collection = new TfsTeamProjectCollection(uri);
			var server = collection.GetService<VersionControlServer>();
			var changeSet = server.GetChangeset(args.Changeset);

			var id = changeSet.ChangesetId;
			var author = changeSet.CommitterDisplayName;
			var comment = changeSet.Comment;
			var date = changeSet.CreationDate.ToUniversalTime();

			var sb = new StringBuilder();
			sb.AppendLine($"Change ID: {id}");
			sb.AppendLine($"Change author: {author}");
			sb.AppendLine($"Change comment: {comment}");
			sb.AppendLine($"Change date (UTC): {date}");

			foreach (var change in changeSet.Changes)
			{
				var path = change.Item.ServerItem;
				var type = change.ChangeType.ToString().ToLowerInvariant();

				sb.AppendLine($"{path} ({type})");
			}

			//TeamFoundationApplicationCore.Log($"{args.ChangesetOwner.DisplayName} committed changeset #{args.Changeset}.", c_eventId, EventLogEntryType.Information);
			TeamFoundationApplicationCore.Log(sb.ToString(), c_eventId, EventLogEntryType.Information);
		}
	}
}
