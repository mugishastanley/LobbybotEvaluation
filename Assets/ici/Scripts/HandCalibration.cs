using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCalibration : MonoBehaviour {

	public GameObject viveTracker;
	public GameObject prop;
	public UR5 ur5;

	private Vector3 savedPosition;
	private Quaternion savedRotation;

	// Use this for initialization
	void Start () {
		savedPosition = new Vector3 ();
		savedRotation = new Quaternion ();
		savedPosition = transform.localPosition;
		savedRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.W)) {
			transform.parent = viveTracker.transform;
		}
		if (Input.GetKeyDown (KeyCode.X)) {
			transform.parent = prop.transform;
			transform.localPosition = savedPosition;
			transform.localRotation = savedRotation;
			ur5.FollowTarget = false;
			ur5.Target.position = new Vector3 (-0.1298f, -0.2261f, 0.6191f);
			ur5.Target.rotation = Quaternion.Euler (12.225f, 83.794f, 100.178f);
            //ur5.MoveToTarget ();
            //double[] q = {1.9989 + 0.5 * (double)Mathf.PI , -2.779, -1.515, -2.1685, -1.3035, -4.4463};
            //double[] q = { 1.9989 + 0.5 * (double)Mathf.PI, -2.27, -1.515, -2.1685, -1.3035, -4.4463 };
            double[] q = { 2.4 + 0.5 * (double)Mathf.PI, -2.77, -1.515, -2.1685, -1.3035, -4.4463 };
            ur5.MoveToConfiguration(q);
		}
	}
}
