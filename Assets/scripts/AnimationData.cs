using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimationData
{
    public string name = "default";
    public Sprite[] frames;
    public int ticktime = 0;
    public bool loop = true;
    public bool fliphorizontal = false;
    public bool flipvertical = false;
}
