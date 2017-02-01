namespace NetBuild.Queue.Client
{
	public class QueueClient : ApiClient
	{
		public QueueClient(string baseUrl)
			: base(baseUrl)
		{
		}

		public bool Test(string item)
		{
			return Execute<bool>(HttpGet("test"));
		}
	}
}
