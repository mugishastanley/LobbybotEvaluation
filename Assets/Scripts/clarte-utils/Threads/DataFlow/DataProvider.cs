using System;

namespace CLARTE.Threads.DataFlow
{
	/// <summary>
	/// Class that provide some data.
	/// </summary>
	/// <typeparam name="OutputType">The type of provided data.</typeparam>
	public class DataProvider<OutputType> : IDataProvider<OutputType>
	{
		#region Members
		/// <summary>
		/// Delegate used to provides output data.
		/// </summary>
		public event ProvideDataDelegate<OutputType> ProvideDataEvent;

		/// <summary>
		/// Delegate used to get some data.
		/// </summary>
		public CreateDataDelegate<OutputType> CreateData;

		private Thread thread;
		private Exception exception;
		#endregion

		#region Getters / Setters
		/// <summary>
		/// Check if the provider is running.
		/// </summary>
		public bool Running { get; private set; } = false;

		/// <summary>
		/// Check if one exception was raised.
		/// </summary>
		public bool HasException { get { return exception != null; } }
		#endregion

		#region Public methods
		/// <summary>
		/// Start the provider.
		/// </summary>
		public virtual void Start()
		{
			exception = null;
			thread = new Thread(ThreadedDataProvider);
			thread.Start();
		}

		/// <summary>
		/// Stop the provider.
		/// </summary>
		/// <param name="join">Should function join the thread (waiting the end of the thread).</param>
		public virtual void Stop(bool join = true)
		{
			Running = false;
			if (join)
			{
				if (thread != null)
				{
					thread.Join();
				}
				if (exception != null)
				{
					throw new Exception("Exception occurred", exception);
				}
			}
		}
		#endregion

		#region Internal methods
		private void ThreadedDataProvider()
		{
			try
			{
				Running = true;
				while (Running)
				{
					OutputType data = CreateData();

					if (ProvideDataEvent != null)
					{
						bool clone = ProvideDataEvent.GetInvocationList().Length > 1;

						ProvideDataEvent.Invoke(data, clone);
					}
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}
		#endregion
	}
}
