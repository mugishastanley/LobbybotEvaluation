using System;

namespace CLARTE.Threads.DataFlow
{
	/// <summary>
	/// Exception raised when no DataProvider is given in configuration.
	/// </summary>
	public class NoDataProviderException: Exception
	{
		#region Constructors
		/// <summary>
		/// Default constructor of NoDataProviderException.
		/// </summary>
		public NoDataProviderException() : base() { }

		/// <summary>
		/// Constructor of NoDataProviderException that takes an error message.
		/// </summary>
		/// <param name="message">The error message associated with this exception.</param>
		public NoDataProviderException(string message) : base(message) { }

		/// <summary>
		/// Constuctor of NoDataProviderException that takes an error message and another exception as context.
		/// </summary>
		/// <param name="message">The error message associated with this exception.</param>
		/// <param name="innerException">An inner exception who raised this exception, as context for the current error.</param>
		public NoDataProviderException(string message, Exception innerException) : base(message, innerException) { }
		#endregion
	}
}
