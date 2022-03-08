namespace CLARTE.Threads.DataFlow
{
	#region Delegates
	/// <summary>
	/// Delegate for functions providing data to the flow.
	/// </summary>
	/// <typeparam name="OutputType">The type on data provided.</typeparam>
	/// <returns>Some data for the flow.</returns>
	public delegate OutputType CreateDataDelegate<OutputType>();

	/// <summary>
	/// Delegate for functions that transform data in the flow.
	/// </summary>
	/// <typeparam name="InputType">The type of data before transformation.</typeparam>
	/// <typeparam name="OutputType">The type of data after transformation.</typeparam>
	/// <param name="data">The data to transform.</param>
	/// <returns>The transformed data.</returns>
	public delegate OutputType WorkOnDataDelegate<InputType, OutputType>(InputType data);

	/// <summary>
	/// Delegate for functions that receive transformed data from the flow.
	/// </summary>
	/// <typeparam name="InputType">The type of transformed data.</typeparam>
	/// <param name="data">The transformed data.</param>
	public delegate void ConsumerDataDelegate<InputType>(InputType data);

	/// <summary>
	/// Delegate function to clone data when multiple consumers are linked to the same output of a provider. 
	/// </summary>
	/// <remarks>Data transmission between chained workers.</remarks>
	/// <typeparam name="OutputType">The type of the data.</typeparam>
	/// <param name="data">The data returned by the provider.</param>
	/// <param name="clone">True if the data should be cloned, false otherwise.</param>
	public delegate void ProvideDataDelegate<OutputType>(OutputType data, bool clone);
	#endregion

	#region Interfaces
	/// <summary>
	/// Interface for data providers.
	/// </summary>
	/// <typeparam name="OutputType">The type of provided data.</typeparam>
	public interface IDataProvider<OutputType>
	{
		/// <summary>
		/// The list of registered output receivers for this provider.
		/// </summary>
		event ProvideDataDelegate<OutputType> ProvideDataEvent;
	}

	/// <summary>
	/// Interface for data providers based on Unity MonoBehaviours.
	/// </summary>
	/// <typeparam name="OuptputType">The type of provided data.</typeparam>
	public interface IMonoBehaviourDataProvider<OuptputType>
	{
		/// <summary>
		/// Getter for associated IDataProvider.
		/// </summary>
		IDataProvider<OuptputType> Provider { get; }
	}
	#endregion
}
