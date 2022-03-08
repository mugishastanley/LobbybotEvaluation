using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CLARTE.Threads
{
	/// <summary>
	/// A thread pool to avoid creating a new thread for every async tasks.
	/// </summary>
	public class Pool : Workers<Workers.DefaultContext>, IDisposable
	{
		#region Members
		protected Queue<Task> tasks;
		protected ManualResetEvent addEvent;
		protected ManualResetEvent completedEvent;
		protected object taskCountMutex;
		protected int taskCount; // We can not use the length of the tasks queue because when the lasts task is removed from the queue, it is still executed and WaitForTasksCompletion should continue to wait.
		#endregion

		#region Constructors / Destructors
		/// <summary>
		/// Create a new thread pool for oneshot tasks.
		/// </summary>
		/// <param name="nb_threads">The number of worker threads to span. If zero, the worker is started in (nb_cpu_cores - 1) threads.</param>
		public Pool(uint nb_threads = 0)
		{
			tasks = new Queue<Task>();

			addEvent = new ManualResetEvent(false);
			completedEvent = new ManualResetEvent(true);

			taskCountMutex = new object();

			Init(new Descriptor(() => new DefaultContext(), Worker, new ManualResetEvent[] { addEvent }, nb_threads));
		}
		#endregion

		#region IDisposable implementation
		protected override void Dispose(bool disposing)
        {
			if (!disposed)
			{
				base.Dispose(disposing);

				if (disposing)
				{
#if NETFX_CORE
					addEvent.Dispose();
					completedEvent.Dispose();
#else
					addEvent.Close();
					completedEvent.Close();
#endif
				}
			}
        }
		#endregion

		#region Worker
		protected void AddTask(Task task)
		{
			if(disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.Pool", "The thread pool is already disposed.");
			}

			if(task != null)
			{
				lock(taskCountMutex)
				{
					taskCount++;

					completedEvent.Reset();
				}

				lock(tasks)
				{
					tasks.Enqueue(task);
				}

				addEvent.Set();
			}
		}

		protected void Worker(Workers.DefaultContext _, WaitHandle ev)
		{
			if(ev == addEvent)
			{
				Task task = null;

				lock(tasks)
				{
					if(tasks.Count > 0)
					{
						task = tasks.Dequeue();
					}
					else
					{
						// Nothing to do anymore, go to sleep
						addEvent.Reset();
					}
				}

				if(task != null)
				{
					task.callback();
					
					lock(taskCountMutex)
					{
						taskCount--;

						if (taskCount <= 0)
						{
							completedEvent.Set();
						}
					}
				}
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Add a new task to be executed asynchronously.
		/// </summary>
		/// <param name="task">A method (task) that does not return any value.</param>
		/// <returns>A helper class to be notified when the task is complete.</returns>
		public Result AddTask(Action task)
		{
			Task t = Task.Create(task);

			if(t != null)
			{
				AddTask(t);

				return (Result) t.result;
			}

			return null;
		}

		/// <summary>
		/// Add a new task to be executed asynchronously.
		/// </summary>
		/// <typeparam name="T">The type of the returned value.</typeparam>
		/// <param name="task">A method (task) that does return a value.</param>
		/// <returns>A helper class to be notified when the task is complete and get the returned value.</returns>
		public Result<T> AddTask<T>(Func<T> task)
		{
			Task t = Task.Create(task);

			if(t != null)
			{
				AddTask(t);

				return (Result<T>) t.result;
			}

			return null;
		}

		/// <summary>
		/// Get the number of tasks currentlty planned or executing.
		/// </summary>
		/// <returns>The number of tasks.</returns>
		public long TaskCount()
		{
			if(disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.Pool", "The thread pool is already disposed.");
			}

			lock(taskCountMutex)
			{
				return taskCount;
			}
		}

		/// <summary>
		/// Wait for all tasks (planned or executing) to complete. This is a blocking barrier instruction.
		/// </summary>
		public void WaitUntilTasksCompletion()
		{
			if (disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.Pool", "The thread pool is already disposed.");
			}

			completedEvent.WaitOne();
		}

		/// <summary>
		/// Wait for all tasks (planned or executing) to complete. This is a non-blocking barrier instruction.
		/// </summary>
		/// <returns>An enumerator that will return null as long as some tasks are present.</returns>
		public IEnumerator WaitForTasksCompletion()
		{
			if(disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.Pool", "The thread pool is already disposed.");
			}

			while(TaskCount() > 0)
			{
				yield return null;
			}
		}

		/// <summary>
		/// Wait for all tasks in a given group (planned or executing) to complete. This is a non-blocking barrier instruction.
		/// </summary>
		/// <param name="tasks">A set of results corresponding to the tasks to wait for.</param>
		/// <returns>An enumerator that will return null as long as some tasks are present.</returns>
		public IEnumerator WaitForTasksCompletion(HashSet<Result> tasks)
		{
			if(disposed)
			{
				throw new ObjectDisposedException("CLARTE.Threads.Pool", "The thread pool is already disposed.");
			}

			while(tasks.Count > 0)
			{
				yield return null;

				tasks.RemoveWhere(r => r.Done);
			}
		}

		/// <summary>
		/// Utility method to execute a task and store the result in an array at a given index.
		/// </summary>
		/// <typeparam name="T">The type of the task return value.</typeparam>
		/// <param name="array">The array where to store the result.</param>
		/// <param name="index">The index at which store the result.</param>
		/// <param name="callback">The task to execute.</param>
		public IResult ExecAndSaveToArray<T>(T[] array, int index, Func<T> callback)
		{
			if(index >= 0 && index < array.Length)
			{
				return AddTask(() => array[index] = callback());
			}
			else
			{
				throw new IndexOutOfRangeException(string.Format("Index '{0}' is not a valid index. Accepted values are [0:{1}].", index, array.Length - 1));
			}
		}
		#endregion
	}
}
