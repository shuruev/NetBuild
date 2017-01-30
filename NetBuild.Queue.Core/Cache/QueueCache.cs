using System;
using System.IO;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Optimizes queue requests by using local cache.
	/// </summary>
	public class QueueCache
	{
		private readonly string m_path;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QueueCache(string directoryPath)
		{
			if (String.IsNullOrEmpty(directoryPath))
				throw new ArgumentNullException(nameof(directoryPath));

			m_path = directoryPath;

			if (!Directory.Exists(m_path))
				Directory.CreateDirectory(m_path);
		}

		private string FilePath(string itemCode) => Path.Combine(m_path, itemCode + ".txt");

		/// <summary>
		/// Checks if specified item is cached at the current moment.
		/// </summary>
		public bool IsCached(string itemCode)
		{
			if (String.IsNullOrEmpty(itemCode))
				throw new ArgumentNullException(nameof(itemCode));

			var file = FilePath(itemCode);
			if (!File.Exists(file))
				return false;

			var text = File.ReadAllText(file);
			long timestamp;
			if (!Int64.TryParse(text, out timestamp))
				return false;

			var date = DateTime.FromBinary(timestamp);
			if (DateTime.UtcNow > date)
				return false;

			return true;
		}

		/// <summary>
		/// Sets specified item as cached with a certain timeout.
		/// </summary>
		public void SetCache(string itemCode, TimeSpan cacheTimeout)
		{
			if (String.IsNullOrEmpty(itemCode))
				throw new ArgumentNullException(nameof(itemCode));

			var expiration = DateTime.UtcNow.Add(cacheTimeout);
			var timestamp = expiration.ToBinary();

			var file = FilePath(itemCode);
			File.WriteAllText(file, timestamp.ToString());
		}

		/// <summary>
		/// Removes caching state for a specified item.
		/// </summary>
		public void RemoveCache(string itemCode)
		{
			if (String.IsNullOrEmpty(itemCode))
				throw new ArgumentNullException(nameof(itemCode));

			var file = FilePath(itemCode);
			File.Delete(file);
		}
	}
}
