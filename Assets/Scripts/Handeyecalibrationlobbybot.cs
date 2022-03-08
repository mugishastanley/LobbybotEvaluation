using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Handeyecalibrationlobbybot : MonoBehaviour
{

    private string Caldata = @"c:\temp\Caladata.txt";
    //private string Robotdata = @"c:\temp\Robotdata.txt";


    private Matrix4x4 originoffset;
    private Matrix4x4 trackeroffset;
    private Matrix4x4 T1;

    //calibration parameters
    private List<Matrix4x4> _robotdata;
    private List<Matrix4x4> _caldata;

    void readCalibrationdata()
    {

    }

    void readTextFileVector4(string file_path, List<Quaternion> structure)
    {
        //Mthd 2
        StreamReader inp_stm = new StreamReader(file_path);
        while (!inp_stm.EndOfStream)
        {

            string inp_ln = inp_stm.ReadLine();
            // Extract everything between :
            Regex regex = new Regex(@"\(.*?\)");
            MatchCollection matches = regex.Matches(inp_ln);
            //remove brackets
            var tr = matches[0].ToString();
            var result = tr.Trim('(', ')');
            var sStrings = result.Split(","[0]);
            float x = float.Parse(sStrings[0]);
            float y = float.Parse(sStrings[1]);
            float z = float.Parse(sStrings[2]);
            float w = float.Parse(sStrings[3]);
            Quaternion rot = new Quaternion(x, y, z, w);
            structure.Add(rot);
            // Debug.Log("Row :"+" x:"+x+" Y:"+y+" z:"+z);
            //Debug.Log("Pos is:"+rot);
        }
        inp_stm.Close();
        //("rotations loaded is:");
    }

    void readTextFileVector16(string file_path, List<Matrix4x4> structure)
    {
        //Mthd 2
        StreamReader inp_stm = new StreamReader(file_path);
        while (!inp_stm.EndOfStream)
        {

            string inp_ln = inp_stm.ReadLine();
            // Extract everything between :
            Regex regex = new Regex(@"\:.*?\:");
            MatchCollection matches = regex.Matches(inp_ln);
            //remove brackets
            var tr = matches[0].ToString();
            var result = tr.Trim('(', ')');
            var sStrings = result.Split(","[0]);
            float x = float.Parse(sStrings[0]);
            float y = float.Parse(sStrings[1]);
            float z = float.Parse(sStrings[2]);
            float w = float.Parse(sStrings[3]);


            T1 = new Matrix4x4();
            T1[0,0] = float.Parse(sStrings[0]);
            T1[0,1] = float.Parse(sStrings[1]);
            T1[0,2] = float.Parse(sStrings[2]);
            T1[0,3] = float.Parse(sStrings[3]);
            T1[1,0] = float.Parse(sStrings[4]);
            T1[1,1] = float.Parse(sStrings[5]);
            T1[1,2] = float.Parse(sStrings[6]);
            T1[1,3] = float.Parse(sStrings[7]);
            T1[2,0] = float.Parse(sStrings[8]);
            T1[2,1] = float.Parse(sStrings[9]);
            T1[2,2] = float.Parse(sStrings[10]);
            T1[2,3] = float.Parse(sStrings[11]);
            T1[3,0] = 0;
            T1[3,1] = 0;
            T1[3,2] = 0;
            T1[3,3] = 1;

            structure.Add(T1);
            Debug.Log("Matrix:"+" t1:"+T1[0,0]+" Y:"+y+" T16:"+T1[3,3]);
            //Debug.Log("Pos is:"+rot);
        }
        inp_stm.Close();
        //("rotations loaded is:");
    }

    void calibrate()
    {
        Lobbybot.HandEyeCalibration.Compute(_robotdata, _caldata, out originoffset, out trackeroffset);
    }

    private void calibrationdata()
    {
        originoffset = new Matrix4x4();
        trackeroffset = new Matrix4x4();
        readTextFileVector16(Caldata, _caldata);
    }


    // Start is called before the first frame update
    void Start()
    {
        calibrationdata();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
