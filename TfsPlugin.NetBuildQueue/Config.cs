using System;
using System.Configuration;
using Atom.Toolbox;
using NetBuild.Common;

namespace TfsPlugin.NetBuildQueue
{
	public static class Config
	{
		public static bool Loaded { get; set; }

		public static string DbConnection { get; set; }
		public static TimeSpan DbTimeout { get; set; }

		public static bool Load(string assemblyPath)
		{
			if (Loaded)
				return false;

			var config = ConfigurationManager.OpenExeConfiguration(assemblyPath);
			var reader = new AppSettingsSectionReader(config.AppSettings);

			var thumbprint = reader.Get<string>("Security.Thumbprint");
			var secure = new LocalEncryptor(thumbprint);

			var dbConnection = reader.Get<string>("NetBuild.DbConnection");
			if (dbConnection.Contains("{password}"))
			{
				var dbPassword = secure.DecryptUtf8(reader.Get<string>("NetBuild.DbPassword"));
				dbConnection = dbConnection.Replace("{password}", dbPassword);
			}

			DbConnection = dbConnection;
			DbTimeout = reader.Get<TimeSpan>("NetBuild.DbTimeout");

			Loaded = true;
			return true;
		}
	}
}
