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
		[Route("set")]
		public bool SetTriggers(string item, string trigger, [FromBody]List<JObject> values)
		{
			return m_engine.SetTriggers(item, trigger, values);
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

		[HttpPost]
		[Route("start")]
		public void StartBuild(string item, string label)
		{
			m_engine.StartBuild(item, label);
		}

		[HttpPost]
		[Route("complete")]
		public void CompleteBuild(string item, string label)
		{
			m_engine.CompleteBuild(item, label);
		}

		[HttpPost]
		[Route("stop")]
		public void StopBuild(string item, string label)
		{
			m_engine.StartBuild(item, label);
		}
	}
}
