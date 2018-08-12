using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTruck : MonoBehaviour {

    public float speed = 0.005f;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position += Vector3.right * speed;

        if (this.transform.position.x >= 9.74f)
            this.transform.position = new Vector3(-11.05f, this.transform.position.y, this.transform.position.z);

    }
}
