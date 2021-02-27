using UnityEngine;
using System.Collections;

public class UR5 : MonoBehaviour
{
	public string ip = "192.168.0.11";

	public Vector3 tcpPositionOffset;

	public Vector3 tcpRotationOffset;

	public Vector3 toolCenterOfMassPosition;

	public float toolMass;

    public bool quack = false;

	public UR_Manager ur;

	public float k = 0.01f, d = 0.5f;

	public float max_lin_vel = 0.01f;

	public float rot_p_gain = 1.0f;

	public float rot_d_gain = 1.0f;

	public float max_rot_vel = 50.0f;


	Transform[] joints;

	Quaternion[] joints_init;

	Transform tcp;

	Transform target;

	bool isInitialized = false;

	[SerializeField]
	bool followTarget = false;

	public Transform Tcp
	{
		get { return tcp; }
		set { tcp = value; }
	}

	public Transform Target
	{
		get { return target; }
		set { target = value; }
	}

	public bool FollowTarget
	{
		get { return followTarget; }
		set { followTarget = value; }
	}

	public bool IsInitialized
	{
		get { return isInitialized; }
	}

	// Use this for initialization
	void Start()
	{
		joints = new Transform[6];

		joints_init = new Quaternion[6];

		joints[0] = transform.Find("Fix/Base");

		joints[1] = transform.Find("Fix/Base/Shoulder");

		joints[2] = transform.Find("Fix/Base/Shoulder/Elbow");

		joints[3] = transform.Find("Fix/Base/Shoulder/Elbow/Wrist1");

		joints[4] = transform.Find("Fix/Base/Shoulder/Elbow/Wrist1/Wrist2");

		joints[5] = transform.Find("Fix/Base/Shoulder/Elbow/Wrist1/Wrist2/Wrist3");

		tcp = transform.Find("tcp");
		//tcp = transform.Find("Prop");

		target = transform.Find("target");

		for (int i = 0; i < 6; i++)
		{
			joints_init[i] = joints[i].transform.localRotation;
		}

		ur = new UR_Manager();


		ur.Connect(ip);

		StartCoroutine(Initialize());
	}

	IEnumerator Initialize()
	{
		while (!ur.isConnected())
		{
			yield return new WaitForEndOfFrame();
		}

		ur.SetTCPPosition(tcpPositionOffset, tcpRotationOffset);

		yield return new WaitForSeconds(1);

		ur.SetPayload(toolMass, toolCenterOfMassPosition);

		yield return null;

		UpdateTcp();

		target.localPosition = tcp.localPosition;

		//target.localRotation = tcp.localRotation;
		target.localRotation = Quaternion.identity;
		isInitialized = true;

		Debug.Log("UR5 initialized");
	}

	// Update is called once per frame
	void Update()
	{
		if(isInitialized)
		{
			UpdateJoints();

			UpdateTcp();

            if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.H))
            {
                goHome();
            }

            if (FollowTarget)
			{
				ContinuouslyMoveToTarget();
			}
		}
	}

	void UpdateJoints()
	{
		if (ur.isConnected())
		{
			for (int i = 0; i < 6; i++)
			{
				joints[i].localRotation = joints_init[i] * Quaternion.AngleAxis(((float)ur.GetJointPositions()[i] * 180f) / Mathf.PI, Vector3.left);
			}
		}
	}

	void UpdateTcp()
	{
		if (ur.isConnected())
		{
			tcp.localPosition = ur.PoseTToVector3(new Vector3((float)ur.GetTCPPosition()[0], (float)ur.GetTCPPosition()[1], (float)ur.GetTCPPosition()[2]));

			tcp.localRotation = ur.PoseRToQuaternion(new Vector3((float)ur.GetTCPPosition()[3], (float)ur.GetTCPPosition()[4], (float)ur.GetTCPPosition()[5]));
			//Debug.Log("TCP position " + tcp.localPosition+"TCP Rotation " +tcp.localRotation.eulerAngles);
		}
	}

	void ContinuouslyMoveToTarget()
	{
		// Position only for the moment
		ur.SpeedL(ur.Vector3ToPoseT(ComputeSpatialSpeedToReachTarget()), ur.QuaternionToPoseR(ComputeRotationSpeedToReachTarget()));
	}

	Vector3 ComputeSpatialSpeedToReachTarget()
	{
		Vector3 position_error = target.localPosition - tcp.localPosition;

		//Vector3 candidate_velocity = k * position_error + d * (position_error - last_position_error);
		Vector3 candidate_velocity = k * position_error + d * new Vector3 ((float)ur.GetTCPSpeed()[0], (float)ur.GetTCPSpeed()[1], (float)ur.GetTCPSpeed()[2]);

		float candidate_vel = candidate_velocity.magnitude;

		Vector3 max_velocity = position_error.normalized * max_lin_vel;

		if (candidate_vel > max_lin_vel)
		{
			candidate_velocity = max_velocity;

			Debug.Log("Max speed (" + max_lin_vel + "m/s)exceeded");
		}

		return candidate_velocity;
	}

	Vector3 QuaternionAxis(Quaternion q)
	{
		float sin = 0;
		if (1 - q.w * q.w > 0.0001)
			sin = 1 / Mathf.Sqrt (1 - q.w * q.w);
		return new Vector3 (q.x * sin, q.y * sin, q.z * sin);
	}

	float QuaternionAngle(Quaternion q)
	{
		return 2 * Mathf.Acos (q.w) * 180 / Mathf.PI;
	}
		
    Quaternion ComputeRotationSpeedToReachTarget()
    {
		Quaternion rotation_error = target.localRotation * Quaternion.Inverse(tcp.localRotation);

		Vector3 axis = QuaternionAxis (rotation_error);
		float angle = rot_p_gain * QuaternionAngle (rotation_error) + rot_d_gain * QuaternionAngle (Quaternion.Euler (new Vector3 ((float)ur.GetTCPSpeed () [3], (float)ur.GetTCPSpeed () [4], (float)ur.GetTCPSpeed () [5]) * 180/3.141592654f));
		//Debug.Log ("rot : " + new Vector3 ((float)ur.GetTCPSpeed () [3], (float)ur.GetTCPSpeed () [4], (float)ur.GetTCPSpeed () [5]));
		/*
		if (angle >= 180) {
			angle = 360 - angle;
			axis = -axis;
		}
		*/
		if (angle > max_rot_vel) {
			angle = max_rot_vel;
		}

		if (angle < -max_rot_vel) {
			angle = -max_rot_vel;
		}

		Quaternion candidate_rotation = Quaternion.AngleAxis (angle, axis);

		return candidate_rotation;
    }


    public double[] GetTCPPosition()
    {
        double[] q = ur.GetTCPPosition();
        return new double[] {q[0],q[1]};
    }

    public double[] GetTCPSpeed()
    {
        double[] q = ur.GetTCPSpeed();
        return new double[] { q[0], q[1], q[2], q[3], q[4], q[5]};
    }

    public double[] GetTCPForce()
    {
        double[] q = ur.GetTCPForce();
        return new double[] { q[0], q[1], q[2], q[3], q[4], q[5] };
    }



    public void goPositionCube()
    {
        double[] q = { Mathf.Deg2Rad * -27.02, Mathf.Deg2Rad * -21.05, Mathf.Deg2Rad * 50.06, Mathf.Deg2Rad * -28.23, Mathf.Deg2Rad * 62.88, Mathf.Deg2Rad * -90 };
        MoveToConfiguration(q);
    }


    public void goPositionGroundTruth()
    {
        double [] q = { Mathf.Deg2Rad * -19.49, Mathf.Deg2Rad * -30.57, Mathf.Deg2Rad * 58.48, Mathf.Deg2Rad * -27.13, Mathf.Deg2Rad * 70.43, Mathf.Deg2Rad * 0.0 };
        MoveToConfiguration(q);
    }

    public void goPosition()
    {
        //double[] q = { Mathf.Deg2Rad * -31.53, Mathf.Deg2Rad * -32.5, Mathf.Deg2Rad * 59.49, Mathf.Deg2Rad * -57.99, Mathf.Deg2Rad * 63.71, Mathf.Deg2Rad * -45.93 };
        //double[] q = { Mathf.Deg2Rad * -31.53, Mathf.Deg2Rad * -35.15, Mathf.Deg2Rad * 58.4, Mathf.Deg2Rad * -54.28, Mathf.Deg2Rad * 63.71, Mathf.Deg2Rad * -45.93 };
        double[] q = { Mathf.Deg2Rad * -28.96, Mathf.Deg2Rad * -24.69, Mathf.Deg2Rad * 52.46, Mathf.Deg2Rad * -119.44, Mathf.Deg2Rad * 90.85, Mathf.Deg2Rad * 29.43 };
        MoveToConfiguration(q);
    }

    public void goHome()
    {
        double[] q = { 0, Mathf.Deg2Rad * -90, 0, Mathf.Deg2Rad * -90, 0, 0};
        MoveToConfiguration(q);
        
    }

    public void MoveToTarget()
	{
		if(ur.isConnected())
		{
			Vector3 position = ur.Vector3ToPoseT(target.localPosition);
			Vector3 orientation = ur.QuaternionToPoseR(target.localRotation);
			ur.MoveJ(position, orientation, 0.5, 0.3);
		}
	}

	public void MoveToConfiguration(double[] q)
	{
		ur.MoveJ (q);
	}

	void OnDestroy()
	{
		ur.Disconnect();

		isInitialized = false;
	}
}
