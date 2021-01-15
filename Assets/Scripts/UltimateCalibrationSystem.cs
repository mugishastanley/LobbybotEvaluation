using UnityEngine;

public class UltimateCalibrationSystem : MonoBehaviour
{
    // The virtual object which is tracked with a ViveTracker and holds  the ground truth
     public GameObject referenceTracker;

    // The virtual object which is attached to the robot as a reference
    public GameObject robotTracker;

    // The original parent of robotTracker which is an element of the     virtual robot
     public GameObject robotWrist;

    // The GameObject containing all the elements in the environment
    public GameObject masterArea;

    public void Calibrate()
    {
        // We unparent the virtual object reference from the robot
        // robotTracker.transform.parent = null;

        // We parent the area to robotTracker
        //Not smart
        masterArea.transform.parent = robotTracker.transform;

        // We move the robotTracker  to where the referenceTracker is
        //what about the orientation !!!
        robotTracker.transform.position = referenceTracker.transform.position;

        // We return things back at their original configuration
        masterArea.transform.parent = null;
        robotTracker.transform.parent = robotWrist.transform;
    }

    void Start()
    {
        Calibrate();

    }
}