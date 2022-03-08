using System;
using UnityEngine;

namespace CLARTE.Pattern
{
	public abstract class Singleton : MonoBehaviour
	{
		[Serializable]
		public class MultipleInstancesException : Exception
		{
			public MultipleInstancesException(string message) : base(message)
			{

			}
		}
	}

	/// <summary>
	/// Create a singleton instance.
	/// </summary>
	/// <remarks>
	/// Be aware this will not prevent a non singleton constructor such as `T myT = new T();`
	/// To prevent that, add `protected T () {}` to your singleton class.
	/// 
	/// As a note, this is made as MonoBehaviour because we need Coroutines.
	/// </remarks>
	public abstract class Singleton<T> : Singleton where T : MonoBehaviour
	{
		#region Members
		private static T me;
		private static readonly object lockObject = new object();
		private static bool applicationIsQuitting;
		#endregion

		#region MonoBehaviour callbacks
		/// <summary>
		/// Destroy the current singleton instance.
		/// </summary>
		/// <remarks>
		/// When Unity quits, it destroys objects in a random order.
		/// In principle, a Singleton is only destroyed when application quits.
		/// If any script calls Instance after it have been destroyed, 
		/// it will create a buggy ghost object that will stay on the Editor scene
		/// even after stopping playing the Application. Really bad!
		/// So, this was made to be sure we're not creating that buggy ghost object.
		/// </remarks>
		protected virtual void OnDestroy()
		{
			applicationIsQuitting = true;

			me = null;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Get the current instance of the singleton.
		/// </summary>
		public static T Instance
		{
			get
			{
				if(applicationIsQuitting)
				{
					Debug.LogWarningFormat("[Singleton] Instance '{0}' already destroyed on application quit.", typeof(T));

					return null;
				}
				
				lock(lockObject)
				{
					if(me == null)
					{
						me = (T) FindObjectOfType(typeof(T));
						
						if(FindObjectsOfType(typeof(T)).Length > 1)
						{
							throw new MultipleInstancesException(string.Format("[Singleton] Found more than one instance of the singleton '{0}'.", typeof(T)));
						}
						
						if(me == null)
						{
							GameObject singleton = new GameObject();

							me = singleton.AddComponent<T>();

							singleton.name = string.Format("Singleton<{0}>", typeof(T));

							singleton.hideFlags = HideFlags.HideAndDontSave;

							DontDestroyOnLoad(singleton);
						}
					}
					
					return me;
				}
			}
		}

		/// <summary>
		/// Check if the singleton exist.
		/// </summary>
		public static bool Exist
		{
			get
			{
				return !applicationIsQuitting && me != null;
			}
		}

		/// <summary>
		/// Reset the singleton state to allow respawn.
		/// </summary>
		public static void Restart()
		{
			applicationIsQuitting = false;
		}
		#endregion
	}
}
