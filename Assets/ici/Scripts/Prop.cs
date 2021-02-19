using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour {

	public UR5 ur5;

	private Transform[] surface;
	private Vector3 PropTCPPosition = new Vector3 ();
	private Quaternion PropTCPRotation = new Quaternion ();
	private bool doneOnce = false;

	private int activeSurface = 0;

	/// <summary>
	/// Gets or sets the active surface.
	/// </summary>
	/// <value>The active surface. Value in [0;6] interval.</value>
	public int ActiveSurface
	{
		get { return activeSurface; }
		set { 
			if(value >= 0 && value < 6)
				activeSurface = value; 
		}
	}

	/// <summary>
	/// Gets the active surface position.
	/// </summary>
	/// <value>The surface position.</value>
	public Vector3 SurfacePosition
	{
		get {
			return (surface [activeSurface].position);
		}
	}

	/// <summary>
	/// Gets the active surface rotation.
	/// </summary>
	/// <value>The surface rotation.</value>
	public Quaternion SurfaceRotation
	{
		get {
			return (surface [activeSurface].rotation);
		}
	}

	void Start ()
	{
		surface = new Transform[6];
		for (int i = 1; i < 7; i++) {
			surface [i-1] = transform.Find ("surface" + i);
		}
	}

	void Update ()
	{
		if (ur5.IsInitialized && !doneOnce) {
			PropTCPRotation = Quaternion.Inverse(ur5.Tcp.rotation) * transform.rotation;
			PropTCPPosition = Quaternion.Inverse(ur5.Tcp.rotation) * (ur5.Tcp.position - transform.position);
			doneOnce = true;
		}

		if ((PropTCPRotation * Quaternion.Inverse(transform.rotation) * ur5.Tcp.rotation).w < 0.99f) {
			doneOnce = false;
		}
	}


	/// <summary>
	/// Updates the UR5 target position. Depend on active surface value.
	/// </summary>
	/// <param name="targetOnPlane">Target on plane.</param>
	public void UpdateTarget(Transform targetOnPlane)
	{
		ur5.Target.rotation = targetOnPlane.rotation * Quaternion.Inverse (PropTCPRotation * surface [activeSurface].localRotation);
		ur5.Target.position = targetOnPlane.position - ur5.Target.rotation * (PropTCPRotation * surface [activeSurface].localPosition - PropTCPPosition);
	}
}
