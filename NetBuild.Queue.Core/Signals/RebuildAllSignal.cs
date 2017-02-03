namespace NetBuild.Queue.Core
{
	/// <summary>
	/// A special signal which forces all existing items to rebuild.
	/// </summary>
	public class RebuildAllSignal : ISignal
	{
	}
}
