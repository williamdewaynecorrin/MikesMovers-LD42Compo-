using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour {

    public GridSpace cost;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SwapWidthHeightOffset()
    {
        int hold = cost.heightoffset;
        cost.heightoffset = cost.widthoffset;
        cost.widthoffset = hold;
    }
}
