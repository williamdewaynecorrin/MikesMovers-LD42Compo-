using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideIn : MonoBehaviour {

    [SerializeField]
    private Transform desired;
    [SerializeField]
    private float lerp = 0.1f;
    [SerializeField]
    private SlideIn waitforbeforeslide;
    [SerializeField]
    private float initialwaittime = 0f;
    [SerializeField]
    private AudioClip slidesound;

    bool slid = false;
    bool begin = false;
    bool prevbegin;
    // Use this for initialization
    void Start ()
    {
        StartCoroutine(WaitFor());
        prevbegin = begin;

    }

    IEnumerator WaitFor()
    {
        yield return new WaitForSeconds(initialwaittime);
        begin = true;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (!begin)
            return;

        if (waitforbeforeslide != null && !waitforbeforeslide.LockedIn())
            return;

        if(begin && !prevbegin)
        {
            AudioManager.PlaySoundEffect(slidesound);
        }
            
		if(!slid)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, desired.position, lerp);

            if(Vector3.Distance(this.transform.position, desired.position) <= Globals.kEpsilon)
            {
                slid = true;
            }
        }

        prevbegin = begin;
	}

    public bool LockedIn()
    {
        return slid;
    }
}
