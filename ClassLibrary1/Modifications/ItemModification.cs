namespace NetBuild.Queue.Engine
{
	/// <summary>
	/// Represents a single modification for the specific item.
	/// </summary>
	public class ItemModification
	{
		/// <summary>
		/// Gets or sets build item code.
		/// </summary>
		public string Item { get; set; }

		/// <summary>
		/// Gets or sets modification data.
		/// </summary>
		public Modification Modification { get; set; }
	}
}
