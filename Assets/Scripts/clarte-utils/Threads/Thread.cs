using System;

#if NETFX_CORE
// On UWP platforms, threads are not available. Therefore, we need support for Tasks, i.e. .Net version >= 4
using InternalThread = System.Threading.Tasks.Task;
#else
using InternalThread = System.Threading.Thread;
#endif

namespace CLARTE.Threads
{
	public class Thread
	{
		#region Members
#if !NETFX_CORE
		protected static int? mainThreadID;
#endif
		protected InternalThread thread;
		#endregion

		#region Constructors
#if !NETFX_CORE
		static Thread()
		{
			mainThreadID = InternalThread.CurrentThread.ManagedThreadId;
		}
#endif

		public Thread(Action start)
		{
#if NETFX_CORE
			thread = new InternalThread(start, System.Threading.Tasks.TaskCreationOptions.LongRunning);
#else
			thread = new InternalThread(new System.Threading.ThreadStart(start));
#endif
		}
		#endregion

		#region Public methods
		public void Start()
		{
			if(thread != null)
			{
				thread.Start();
			}
		}

		public void Join()
		{
			if(thread != null)
			{
#if NETFX_CORE
				thread.Wait();
#else
				thread.Join();
#endif
			}
		}

		public static bool IsMainThread
		{
			get
			{
#if NETFX_CORE
				return !InternalThread.CurrentId.HasValue;
#else
				return (InternalThread.CurrentThread.ManagedThreadId == mainThreadID);
#endif
			}
		}
		#endregion
	}
}
