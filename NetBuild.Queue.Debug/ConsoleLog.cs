using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetBuild.Queue.Core;

namespace NetBuild.Queue.Debug
{
	public class ConsoleLog : ILog
	{
		public void Log(string message)
		{
			Console.WriteLine(message);
		}
	}
}
