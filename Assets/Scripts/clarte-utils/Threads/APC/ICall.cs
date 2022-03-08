using System;

namespace CLARTE.Threads.APC
{
	/// <summary>
	/// Interface for callable objects
	/// </summary>
	public interface ICall
	{
		Result<T> Call<T>(Func<T> callback);

		Result Call(Action callback);
	}
}
