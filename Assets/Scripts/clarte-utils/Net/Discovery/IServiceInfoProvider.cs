using UnityEngine;

namespace CLARTE.Net.Discovery
{
	public abstract class IServiceInfoProvider : MonoBehaviour
	{
		public abstract IServiceInfo ServiceInfo { get; }
    }
}
