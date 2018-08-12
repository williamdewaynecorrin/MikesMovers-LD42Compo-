using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopText : MonoBehaviour {

    [HideInInspector]
    SpriteRenderer child;
    [HideInInspector]
    public TextMesh textmesh;
    [HideInInspector]
    public int lifetime = 60 * 3;

    float startingrotz;
	// Use this for initialization
	void Awake ()
    {
        textmesh = GetComponent<TextMesh>();
        startingrotz = transform.eulerAngles.z;

        child = transform.GetChild(0).GetComponent<SpriteRenderer>();
        GetComponent<MeshRenderer>().sortingLayerID = child.sortingLayerID;
        GetComponent<MeshRenderer>().sortingOrder = child.sortingOrder;
    }
	
    IEnumerator FadeOut()
    {
        for (int i = 0; i < 60; ++i)
        {
            textmesh.color = new Color(textmesh.color.r, textmesh.color.g, textmesh.color.b, textmesh.color.a - 0.0167f);
            yield return new WaitForSeconds(0.016666666f);
        }

        yield return null;
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        this.transform.position += Vector3.up * 0.00075f;
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y,
             startingrotz + Mathf.Sin(Time.fixedTime * 5.0f) * 1f);

        lifetime--;

        if(lifetime == 60)
        {
            StartCoroutine(FadeOut());
        }

        if(lifetime == 0)
        {
            GameObject.Destroy(this.gameObject);
        }
	}
}
