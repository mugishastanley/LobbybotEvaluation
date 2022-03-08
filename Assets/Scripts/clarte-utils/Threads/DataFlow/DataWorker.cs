using System;
using System.Threading;

namespace CLARTE.Threads.DataFlow
{
	/// <summary>
	/// Class that transform some input data into output data.
	/// </summary>
	/// <typeparam name="InputType">The type of input data.</typeparam>
	/// <typeparam name="OutputType">The type of output data.</typeparam>
	public class DataWorker<InputType, OutputType>: IDataProvider<OutputType> where InputType: ICloneable
	{
		#region Members
		/// <summary>
		/// Delegate used to provides output data.
		/// </summary>
		public event ProvideDataDelegate<OutputType> ProvideDataEvent;

		/// <summary>
		/// Delegate to transform data.
		/// </summary>
		public WorkOnDataDelegate<InputType, OutputType> WorkOnData;

		/// <summary>
		/// Timeout used to raise exception if no data have been received in a given amount of time.
		/// </summary>
		public int millisecondsTimeout = 1000;

		private InputType inputData;
		private Exception exception;
		private Barrier barrier;
		private AutoResetEvent enqueue = new AutoResetEvent(true);
		#endregion

		#region Getters / Setters
		/// <summary>
		/// Check if one exception was raised.
		/// </summary>
		public bool HasException
		{
			get
			{
				return exception != null;
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Register this worker to wait at a given barrier.
		/// </summary>
		/// <param name="barrier">The barrier to wait at.</param>
		public void RegisterBarrier(Barrier barrier)
		{
			if (this.barrier != null)
			{
				this.barrier.RemoveParticipant();
			}

			this.barrier = barrier;
			this.barrier.AddParticipant();
		}

		/// <summary>
		/// ProvideDataDelegate implementation to clone data and start transformation on it.
		/// </summary>
		/// <param name="data">The data to work on.</param>
		/// <param name="clone">True if the data must be cloned, false otherwise.</param>
		public void EnqeueTask(InputType data, bool clone)
		{
			inputData = clone && data != null ? (InputType)data.Clone() : data;

			if (!enqueue.WaitOne(millisecondsTimeout))
			{
				throw new TimeoutException(string.Format("ConsumeData is too long, new data is waiting for {0} milliseconds.", millisecondsTimeout));
			}

			if (barrier != null && !barrier.SignalAndWait(millisecondsTimeout))
			{
				throw new TimeoutException(string.Format("A barrier participant is too long, new data is waiting for {0} milliseconds.", millisecondsTimeout));
			}

			Tasks.Add(AsyncWork);
		}

		/// <summary>
		/// Rethrow a received exception.
		/// </summary>
		public void Throw()
		{
			if (exception != null)
			{
				throw new Exception("Exception occurred", exception);
			}
		}
		#endregion

		#region Internal methods
		private void AsyncWork()
		{
			try
			{
				OutputType data = WorkOnData(inputData);

				if (ProvideDataEvent != null)
				{
					bool clone = ProvideDataEvent.GetInvocationList().Length > 1;

					ProvideDataEvent.Invoke(data, clone);
				}
			} catch (Exception ex)
			{
				exception = ex;
			}

			enqueue.Set();
		}
		#endregion
	}
}
