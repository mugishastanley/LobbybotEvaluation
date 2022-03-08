using System;
using CLARTE.Pattern;

namespace CLARTE.Threads.APC
{
	/// <summary>
	/// Utility class to execute code in unity thread.
	/// </summary>
	/// <remarks>
	/// This class is based on the Reactor design pattern. It allows various threads
	/// to call callbacks back in the unity thread and be notified of the end and result
	/// value of the operation.
	/// </remarks>
	public class MonoBehaviourCall : Singleton<MonoBehaviourCall>, ICall
	{
		#region Members
		protected Reactor reactor = new Reactor();
		#endregion

		#region Constructors
		protected MonoBehaviourCall()
		{
			// To comply with Singleton
		}
		#endregion

		#region MonoBehaviour callbacks
		protected void Update()
		{
			reactor.Update();
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Add a task that will return a value.
		/// </summary>
		/// <typeparam name="T">The type of the return value.</typeparam>
		/// <param name="callback">The task to execute.</param>
		/// <returns>The return value of the task embedded in a Result object.</returns>
		public Result<T> Call<T>(Func<T> callback)
		{
			Task task = Task.Create(callback);

			reactor.Add(task, false);

			return (Result<T>) task.result;
		}

		/// <summary>
		/// A task that will not return a value.
		/// </summary>
		/// <param name="callback">The task to execute.</param>
		/// <returns>An empty Result object to get notified of the task completion and raised exceptions.</returns>
		public Result Call(Action callback)
		{
			Task task = Task.Create(callback);

			reactor.Add(task, false);

			return (Result) task.result;
		}
		#endregion
	}
}
