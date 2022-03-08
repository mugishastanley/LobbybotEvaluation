using System.Diagnostics;

namespace CLARTE.Dev.Profiling
{
	/// <summary>
	/// Tick-accurate chronometer class
	/// </summary>
	public class Chrono
	{
		private Stopwatch stopWatch;

		public Chrono()
		{
			stopWatch = new Stopwatch();
		}

		/// <summary>
		/// Start chronometer
		/// </summary>
		public void Start()
		{
			stopWatch.Start();
		}

		/// <summary>
		/// Stop chronometer
		/// </summary>
		public void Stop()
		{
			stopWatch.Stop();
		}

		/// <summary>
		/// Reset chronometer
		/// </summary>
		public void Reset()
		{
			stopWatch.Reset();
		}

		/// <summary>
		/// Retart chronometer
		/// </summary>
		public void Restart()
		{
			stopWatch.Restart();
		}

		/// <summary>
		/// Get elapsed time in seconds
		/// </summary>
		/// <returns></returns>
		public double GetElapsedTime()
		{
			long elapsed_ticks = stopWatch.ElapsedTicks;
			double elapsed_s = (double)elapsed_ticks / (double)Stopwatch.Frequency;

			return elapsed_s;
		}
	}
}