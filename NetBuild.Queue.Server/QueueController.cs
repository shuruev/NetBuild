using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NetBuild.Queue.Engine;

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
		public string Check()
		{
			return "OK";
		}

		[HttpGet]
		[Route("test")]
		public bool Test()
		{
			return m_engine.ShouldBuild("Project5").Count > 0;
		}
	}
}
