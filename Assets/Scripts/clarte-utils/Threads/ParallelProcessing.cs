using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CLARTE.Threads
{
	public abstract class ParallelProcessing
	{
		public abstract class Context<T> : IDisposable
        {
			public Queue<T> tasks;
			public ManualResetEvent addEvent;
			protected bool disposed;

			public Context()
            {
				tasks = new Queue<T>();

				addEvent = new ManualResetEvent(false);
			}

			#region IDisposable implementation
			protected virtual void Dispose(bool disposing)
			{
				if (!disposed)
				{
					if (disposing)
					{
						// TODO: delete managed state (managed objects).

						if (addEvent != null)
						{
							try
							{
#if NETFX_CORE
								addEvent.Dispose();
#else
								addEvent.Close();
#endif
							}
							catch (ObjectDisposedException)
							{
								// Fixed errors in Unity editor
							}
						}
					}

					// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
					// TODO: set fields of large size with null value.

					disposed = true;
				}
			}

			// TODO: replace finalizer only if the above Dispose(bool disposing) function as code to free unmanaged resources.
			~Context()
			{
				Dispose(/*false*/);
			}

			/// <summary>
			/// Dispose of the context.
			/// </summary>
			public void Dispose()
			{
				// Pass true in dispose method to clean managed resources too and say GC to skip finalize in next line.
				Dispose(true);

				// If dispose is called already then say GC to skip finalize on this instance.
				// TODO: uncomment next line if finalizer is replaced above.
				GC.SuppressFinalize(this);
			}
            #endregion
        }
    }

	/// <summary>
	/// A thread pool for parallel processing of identical data.
	/// </summary>
	public class ParallelProcessing<T, U> : IDisposable, IEnumerable<U> where U : ParallelProcessing.Context<T>
	{
		public delegate U ContextFactory();
		public delegate void Process(U context, T data);

		#region Members
		protected Process algorithm;
		protected List<Thread> threads;
		protected List<U> contexts;
		protected ManualResetEvent stopEvent;
		protected ManualResetEvent completedEvent;
		protected int taskCount; // We can not use the length of the tasks queue because when the lasts task is removed from the queue, it is still executed and WaitForTasksCompletion should continue to wait.
		protected int current;
		protected bool disposed;
		#endregion

		#region Constructors / Destructors
		/// <summary>
		/// Create a new thread pool for processing data in parallel.
		/// </summary>
		/// <param name="algorithm">Algorithm used to process the queued data.</param>
		/// <param name="context_factory">a factory method used to generate context for each of the worker threads</param>
		/// <param name="nb_threads">The number of worker threads to span. If zero, the worker is started in (nb_cpu_cores - 1) threads.</param>
		public ParallelProcessing(Process algorithm, ContextFactory context_factory, uint nb_threads = 0)
		{
			if (algorithm == null)
			{
				throw new ArgumentNullException("algorithm", "Invalid null algorithm method in parallel processing constructor.");
			}

			if (context_factory == null)
			{
				throw new ArgumentNullException("context_factory", "Invalid null context factory method in parallel processing constructor.");
			}

			this.algorithm = algorithm;

			completedEvent = new ManualResetEvent(true);

			current = 0;

			if (threads == null)
			{
				if(nb_threads <= 0)
                {
					nb_threads = Workers.DefaultThreadsCount;
                }

				threads = new List<Thread>((int) nb_threads);
				contexts = new List<U>((int) nb_threads);

				stopEvent = new ManualResetEvent(false);

				for (int i = 0; i < nb_threads; i++)
				{
					U context = context_factory();

					Thread thread = new Thread(() => Worker(context));

					threads.Add(thread);
					contexts.Add(context);
				}

				foreach (Thread thread in threads)
				{
					thread.Start();
				}
			}
		}
		#endregion

		#region IDisposable implementation
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// TODO: delete managed state (managed objects).

					if (stopEvent != null && threads != null && threads.Count > 0)
					{
						try
						{
							stopEvent.Set();
						}
						catch (ObjectDisposedException)
						{
							// Fixed errors in Unity editor
						}

						foreach (Thread thread in threads)
						{
							thread.Join();
						}

						threads.Clear();

						try
						{
#if NETFX_CORE
							stopEvent.Dispose();
							completedEvent.Dispose();
#else
							stopEvent.Close();
							completedEvent.Close();
#endif
						}
						catch (ObjectDisposedException)
						{
							// Fixed errors in Unity editor
						}

						foreach(U context in contexts)
                        {
							context.Dispose();
                        }
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
				// TODO: set fields of large size with null value.

				disposed = true;
			}
		}

		// TODO: replace finalizer only if the above Dispose(bool disposing) function as code to free unmanaged resources.
		~ParallelProcessing()
		{
			Dispose(/*false*/);
		}

		/// <summary>
		/// Dispose of the thread pool. Wait for curently executing async task to complete and release all the allocated threads.
		/// </summary>
		/// <remarks>Note that async tasks that are planned but not started yet will be discarded.</remarks>
		public void Dispose()
		{
			// Pass true in dispose method to clean managed resources too and say GC to skip finalize in next line.
			Dispose(true);

			// If dispose is called already then say GC to skip finalize on this instance.
			// TODO: uncomment next line if finalizer is replaced above.
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IEnumerable implementation
		public IEnumerator<U> GetEnumerator()
		{
			return contexts.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Worker
		protected void Worker(U context)
		{
			int event_idx;

			// Generate the list of events the worker will be waiting for
			WaitHandle[] wait = new WaitHandle[] { stopEvent, context.addEvent };

			try
			{
				// Wait for events to call the worker callback
				while ((event_idx = WaitHandle.WaitAny(wait)) != 0)
				{
					T data = default;

					bool has_data = false;

					lock (context.tasks)
					{
						if (context.tasks.Count > 0)
						{
							data = context.tasks.Dequeue();

							has_data = true;
						}
						else
						{
							// Nothing to do anymore, go to sleep
							context.addEvent.Reset();
						}
					}

					if (has_data)
					{
						algorithm(context, data);

						Interlocked.Decrement(ref taskCount);

						if (taskCount <= 0)
						{
							completedEvent.Set();
						}
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Fixed errors in Unity editor
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Add a new data to be processed asynchronously.
		/// </summary>
		/// <param name="data">The data to process.</param>
		public void AddData(T data)
		{
			if (disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.ParallelProcessing", "The processing pool is already disposed.");
			}

			if (data != null)
			{
				Interlocked.Increment(ref taskCount);

				completedEvent.Reset();

				U context = contexts[current];

				current = (current + 1) % contexts.Count;

				lock (context.tasks)
				{
					context.tasks.Enqueue(data);
				}

				context.addEvent.Set();
			}
		}

		/// <summary>
		/// Get the number of tasks currentlty planned or executing.
		/// </summary>
		/// <returns>The number of tasks.</returns>
		public long TaskCount()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.ParallelProcessing", "The processing pool is already disposed.");
			}

			return taskCount;
		}

		/// <summary>
		/// Wait for all tasks (planned or executing) to complete. This is a blocking barrier instruction.
		/// </summary>
		public void WaitUntilTasksCompletion()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.ParallelProcessing", "The processing pool is already disposed.");
			}

			completedEvent.WaitOne();
		}

		/// <summary>
		/// Wait for all tasks (planned or executing) to complete. This is a non-blocking barrier instruction.
		/// </summary>
		/// <returns>An enumerator that will return null as long as some tasks are present.</returns>
		public IEnumerator WaitForTasksCompletion()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.ParallelProcessing", "The processing pool is already disposed.");
			}

			while (TaskCount() > 0)
			{
				yield return null;
			}
		}
		#endregion
	}
}
