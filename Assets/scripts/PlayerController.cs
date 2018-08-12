using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    private AudioClip[] walknoises;
    [SerializeField]
    private GameObject feetsmokeprefab;

    int walknoisetick = 15;
    int frameholdstack = 5;
    int frameholdstackR;
    int walknoisetickR;
    bool wasmoving = false;
    PlayerDirection currentdirection = PlayerDirection.Idle;

    AnimationController animcontroller;
    string prevanimation;
    GridLocation location;
    GridLocation nextlocation;
    PlayerMoveStyle movestyle = PlayerMoveStyle.KeyboardOnly;
    float dt = 0f;

    ItemQueue iq;
    // =============================================================================================
    // BUILT-IN UNITY FUNCTIONS
    // =============================================================================================

    // Use this for initialization

    private void Awake()
    {
        animcontroller = GetComponent<AnimationController>();
        walknoisetickR = walknoisetick;
        frameholdstackR = frameholdstack;
    }

    void Start ()
    {
        nextlocation = GridController.GetDoorLocation();
        SetPositionImmediate(nextlocation);
        prevanimation = animcontroller.currentanimation.name;
        iq = GameObject.FindObjectOfType<ItemQueue>();
        animcontroller.SetAnimation("IdleDown");
        currentdirection = PlayerDirection.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (!iq.RoundStarted() || iq.RoundOver())
        {
            animcontroller.SetAnimation("IdleDown");
            currentdirection = PlayerDirection.Idle;
            return;
        }

        switch(movestyle)
        {
            case PlayerMoveStyle.KeyboardOnly:
                ControlWithKeyboard();
                break;
        }
    }

    void ControlWithKeyboard()
    {
        if (nextlocation != location)
            return;

        if (LeftPressed())
        {
            if (currentdirection == PlayerDirection.Left || wasmoving)
            {
                MoveLeft();
                animcontroller.SetAnimation("WalkLeft");
                frameholdstack = frameholdstackR;
                wasmoving = true;
            }
            else
            {
                if (!prevanimation.Equals("IdleLeft"))
                    frameholdstack = frameholdstackR;

                animcontroller.SetAnimation("IdleLeft");

                if (frameholdstack == 0)
                {
                    currentdirection = PlayerDirection.Left;
                    frameholdstack = frameholdstackR;
                }
                else frameholdstack--;
            }
        }
        else if (RightPressed())
        {
            if (currentdirection == PlayerDirection.Right || wasmoving)
            {
                MoveRight();
                animcontroller.SetAnimation("WalkRight");
                frameholdstack = frameholdstackR;
                wasmoving = true;
            }
            else
            {
                if (!prevanimation.Equals("IdleRight"))
                    frameholdstack = frameholdstackR;

                animcontroller.SetAnimation("IdleRight");

                if (frameholdstack == 0)
                {
                    currentdirection = PlayerDirection.Right;
                    frameholdstack = frameholdstackR;
                }
                else frameholdstack--;
            }
        }
        else if (UpPressed())
        {
            if (currentdirection == PlayerDirection.Up || wasmoving)
            {
                MoveUp();
                animcontroller.SetAnimation("WalkUp");
                frameholdstack = frameholdstackR;
                wasmoving = true;
            }
            else
            {
                if (!prevanimation.Equals("IdleUp"))
                    frameholdstack = frameholdstackR;

                animcontroller.SetAnimation("IdleUp");

                if (frameholdstack == 0)
                {
                    currentdirection = PlayerDirection.Up;
                    frameholdstack = frameholdstackR;
                }
                else frameholdstack--;
            }
        }
        else if (DownPressed())
        {
            if (currentdirection == PlayerDirection.Down || wasmoving)
            {
                MoveDown();
                animcontroller.SetAnimation("WalkDown");
                frameholdstack = frameholdstackR;
                wasmoving = true;
            }
            else
            {
                if (!prevanimation.Equals("IdleDown"))
                    frameholdstack = frameholdstackR;

                animcontroller.SetAnimation("IdleDown");

                if (frameholdstack == 0)
                {
                    currentdirection = PlayerDirection.Down;
                    frameholdstack = frameholdstackR;
                }
                else frameholdstack--;
            }
        }
        else
        {
            string idlename = animcontroller.currentanimation.name.Replace("Walk", "Idle");
            animcontroller.SetAnimation(idlename);
            wasmoving = false;
            currentdirection = PlayerDirection.Idle;
        }
    }

    public GridLocation GetCurrentGridLocation()
    {
        return location;
    }

    public GridLocation GetNextGridLocation()
    {
        return nextlocation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(nextlocation != location)
        {
            dt += Time.fixedDeltaTime * 5f;
            transform.position = Vector3.Lerp(location.anchorpoint, nextlocation.anchorpoint, dt);

            if(Vector3.Distance(transform.position, nextlocation.anchorpoint) < Globals.kEpsilon)
            {
                SetPositionImmediate(nextlocation);
                // -- check for breakable items
                if(GridController.IsBreakable(nextlocation))
                {
                    if (nextlocation.RemoveBreakable())
                    {

                        GridController.BreakFX(nextlocation.anchorpoint);
                    }
                }
            }
        }

        // -- play some walking noises if we are walking
        if(animcontroller.currentanimation.name.Contains("Walk"))
        {
            walknoisetick--;

            if(walknoisetick % 10 == 0)
            {
                GameObject smoke = GameObject.Instantiate(feetsmokeprefab, 
                    this.transform.position + Vector3.down * 0.075f, 
                    Quaternion.identity);
                GameObject.Destroy(smoke, 3f);
            }

            if(walknoisetick == 0)
            {
                walknoisetick = walknoisetickR;
                int r = Random.Range(0, walknoises.Length);
                AudioManager.PlaySoundEffect(walknoises[r]);
            }
        }
        else if(prevanimation.Contains("Walk"))
        {
            walknoisetick = walknoisetickR;
        }

        prevanimation = animcontroller.currentanimation.name;
    }

    void MoveUp()
    {
        GridLocation above = GridController.Above(location);

        if(above != null && GridController.CanWalkOn(above))
        {
            nextlocation = above;
        }
    }

    void MoveDown()
    {
        GridLocation down = GridController.Below(location);

        if (down != null && GridController.CanWalkOn(down))
        {
            nextlocation = down;
        }
    }

    void MoveRight()
    {
        GridLocation right = GridController.Right(location);

        if (right != null && GridController.CanWalkOn(right))
        {
            nextlocation = right;
        }
    }

    void MoveLeft()
    {
        GridLocation left = GridController.Left(location);

        if (left != null && GridController.CanWalkOn(left))
        {
            nextlocation = left;
        }
    }

    void SetPositionImmediate(GridLocation gl)
    {
        location = gl;
        this.transform.position = gl.anchorpoint;
        dt = 0f;
    }

    // =============================================================================================
    // INPUT FUNCTIONS
    // =============================================================================================
    bool LeftPressed()
    {
        return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    }

    bool RightPressed()
    {
        return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
    }

    bool UpPressed()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
    }

    bool DownPressed()
    {
        return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
    }
}

public enum PlayerMoveStyle
{
    KeyboardOnly,
    PointAndClick
}

public enum PlayerDirection
{
    Idle,
    Right,
    Down,
    Left,
    Up
}