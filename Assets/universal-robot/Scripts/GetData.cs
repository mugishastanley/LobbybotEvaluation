using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GetData : MonoBehaviour {

    string value;
    public string subjectName;
    string serializedValues = "";
    


    // Use this for initialization
    void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {
    
    }

    public void setString(string s){
        value = s;
    }

    public void publishValue(){
        serializedValues += (value + ";");
        Debug.Log(value); 
    }

    public void WriteData(){
        // Write to disk
        string fileName = subjectName + "_data.csv";
        StreamWriter writer = new StreamWriter(fileName, true);
        Debug.Log(serializedValues);
        writer.WriteLine(serializedValues);
        writer.Close();
    }

}
