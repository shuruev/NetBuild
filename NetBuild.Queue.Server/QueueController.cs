using System.Collections.Generic;
using System.Web.Http;
using NetBuild.Queue.Core;
using NetBuild.Queue.Engine;
using Newtonsoft.Json.Linq;

namespace NetBuild.Queue.Server
{
	public class QueueController : ApiController
	{
		private readonly QueueEngine m_engine;

		public QueueController(QueueEngine engine)
		{
			m_engine = engine;
		}

		[HttpGet]
		[Route("status")]
		public string Status()
		{
			return "OK";
		}

		[HttpPost]
		[Route("process")]
		public List<string> ProcessSignal(string signal, [FromBody]JObject value)
		{
			return m_engine.ProcessSignal(signal, value);
		}

		[HttpGet]
		[Route("check")]
		public List<Modification> ShouldBuild(string item)
		{
			return m_engine.ShouldBuild(item);
		}
	}
}
