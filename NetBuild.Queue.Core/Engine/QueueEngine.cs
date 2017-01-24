using System;
using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	public class QueueEngine
	{
		private readonly QueueDb m_db;
		private readonly JsonSerializerSettings m_settings;

		public QueueEngine(QueueDb db)
		{
			if (db == null)
				throw new ArgumentNullException(nameof(db));

			m_db = db;
			m_settings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};
		}

		public void ProcessSignal<TSignal>(TSignal signal) where TSignal : ISignal
		{
			if (signal == null)
				throw new ArgumentNullException(nameof(signal));

			m_db.ProcessSignal(signal.SignalType, JsonConvert.SerializeObject(signal, m_settings));
		}
	}
}
