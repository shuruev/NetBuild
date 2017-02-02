using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Owin.Hosting;

namespace NetBuild.Queue.Server
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			// netsh http show urlacl
			// netsh http add urlacl url=http://*:8000/ user=<username> listen=yes
			var url = $"http://*:{GetPort(args)}/";

			using (WebApp.Start(url, app => new Startup().Configuration(app)))
			{
				var status = url.Replace("*", "localhost") + "status";
				Console.WriteLine(status);

				var client = new HttpClient();
				var response = client.GetAsync(status).Result;

				Console.WriteLine(response);
				Console.WriteLine(response.Content.ReadAsStringAsync().Result);

				Console.WriteLine();
				Console.WriteLine($"Server started at {url}");
				Console.WriteLine("Press Enter to exit.");
				Console.ReadLine();
			}
		}

		/// <summary>
		/// Gets port to use for local HTTP listener.
		/// </summary>
		private static int GetPort(string[] args)
		{
			var port = args.FirstOrDefault(arg => arg.StartsWith("-port:"));
			if (port != null)
			{
				return Convert.ToInt32(port.Substring(6));
			}

			return 8000;
		}
	}
}
