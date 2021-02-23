using UnityEngine;
using System.Collections;

public class UR5_Manager : MonoBehaviour {

	public string ip = "192.168.0.11";
	public Vector3 tcp_vector;
	public Vector3 tcp_rotation;
	public Vector3 payload_vector;
	public float payload_mass;
	public UR_Manager ur;

	Transform[] joints;
	Quaternion[] joints_init;
	Transform tcp;
	Transform follower;
	 
	// Use this for initialization
	void Start () {
		ur = new UR_Manager();
		ur.Connect( ip );
		StartCoroutine( "SendParameters" );

		joints = new Transform[6];
		joints_init = new Quaternion[6];
		joints[0] = transform.Find( "Fix/Base" );
		joints[1] = transform.Find( "Fix/Base/Shoulder" );
		joints[2] = transform.Find( "Fix/Base/Shoulder/Elbow" );
		joints[3] = transform.Find( "Fix/Base/Shoulder/Elbow/Wrist1" );
		joints[4] = transform.Find( "Fix/Base/Shoulder/Elbow/Wrist1/Wrist2" );
		joints[5] = transform.Find( "Fix/Base/Shoulder/Elbow/Wrist1/Wrist2/Wrist3" );
		//tcp = transform.Find( "tcp" );
		tcp = transform.Find("Prop");
		follower = transform.Find( "follower" );
		for (int i = 0; i < 6; i++)
			joints_init[i] = joints[i].transform.localRotation;
	}

	IEnumerator SendParameters( )
	{
		while (!ur.isConnected())
			yield return new WaitForEndOfFrame();

		ur.SetTCPPosition(tcp_vector, tcp_rotation );
		yield return new WaitForSeconds( 1 );
		ur.SetPayload( payload_mass, payload_vector );
		yield return null;
	}

	// Update is called once per frame
	void Update () {
		UpdateJoints();
		UpdateTcp();
	}

	void UpdateJoints( )
	{
		if(ur.isConnected())
		{
			for (int i = 0; i < 6; i++)
				joints[i].localRotation = joints_init[i] * Quaternion.AngleAxis( ((float)ur.GetJointPositions()[i] * 180f) / Mathf.PI, Vector3.left );
		}
	}

	void UpdateTcp( )
	{
		if (ur.isConnected())
		{
			tcp.localPosition = ur.PoseTToVector3( new Vector3( (float)ur.GetTCPPosition()[0], (float)ur.GetTCPPosition()[1], (float)ur.GetTCPPosition()[2] ) );
			tcp.localRotation = ur.PoseRToQuaternion( new Vector3( (float)ur.GetTCPPosition()[3], (float)ur.GetTCPPosition()[4], (float)ur.GetTCPPosition()[5] ) );
		}
	}

	void OnGUI()
	{
		if (ur.isConnected() && GUI.Button( new Rect( 0, 0, 120, 50 ), "Move to follower" ))
		{
			Vector3 poseT = ur.Vector3ToPoseT( follower.localPosition );
			Vector3 poseR = ur.QuaternionToPoseR( follower.localRotation );
			ur.MoveL( poseT, poseR );
		}
		if (ur.isConnected() && GUI.Button( new Rect( 0, 100, 120, 50 ), "Speed" ))
		{
			ur.SpeedL( Vector3.right, Vector3.zero );
		}
	}

	void OnDestroy()
	{
		ur.Disconnect();
	}
}