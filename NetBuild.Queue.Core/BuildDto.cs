using System;

namespace NetBuild.Queue.Core
{
	public class BuildDto
	{
		public long BuildId { get; set; }
		public string ModificationCode { get; set; }
		public string ModificationType { get; set; }
		public string ModificationAuthor { get; set; }
		public string ModificationItem { get; set; }
		public string ModificationComment { get; set; }
		public DateTime? ModificationDate { get; set; }
		public DateTime Created { get; set; }
	}
}
