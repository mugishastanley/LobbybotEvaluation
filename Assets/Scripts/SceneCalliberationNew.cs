using UnityEngine;
using UnityEngine.UI;



public class SceneCalliberationNew : MonoBehaviour
{
    // The virtual object which is the tracking reference
    public Transform reference;

    // The virtual object which is attached to the robot as a reference
    public Transform RobotTracker;

    // Representation of real robot in the car
    //public Transform Real_Robot;

    //our points of interest
    public Transform Points;

    //our points of interest
    public Transform Car;
    public Button InitialiseButton;

    //[Header("Tracker offset from Robot")]
    //[SerializeField]
    //private float x;
    //[SerializeField]
    //private float y;
    //Space between variables 
    //[Space(10)]
    //Attribute used to make a float or int variable in a script be restricted to a specific range.
    //public RangeAttribute(float min, float max);
    [Range(5, 9)]
    public string playerName = "Unnamed";

    Matrix4x4 pwrtvr;
    Matrix4x4 rwrtvr;
    Matrix4x4 Transmat;
    Matrix4x4 TrackedPointWRTVR;
    Matrix4x4 TrackedPointWRTRobot;
    Matrix4x4 RobotWRTVR;
    Matrix4x4 VRWRTRobot;
    Matrix4x4 Home;
    TransformMatrix mp;
    TransformMatrix mr;


    //This function returns the VR points to Robot Tranformation matrix
    void calibrate()
    {

       // float offset_x = x;
       // float offset_y = y;
       // float offset_z = Table_Height_meters;

        // We unparent the virtual object reference from the robot
        RobotTracker.transform.parent = null;
        // We unparent the points
        //Points.transform.parent = null;

        //position of the real robot to align with the tracker
        //Real_Robot.transform.position = RobotTracker.transform.position;

        // We parent the points of interest to the reference point
        //or parent to the reference point
        //Points.transform.parent = reference.transform;

        // We move the robotTracker  to where the referenceTracker is
        //RobotTracker.transform.parent = reference.transform;
        //RobotTracker.transform.parent = reference.transform;


        //Car.transform.parent = Real_Robot.transform;

        //position of the real robot to align with the tracker
        //Real_Robot.transform.position = RobotTracker.transform.position;

        //Alignment of the blue axis
        //Real_Robot.transform.forward = RobotTracker.transform.right;



        //Parent the points to the car
        //Points.transform.SetParent(Car, true);


        //we move the real robot to the robot tracker
        //Real_Robot.transform.parent = RobotTracker.transform.parent ;

        //all local positions to be converted to world positions

        pwrtvr = Points.transform.localToWorldMatrix.inverse;
        rwrtvr = RobotTracker.transform.localToWorldMatrix;

        //VR to robot Transformation
        Transmat = pwrtvr * rwrtvr;
        // Debug.Log("Button clicked Transmat = " + Transmat);

        Matrix4x4 HomePos()
        {
            Home[0, 0] = 0.98208f; Home[0, 1] = -0.18160f; Home[0, 2] = 0.05046f; Home[0, 3] = -0.03152f;
            Home[1, 0] = 0.12616f; Home[1, 1] = 0.43447f; Home[1, 2] = -0.89181f; Home[1, 3] = 0.69367f;
            Home[2, 0] = 0.14002f; Home[2, 1] = 0.88219f; Home[2, 2] = 0.44960f; Home[2, 3] = -0.51877f;
            Home[3, 0] = 0f; Home[3, 1] = 0f; Home[3, 2] = 0f; Home[3, 3] = 1.0f;

            return Home;



        }

        // Start is called before the first frame update
        void Start()
        {
            InitialiseButton.onClick.AddListener(delegate { calibrate(); });
            //InitialiseButton.onClick.AddListener(delegate { initialise.init();});
        }
    }


}
