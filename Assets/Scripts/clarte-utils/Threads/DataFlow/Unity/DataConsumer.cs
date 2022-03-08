using System;
using UnityEngine;

namespace CLARTE.Threads.DataFlow.Unity
{
	public abstract class DataConsumer<InputType>: MonoBehaviour where InputType : ICloneable
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
		/// Encapsulated DataConsumer.
		/// </summary>
		protected DataFlow.DataConsumer<InputType> consumer;
		#endregion

		#region Abstract methods
		/// <summary>
		/// ConsumerDataDelegate implementation to receive data.
		/// </summary>
		/// <param name="data">The received data.</param>
		protected abstract void ConsumeData(InputType data);
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
			// Create consumer
			consumer = new DataFlow.DataConsumer<InputType>
			{
				millisecondsTimeout = millisecondsTimeout,
				ConsumeData = ConsumeData
			};

			if (DataBarrier != null && enabled)
			{
				consumer.RegisterBarrier(DataBarrier.barrier);
			}
		}

		protected virtual void Start()
		{
			if (DataProvider != null)
			{
				IMonoBehaviourDataProvider<InputType> monoProvider = DataProvider.GetComponent<IMonoBehaviourDataProvider<InputType>>();

				monoProvider.Provider.ProvideDataEvent += consumer.EnqeueTask;
			}
			else
			{
				throw new NoDataProviderException("You have to set a DataProvider in Unity or in Awake.");
			}
		}

		protected virtual void Update()
		{
			if (consumer.HasException)
			{
				consumer.Throw();
			}
		}
		#endregion
	}
}
