using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.VersionControl.Server;

namespace TfsPlugin.NetBuildQueue
{
	public class NetBuildQueuePlugin : ISubscriber
	{
		public string Name => "TfsPlugin.NetBuildQueue";
		public SubscriberPriority Priority => SubscriberPriority.Normal;
		public Type[] SubscribedTypes() => new[] { typeof(CheckinNotification) };

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
				if (notificationType == NotificationType.Notification)
				{
					var args = notificationEventArgs as CheckinNotification;
					if (args != null)
					{
						ProcessCheckinEvent(args);
					}
				}
			}
			catch (Exception e)
			{
				TeamFoundationApplicationCore.LogException($"An error occured in {Name}.", e);
			}

			return EventNotificationStatus.ActionPermitted;
		}

		private void ProcessCheckinEvent(CheckinNotification args)
		{
			TeamFoundationApplicationCore.Log("Hello world", 2505, EventLogEntryType.Information);
			TeamFoundationApplicationCore.Log($"{args.ChangesetOwner.DisplayName} committed changeset #{args.Changeset}.", 2505, EventLogEntryType.Information);
		}
	}
}
