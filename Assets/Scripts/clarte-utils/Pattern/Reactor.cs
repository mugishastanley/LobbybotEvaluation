using System.Collections.Generic;
using CLARTE.Threads;

namespace CLARTE.Pattern
{
	/// <summary>
	/// Demultiplexing service provider for asynchronous executions.
	/// </summary>
	/// <remarks>
	/// This component is designed for grouping execution of code
	/// from multiple threads into a single thread.
	/// </remarks>
	public class Reactor
	{
		protected struct Context
		{
			#region Members
			public Task task;
			public bool wait;
			#endregion

			#region Constructors
			public Context(Task task, bool wait)
			{
				this.task = task;
				this.wait = wait;
			}
			#endregion
		}

		#region Members
		protected Queue<Context> pending = new Queue<Context>();
		protected Queue<Context> inProgress = new Queue<Context>();
		protected Context current;
		#endregion

		#region Public methods
		/// <summary>
		/// Method to execute all currently available tasks.
		/// </summary>
		public void Update()
		{
			lock(pending)
			{
				while(pending.Count > 0)
				{
					inProgress.Enqueue(pending.Dequeue());
				}
			}

			while(inProgress.Count > 0 && (current.task == null || !current.wait || current.task.result.Done))
			{
				current = inProgress.Dequeue();

				current.task.callback();
			}
		}

		/// <summary>
		/// Add a new task to be executed.
		/// </summary>
		/// <param name="task">The task to execute.</param>
		/// <param name="wait_before_next_task">
		/// If true, the reactor will wait for the current task to signal it's end before launching the next.
		/// If false, the reactor will launch the next task as soon as the task callback end.
		/// Usually, both are equivalent. However, in constructs where the task is created manually to use a custom Result,
		/// and then span other tasks in other threads, the execution flow might differ.
		/// </param>
		public void Add(Task task, bool wait_before_next_task)
		{
			lock(pending)
			{
				pending.Enqueue(new Context(task, wait_before_next_task));
			}
		}
		#endregion
	}
}
