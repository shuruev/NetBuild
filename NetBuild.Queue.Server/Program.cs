using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace NetBuild.Queue.Server
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var url = $"http://localhost:{GetPort(args)}/";

			using (WebApp.Start(url, app => new Startup().Configuration(app)))
			{
				var client = new HttpClient();
				var response = client.GetAsync(url + "status").Result;

				Console.WriteLine(response);
				Console.WriteLine(response.Content.ReadAsStringAsync().Result);

				Console.WriteLine();
				Console.WriteLine(url);
				Console.WriteLine("Server started... Press Enter to exit.");
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
