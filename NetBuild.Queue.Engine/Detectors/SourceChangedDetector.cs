using System;
using System.Collections.Generic;
using System.Linq;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Engine
{
	public class SourceChangedDetector : EmptyDetector
	{
		private readonly Dictionary<string, List<string>> m_paths;

		public SourceChangedDetector()
		{
			m_paths = new Dictionary<string, List<string>>();
		}

		public override void SetTriggers(string item, Type type, IEnumerable<ITrigger> triggers)
		{
			if (type != typeof(SourcePathTrigger))
				return;

			lock (m_sync)
			{
				m_paths[item] = triggers.Cast<SourcePathTrigger>().Select(i => i.SourcePath).ToList();
			}
		}

		public override List<ItemModification> DetectChanges<T>(T signal)
		{
			var result = new List<ItemModification>();

			var local = signal as SourceChangedSignal;
			if (local == null)
				return result;

			if (String.IsNullOrEmpty(local.ChangePath))
				throw new InvalidOperationException("Source path is required for source control changes.");

			var comment = BuildComment(local);

			var items = m_paths
				.SelectMany(item => item.Value.Select(i => new KeyValuePair<string, string>(item.Key, i)))
				.Where(i => local.ChangePath.StartsWith(i.Value, StringComparison.OrdinalIgnoreCase))
				.Select(i => i.Key)
				.Distinct()
				.ToList();

			foreach (var item in items)
			{
				result.Add(new ItemModification
				{
					Item = item,
					Modification = new Modification
					{
						Code = local.ChangeId,
						Type = local.ChangeType,
						Author = local.ChangeAuthor,
						Item = local.ChangePath,
						Comment = comment,
						Date = local.ChangeDate.ToUniversalTime()
					}
				});
			}

			return result;
		}

		private string BuildComment(SourceChangedSignal signal)
		{
			if (String.IsNullOrEmpty(signal.ChangeId))
				return signal.ChangeComment;

			if (String.IsNullOrEmpty(signal.ChangeComment))
				return $"#{signal.ChangeId}";

			return $"#{signal.ChangeId}: {signal.ChangeComment}";
		}
	}
}
