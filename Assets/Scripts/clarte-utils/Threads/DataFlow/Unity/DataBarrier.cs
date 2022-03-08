using UnityEngine;
using System.Threading;

public class DataBarrier: MonoBehaviour
{
	#region Members
	/// <summary>
	/// The barrier object associated to this object.
	/// </summary>
	public Barrier barrier = new Barrier(0);
	#endregion
}
