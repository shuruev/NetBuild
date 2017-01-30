using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	public interface ITrigger
	{
		[JsonIgnore]
		string TriggerType { get; }
	}
}
