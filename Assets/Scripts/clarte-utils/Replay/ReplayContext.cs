using UnityEngine;

namespace CLARTE.Replay
{
	/// <summary>
	/// Inherit to create your own replay context.
	/// It should contain the status of the context all methods that modify the context.
	/// Context should maintain a timestamp to allow the replay.
	/// </summary>
	public abstract class ReplayContext: MonoBehaviour
	{
		#region Public Methods
		/// <summary>
		/// Should return the current timestamp
		/// </summary>
		/// <returns>the timestamp</returns>
		public abstract double GetTimeStamp();

		/// <summary>
		/// Should Fast Forward the context to the value.
		/// Or do nothing if value < current timestamp.
		/// </summary>
		public abstract void SetTimeStamp(double value);

		/// <summary>
		/// Should reset the context and set timestamp to 0
		/// The context should be in "pause" after this call.
		/// </summary>
		public abstract void ResetContext();
		
		/// <summary>
		/// Start the context. Time should be started.
		/// </summary>
		public abstract void StartContext();

		/// <summary>
		/// Stop the context. Time should be set in pause.
		/// </summary>
		public abstract void StopContext();
		#endregion
	}
}
