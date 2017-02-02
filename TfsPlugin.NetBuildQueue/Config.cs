using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Atom.Toolbox;

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
		/// Gets or sets queue server URL.
		/// </summary>
		public static string ServerUrl { get; set; }

		/// <summary>
		/// Gets or sets queue server URL.
		/// </summary>
		public static TimeSpan Timeout { get; set; }

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

			ServerUrl = reader.Get<string>("NetBuild.ServerUrl");
			Timeout = reader.Get<TimeSpan>("NetBuild.Timeout");

			Loaded = true;
			return true;
		}
	}
}
