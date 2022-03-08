using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CLARTE.Threads
{
	public abstract class Workers
    {
		public class DefaultContext
        {

        }

		/// <summary>
		/// Return the default number of worker threads on the current machine.
		/// </summary>
		public static uint DefaultThreadsCount
		{
			get
			{
				return (uint)Math.Max(Environment.ProcessorCount - 1, 1);
			}
		}
	}

	public class Workers<T> : Workers, IDisposable, IEnumerable<T>
	{
		public class Descriptor
		{
			public delegate T ContextFactory();

			#region Members
			public ContextFactory contextFactory;
			public Action<T, WaitHandle> worker;
			public ICollection<WaitHandle> events;
			public uint nbEvents;
			public uint nbThreads;
			#endregion

			#region Constructor
			/// <summary>
			/// Descriptor for a new group of worker threads.
			/// </summary>
			/// <param name="context_factory">A factory method used to generate context for each worker threads.</param>
			/// <param name="worker">A method executed by each worker thread in an infinite loop. This method get the event that started the new iteration as parameter.</param>
			/// <param name="events">A set of events to wait for before doing the next iteration of the worker loop.</param>
			/// <param name="nb_threads">The number of worker threads to span. If zero, the worker is started in (nb_cpu_cores - 1) threads.</param>
			public Descriptor(ContextFactory context_factory, Action<T, WaitHandle> worker, ICollection<WaitHandle> events, uint nb_threads = 0)
			{
				if (context_factory == null)
				{
					throw new ArgumentNullException("context_factory", "Invalid null context factory method in thread group description.");
				}

				if (worker == null)
				{
					throw new ArgumentNullException("worker", "Invalid null worker method in thread group description.");
				}

				if(events == null)
				{
					throw new ArgumentNullException("events", "Invalid null event collection in thread group description.");
				}

				contextFactory = context_factory;
				this.worker = worker;
				this.events = events;

				nbEvents = (uint) events.Count(x => x != null);

				if(nbEvents == 0)
				{
					throw new ArgumentNullException("events", "Invalid empty event collection in thread group description.");
				}

				nbThreads = nb_threads != 0 ? nb_threads : DefaultThreadsCount;
			}
			#endregion
		}

		#region Members
		protected List<Thread> threads;
		protected List<T> contexts;
		protected ManualResetEvent stopEvent;
		protected bool disposed;
        #endregion

        #region Getters / Setters
		/// <summary>
		/// Return the number of worker threads used.
		/// </summary>
		public uint ThreadsCount
        {
			get
            {
				return (uint) threads.Count;
            }
        }
		#endregion

		#region Constructors / Destructors
		/// <summary>
		/// Create a new group of worker threads.
		/// </summary>
		/// <param name="descriptors">A collection of descriptors for the worker threads to start, each with the events they will wait for.</param>
		public void Init(params Descriptor[] descriptors)
		{
			if(disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.Workers", "The thread group is already disposed.");
			}

			if(descriptors == null || descriptors.Length == 0)
			{
				throw new ArgumentNullException("descriptors", "Invalid empty descriptors collection in thread group initialization.");
			}

			if(threads == null)
			{
				uint nb_threads = 0;

				foreach(Descriptor desc in descriptors)
				{
					nb_threads += desc.nbThreads;
				}

				threads = new List<Thread>((int) nb_threads);
				contexts = new List<T>((int) nb_threads);

				stopEvent = new ManualResetEvent(false);

				foreach(Descriptor desc in descriptors)
				{
					for(int i = 0; i < desc.nbThreads; i++)
					{
						T context = desc.contextFactory();

						Thread thread = new Thread(() => Worker(desc, context));

						threads.Add(thread);
						contexts.Add(context);
					}
				}

				foreach(Thread thread in threads)
				{
					thread.Start();
				}
			}
		}
		#endregion

		#region IDisposable implementation
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				if(disposing)
				{
					// TODO: delete managed state (managed objects).

					if(stopEvent != null && threads != null && threads.Count > 0)
					{
						try
						{
							stopEvent.Set();
						}
						catch(ObjectDisposedException)
						{
							// Fixed errors in Unity editor
						}

						foreach(Thread thread in threads)
						{
							thread.Join();
						}

						threads.Clear();

						try
						{
#if NETFX_CORE
							stopEvent.Dispose();
#else
							stopEvent.Close();
#endif
						}
						catch(ObjectDisposedException)
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
		~Workers()
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
		public IEnumerator<T> GetEnumerator()
		{
			return contexts.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Worker
		protected void Worker(Descriptor descriptor, T context)
		{
			uint events_count = descriptor.nbEvents + 1;
			int event_idx = 0;
			
			// Generate the list of events the worker will be waiting for
			WaitHandle[] wait = new WaitHandle[events_count];

			wait[event_idx++] = stopEvent;

			foreach(WaitHandle ev in descriptor.events)
			{
				if(ev != null)
				{
					wait[event_idx++] = ev;
				}
			}

			try
			{
				// Wait for events to call the worker callback
				while((event_idx = WaitHandle.WaitAny(wait)) != 0)
				{
					descriptor.worker(context, wait[event_idx]);
				}
			}
			catch(ObjectDisposedException)
			{
				// Fixed errors in Unity editor
			}

			// Cleanup of events
			for(uint i = 1; i < wait.Length; i++)
			{
				try
				{
#if NETFX_CORE
					wait[i].Dispose();
#else
					wait[i].Close();
#endif
				}
				catch(ObjectDisposedException)
				{
					// Fixed errors in Unity editor
				}
			}
		}
        #endregion
    }
}
