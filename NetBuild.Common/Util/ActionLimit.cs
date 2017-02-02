using System;
using System.Threading;

namespace NetBuild.Common
{
	/// <summary>
	/// Small helper class for limiting concurrent calls.
	/// </summary>
	public class ActionLimit
	{
		private readonly SemaphoreSlim m_semaphore;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ActionLimit(int maximumCount)
		{
			if (maximumCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumCount));

			m_semaphore = new SemaphoreSlim(maximumCount, maximumCount);
		}

		/// <summary>
		/// Performs specified action.
		/// </summary>
		public void Do(Action action)
		{
			m_semaphore.Wait();

			try
			{
				action.Invoke();
			}
			finally
			{
				m_semaphore.Release();
			}
		}
	}
}
