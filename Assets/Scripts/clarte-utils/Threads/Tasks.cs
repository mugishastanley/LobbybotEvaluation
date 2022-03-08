using System;

namespace CLARTE.Threads
{
	/// <summary>
	/// Helper class providing a global thread pool for the application.
	/// </summary>
	public class Tasks
	{
		#region Members
		private static Pool threads = new Pool();
		#endregion

		#region Destructors
		~Tasks()
		{
			if(threads != null)
			{
				threads.Dispose();

				threads = null;
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Add a new task to be executed asynchronously.
		/// </summary>
		/// <param name="task">A method (task) that does not return any value.</param>
		/// <returns>A helper class to be notified when the task is complete.</returns>
		public static Result Add(Action task)
		{
			return threads.AddTask(task);
		}

		/// <summary>
		/// Add a new task to be executed asynchronously.
		/// </summary>
		/// <typeparam name="T">The type of the returned value.</typeparam>
		/// <param name="task">A method (task) that does return a value.</param>
		/// <returns>A helper class to be notified when the task is complete and get the returned value.</returns>
		public static Result<T> Add<T>(Func<T> task)
		{
			return threads.AddTask(task);
		}

		/// <summary>
		/// Get the number of tasks currentlty planned or executing.
		/// </summary>
		/// <returns>The number of tasks.</returns>
		public static long TaskCount()
		{
			return threads.TaskCount();
		}

		public static uint ThreadsCount()
        {
			return threads.ThreadsCount;
        }
		#endregion
	}
}
