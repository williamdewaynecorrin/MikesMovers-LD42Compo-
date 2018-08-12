using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour {

    public Furniture nextitem;
    public SpriteRenderer[] selectorpreview;

    public void SetSelectorFor(GridSpace space)
    {
        for(int i = 0; i < space.shape.Length; ++i)
        {
            selectorpreview[i].enabled = space.shape[i];
        }
    }
}
