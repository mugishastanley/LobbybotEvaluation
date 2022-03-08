using System;

namespace CLARTE.Threads
{
	public class Task
	{
		#region Members
		public Action callback;
		public IResult result;
		#endregion

		#region Constructors
		public Task(Action func, IResult res)
		{
			callback = func;
			result = res;
		}
		#endregion

		#region Public methods
		public static Task Create(Action callback)
		{
			if(callback == null)
			{
				throw new ArgumentNullException("callback", "Invalid null callback in task.");
			}

			Result result = new Result();

			return new Task(() =>
			{
				Exception exception = null;

				try
				{
					callback();
				}
				catch(Exception e)
				{
					exception = e;
				}

				result.Complete(exception);
			}, result);
		}

		public static Task Create<T>(Func<T> callback)
		{
			if(callback == null)
			{
				throw new ArgumentNullException("callback", "Invalid null callback in task.");
			}

			Result<T> result = new Result<T>();

			return new Task(() =>
			{
				T value = default(T);
				Exception exception = null;

				try
				{
					value = callback();
				}
				catch(Exception e)
				{
					exception = e;
				}

				result.Complete(value, exception);
			}, result);
		}
		#endregion
	}
}
