using System;
using Exortech.NetReflector;
using NetBuild.Common;
using NetBuild.Queue.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace CCNet.NetBuildQueue.Plugin
{
	public abstract class NetBuildPlugin
	{
		private const int c_maxDegreeOfParallelism = 5;

		protected string m_itemCode;
		protected QueueEngine m_db;

		[ReflectorProperty("item", Required = false)]
		public string ItemName { get; set; }

		[ReflectorProperty("db")]
		public string DbConnection { get; set; }

		[ReflectorProperty("thumbprint", Required = false)]
		public string SecurityThumbprint { get; set; }

		[ReflectorProperty("password", Required = false)]
		public string EncryptedPassword { get; set; }

		protected virtual void Init(string projectName)
		{
			m_itemCode = String.IsNullOrEmpty(ItemName) ? projectName : ItemName;

			var db = InitDb();
			m_db = new QueueEngine(db, c_maxDegreeOfParallelism);

			Log.Debug($"[NETBUILD] Initialized database connection for '{m_itemCode}'.");
		}

		private QueueDb InitDb()
		{
			// will put into configuration once there is a need
			var timeout = TimeSpan.FromSeconds(15);

			Log.Debug("[NETBUILD] Creating database connection using configuration values...");
			if (!DbConnection.Contains("{password}"))
			{
				// not using the encrypted password
				return new QueueDb(DbConnection, timeout);
			}

			// using the encrypted password
			Log.Debug($"[NETBUILD] Using certificate '{SecurityThumbprint}' to decrypt database password...");

			if (String.IsNullOrEmpty(SecurityThumbprint))
				throw new InvalidOperationException("Configuration parameter <thumbprint> should be defined if <db> is using encrypted '{password}' syntax.");

			if (String.IsNullOrEmpty(EncryptedPassword))
				throw new InvalidOperationException("Configuration parameter <password> should be defined if <db> is using encrypted '{password}' syntax.");

			var secure = new LocalEncryptor(SecurityThumbprint);
			var password = secure.DecryptUtf8(EncryptedPassword);
			var connection = DbConnection.Replace("{password}", password);

			return new QueueDb(connection, timeout);
		}
	}
}
