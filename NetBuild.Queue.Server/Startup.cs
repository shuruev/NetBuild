using System;
using System.Web.Http;
using Atom.Toolbox;
using NetBuild.Common;
using NetBuild.Queue.Engine;
using Owin;
using Serilog;
using Serilog.Core;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace NetBuild.Queue.Server
{
	public class Startup
	{
		/// <summary>
		/// Main entry point for ASP.NET application.
		/// Please note that both name and signature of this method cannot be changed,
		/// since it's used when we host this assembly under IIS for development purposes.
		/// </summary>
		public void Configuration(IAppBuilder app)
		{
			var logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.LiterateConsole()
				.CreateLogger();

			try
			{
				var config = new HttpConfiguration();
				logger.Debug("Configuring service...");

				// setup IoC container and services
				var container = new Container();
				container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();

				SetupServices(container, logger);

				container.RegisterWebApiControllers(config);
				container.Verify();

				// configure application
				config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
				config.MapHttpAttributeRoutes();
				config.EnsureInitialized();

				// start application
				app.UseWebApi(config);
				logger.Information("Service started.");
			}
			catch (Exception e)
			{
				logger.Error(e, "Service failed to start.");
				throw;
			}
		}

		public void SetupServices(Container container, Logger logger)
		{
			var config = new AppConfigReader();

			var dbConnection = config.Get<string>("NetBuild.DbConnection");
			if (dbConnection.Contains("{password}"))
			{
				var thumbprint = config.Get<string>("Security.Thumbprint");
				var secure = new LocalEncryptor(thumbprint);

				var dbPassword = secure.DecryptUtf8(config.Get<string>("NetBuild.DbPassword"));
				dbConnection = dbConnection.Replace("{password}", dbPassword);
			}

			var dbTimeout = config.Get<TimeSpan>("NetBuild.DbTimeout");
			var maxConcurrentBuilds = config.Get<int>("NetBuild.MaxConcurrentBuilds");

			// TODO: add config parameters to override logging
			var log = new SerilogAdapter(logger);

			var triggers = new Triggers(new TriggerStorage(dbConnection, dbTimeout));
			var modifications = new Modifications(new ModificationStorage(dbConnection, dbTimeout));

			var engine = new QueueEngine(triggers, modifications, log);
			engine.AddDetector(new SourceChangedDetector());
			engine.AddDetector(new BuildCompleteDetector());
			engine.AddDetector(new RebuildAllDetector());
			engine.AddDetector(new ConcurrentBuildDetector(maxConcurrentBuilds));

			engine.Load();

			container.RegisterSingleton(engine);
		}
	}
}
