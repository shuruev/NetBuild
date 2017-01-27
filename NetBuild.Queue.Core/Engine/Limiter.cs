using System;
using System.Threading;

namespace NetBuild.Queue.Core
{
	/// <summary>
	/// Small helper for limiting concurrent calls using named semaphore.
	/// </summary>
	public class Limiter
	{
		/// <summary>
		/// Represents a step locked over a semaphore.
		/// </summary>
		public class Lock : IDisposable
		{
			private readonly Semaphore m_semaphore;

			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			public Lock(string semaphoreName, int maxParallelRequests)
			{
				m_semaphore = new Semaphore(maxParallelRequests, maxParallelRequests, semaphoreName);
				m_semaphore.WaitOne();
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				m_semaphore.Release();
			}
		}

		private readonly string m_semaphoreName;
		private readonly int m_maxParallelRequests;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public Limiter(string semaphoreName, int maxParallelRequests)
		{
			if (String.IsNullOrWhiteSpace(semaphoreName))
				throw new ArgumentException("Semaphore name should not be null or whitespace.", nameof(semaphoreName));

			if (maxParallelRequests <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxParallelRequests));

			m_semaphoreName = semaphoreName;
			m_maxParallelRequests = maxParallelRequests;
		}

		/// <summary>
		/// Blocks the current thread until the concurrency level is reduced.
		/// </summary>
		public Lock Wait()
		{
			return new Lock(m_semaphoreName, m_maxParallelRequests);
		}
	}
}
