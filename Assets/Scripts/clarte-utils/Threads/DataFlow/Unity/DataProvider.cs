using System;
using UnityEngine;

namespace CLARTE.Threads.DataFlow.Unity
{
	public abstract class DataProvider<OuptputType>: MonoBehaviour, IMonoBehaviourDataProvider<OuptputType>
	{
		#region Members
		/// <summary>
		/// Encapsulated DataProvider.
		/// </summary>
		protected DataFlow.DataProvider<OuptputType> DataCreator;
		#endregion

		#region Abstract methods
		/// <summary>
		/// CreateDataDelegate implementation to provide data.
		/// </summary>
		/// <returns>Some data for the flow.</returns>
		protected abstract OuptputType CreateData();
		#endregion

		#region Getters / Setters
		/// <summary>
		/// Get the IDataProvider associated with this object.
		/// </summary>
		public IDataProvider<OuptputType> Provider
		{
			get
			{
				return DataCreator;
			}
		}
		#endregion

		#region MonoBehaviour callbacks
		protected virtual void Awake()
		{
			DataCreator = new DataFlow.DataProvider<OuptputType>
			{
				CreateData = CreateData
			};
		}

		protected virtual void OnEnable()
		{
			DataCreator.Start();
		}

		protected virtual void Update()
		{
			if (DataCreator.HasException)
			{
				DataCreator.Stop();
			}
		}

		protected virtual void OnDisable()
		{
			DataCreator.Stop();
		}
		#endregion
	}
}
