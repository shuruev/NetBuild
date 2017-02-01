using Newtonsoft.Json;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Represents trigger which is used to react upon signals.
	/// </summary>
	public interface ITrigger
	{
		/// <summary>
		/// Gets trigger type (e.g. 'ReferenceItem').
		/// </summary>
		[JsonIgnore]
		string TriggerType { get; }
	}
}
