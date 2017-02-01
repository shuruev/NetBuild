namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// Represents a single trigger entry for the specific item.
	/// </summary>
	public class ItemTrigger
	{
		/// <summary>
		/// Gets or sets build item code.
		/// </summary>
		public string Item { get; set; }

		/// <summary>
		/// Gets or sets trigger data.
		/// </summary>
		public ITrigger Trigger { get; set; }
	}
}
