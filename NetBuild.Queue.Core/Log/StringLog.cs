using System;
using System.Text;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Appends diagnostics messages to a string builder.
	/// </summary>
	public class StringLog : ILog
	{
		private readonly StringBuilder m_sb;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public StringLog(StringBuilder sb)
		{
			if (sb == null)
				throw new ArgumentNullException(nameof(sb));

			m_sb = sb;
		}

		/// <summary>
		/// Writes diagnostics message.
		/// </summary>
		public void Log(string message)
		{
			m_sb.AppendLine(message);
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return m_sb.ToString();
		}
	}
}
