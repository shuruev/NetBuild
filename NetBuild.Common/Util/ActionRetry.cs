using System;
using System.Threading;

namespace NetBuild.Common
{
	/// <summary>
	/// Small helper class for repeating failed action several times.
	/// </summary>
	public class ActionRetry
	{
		private readonly int m_count;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ActionRetry(int retryCount)
		{
			if (retryCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(retryCount));

			m_count = retryCount;
		}

		/// <summary>
		/// Performs specified action.
		/// </summary>
		public void Do(Action action)
		{
			var i = 0;
			while (true)
			{
				try
				{
					action.Invoke();
					break;
				}
				catch
				{
					i++;
					if (i > m_count)
						throw;

					Thread.Sleep(1000 * i);
				}
			}
		}
	}
}
