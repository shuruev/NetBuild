using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	public interface ISignal
	{
		[JsonIgnore]
		string SignalType { get; }
	}
}
