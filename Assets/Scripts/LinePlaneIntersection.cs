using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class LinePlaneIntersection : MonoBehaviour
{
	public GameObject start;
	public GameObject End;
	public GameObject Plane;
	public GameObject result;
	// Start is called before the first frame update
	void Start()
    {
		//Vector3 Interpoint = new Vector3(0,0,0);

	}

    // Update is called once per frame
    void Update()
    {
		Vector3 Interpoint = Intersection(start.transform.localPosition, End.transform.localPosition, Plane.transform.localPosition);
		result.transform.localPosition = Interpoint;
		string output = Interpoint.ToString("F4");
		//Debug.Log("Intersection"+ output );



	}

	//Get the intersection between a line and a plane. 
	//If the line and plane are not parallel, the function outputs true, otherwise false.
	public static bool LinePlaneInter(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
	{

		float length;
		float dotNumerator;
		float dotDenominator;
		Vector3 vector;
		intersection = Vector3.zero;

		//calculate the distance between the linePoint and the line-plane intersection point
		dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
		dotDenominator = Vector3.Dot(lineVec, planeNormal);

		//line and plane are not parallel
		if (dotDenominator != 0.0f)
		{
			length = dotNumerator / dotDenominator;

			//create a vector from the linePoint to the intersection point
			vector = SetVectorLength(lineVec, length);

			//get the coordinates of the line-plane intersection point
			intersection = linePoint + vector;

			return true;
		}

		//output not valid
		else
		{
			return false;
		}
	}
	public static Vector3 SetVectorLength(Vector3 vector, float size)
	{

		//normalize the vector
		Vector3 vectorNormalized = Vector3.Normalize(vector);

		//scale the vector
		return vectorNormalized *= size;
	}


	public Vector3 Intersection(Vector3 start, Vector3 end, Vector3 planeOrgin) {
		Vector3 point;
		Vector3 diff = end - start;
		var t = (planeOrgin.x - start.x) / diff.x;	
		point.x = planeOrgin.x;
		point.y = start.y + t * diff.y;
		point.z = start.z + t * diff.z;
		return point;
	}


}
