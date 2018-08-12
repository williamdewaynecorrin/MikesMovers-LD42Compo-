using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

    public string startinganimation;
    public AnimationData[] animations;
    [HideInInspector]
    public AnimationData currentanimation = null;
    new SpriteRenderer renderer;

    int ticker;
    int tickerR;
    int currentframe;
    bool stopped = false;

    // =============================================================================================
    // UNITY BUILT IN FUNCTIONS
    // =============================================================================================
    
    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        tickerR = ticker;
        currentframe = 0;
        SetAnimation(startinganimation);
    }
    
    void Start () {
		
	}
	
	void Update () {
		
	}

    private void FixedUpdate()
    {
        if (currentanimation == null || stopped)
            return;

        ticker--;

        if (ticker == 0)
        {
            currentframe++;
            if (currentframe > currentanimation.frames.Length - 1)
            {
                if (currentanimation.loop)
                {
                    currentframe = 0;
                }
                else
                {
                    stopped = true;
                    return;
                }
            }

            ticker = tickerR;
            renderer.sprite = currentanimation.frames[currentframe];
        }
    }

    // =============================================================================================
    // ANIMATION FUNCTIONS
    // =============================================================================================

    public void SetAnimation(string name)
    {
        if (currentanimation != null && currentanimation.name.Equals(name))
            return;

        for (int i = 0; i < animations.Length; ++i)
        {
            if (animations[i].name.Equals(name))
            {
                currentanimation = animations[i];
                ticker = currentanimation.ticktime;
                tickerR = ticker;
                currentframe = 0;
                renderer.sprite = currentanimation.frames[currentframe];
                renderer.flipX = currentanimation.fliphorizontal;
                renderer.flipY = currentanimation.flipvertical;
                stopped = false;
                return;
            }
        }

        Debug.LogError(string.Format("AnimationData with name: {0} does not exist in this AnimationController.", name));
    }
}
