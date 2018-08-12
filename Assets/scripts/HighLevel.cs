using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighLevel : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        GetComponent<TextMesh>().text = "HIGH LEVEL: " + PlayerPrefs.GetInt("highlevel");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
