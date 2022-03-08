using System;
using UnityEngine;

namespace CLARTE.Threads.DataFlow.Unity
{
	public abstract class DataWorker<InputType, OutputType>: MonoBehaviour, IMonoBehaviourDataProvider<OutputType> where InputType : ICloneable
	{
		#region Members
		/// <summary>
		/// IMonoBehaviourDataProvider that provides input data for this object.
		/// </summary>
		[SerializeField] protected GameObject DataProvider;

		/// <summary>
		/// Optional barrier to wait at.
		/// </summary>
		[SerializeField] protected DataBarrier DataBarrier;

		/// <summary>
		/// Timeout used to raise exception if no data have been received in a given amount of time.
		/// </summary>
		[SerializeField] protected int millisecondsTimeout = 1000;

		/// <summary>
		/// Encapsulated DataWorker.
		/// </summary>
		protected DataFlow.DataWorker<InputType, OutputType> worker;
		#endregion

		#region Abstract methods
		/// <summary>
		/// WorkOnDataDelegate implementation to transform data.
		/// </summary>
		/// <param name="data">The data to transform.</param>
		/// <returns>The transformed data.</returns>
		protected abstract OutputType Work(InputType data);
		#endregion

		#region Getters / Setters
		/// <summary>
		/// Get the IDataProvider associated with this object.
		/// </summary>
		public IDataProvider<OutputType> Provider
		{
			get
			{
				return worker;
			}
		}
		#endregion

		#region MonoBehaviour callbacks
		protected virtual void OnValidate()
		{
			if (DataProvider != null)
			{
				IMonoBehaviourDataProvider<InputType> monoprovider = DataProvider.GetComponent<IMonoBehaviourDataProvider<InputType>>();

				if (monoprovider == null)
				{
					DataProvider = null;
				}
			}
		}

		protected void Awake()
		{
			// Create worker
			worker = new DataFlow.DataWorker<InputType, OutputType>
			{
				millisecondsTimeout = millisecondsTimeout,
				WorkOnData = Work
			};

			if (DataBarrier != null && enabled)
			{
				worker.RegisterBarrier(DataBarrier.barrier);
			}
		}

		protected virtual void Start()
		{
			if (DataProvider != null)
			{
				IMonoBehaviourDataProvider<InputType> monoProvider = DataProvider.GetComponent<IMonoBehaviourDataProvider<InputType>>();

				monoProvider.Provider.ProvideDataEvent += worker.EnqeueTask;
			}
			else
			{
				throw new NoDataProviderException("You have to set a DataProvider in Unity or in Awake.");
			}
		}

		protected virtual void Update()
		{
			if (worker.HasException)
			{
				worker.Throw();
			}
		}
		#endregion
	}
}
