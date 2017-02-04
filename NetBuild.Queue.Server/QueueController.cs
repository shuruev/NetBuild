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
		[Route("triggers/{item}")]
		public bool SetTriggers(string item, string type, [FromBody]List<JObject> values)
		{
			return m_engine.SetTriggers(item, type, values);
		}

		[HttpPost]
		[Route("signal")]
		public List<string> ProcessSignal(string type, [FromBody]JObject value)
		{
			return m_engine.ProcessSignal(type, value);
		}

		[HttpPost]
		[Route("build/check/{item}")]
		public List<Modification> ShouldBuild(string item)
		{
			return m_engine.ShouldBuild(item);
		}

		[HttpPost]
		[Route("build/start/{item}")]
		public void StartBuild(string item, string label)
		{
			m_engine.StartBuild(item, label);
		}

		[HttpPost]
		[Route("build/complete/{item}")]
		public void CompleteBuild(string item, string label)
		{
			m_engine.CompleteBuild(item, label);
		}
	}
}
