using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScore : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        GetComponent<TextMesh>().text = "HIGH SCORE: " + PlayerPrefs.GetInt("highscore");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
