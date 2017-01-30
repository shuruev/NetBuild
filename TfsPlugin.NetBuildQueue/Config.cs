using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Atom.Toolbox;
using NetBuild.Common;

namespace TfsPlugin.NetBuildQueue
{
	/// <summary>
	/// Small static helper for accessing configuration values.
	/// </summary>
	public static class Config
	{
		/// <summary>
		/// Gets or sets a value indicating whether configuration was already loaded.
		/// </summary>
		public static bool Loaded { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether we should apply extended logging.
		/// </summary>
		public static bool DebugMode { get; set; }

		/// <summary>
		/// Gets or sets local cache folder.
		/// </summary>
		public static string LocalCache { get; set; }

		/// <summary>
		/// Gets or sets database connection string.
		/// </summary>
		public static string DbConnection { get; set; }

		/// <summary>
		/// Gets or sets database command timeout.
		/// </summary>
		public static TimeSpan DbTimeout { get; set; }

		/// <summary>
		/// Loads configuration from the specified TFS plugin folder.
		/// It locates the plugin DLL and loads values from the corresponding *.dll.config file.
		/// </summary>
		public static bool Load(string plugInDirectory)
		{
			if (Loaded)
				return false;

			var assemblyName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
			var assemblyPath = Path.Combine(plugInDirectory, assemblyName);

			var config = ConfigurationManager.OpenExeConfiguration(assemblyPath);
			var reader = new AppSettingsSectionReader(config.AppSettings);

			DebugMode = reader.Get<bool>("Debug.Enabled");

			Log.Debug($"Loading configuration from: {assemblyPath}");

			LocalCache = reader.Get<string>("NetBuild.LocalCache", null);

			var dbConnection = reader.Get<string>("NetBuild.DbConnection");
			if (dbConnection.Contains("{password}"))
			{
				var thumbprint = reader.Get<string>("Security.Thumbprint");
				var secure = new LocalEncryptor(thumbprint);

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
