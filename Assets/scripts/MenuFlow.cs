using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFlow : MonoBehaviour
{
    bool optionsactive = false;
    private BoxCollider2D[] options;
    BoxCollider2D hovered = null;

    [SerializeField]
    private Color hovercolor;

    [SerializeField]
    private GameObject title;
    [SerializeField]
    private GameObject optionsmenu;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private AudioClip hovernoise;
    [SerializeField]
    private AudioClip clicknoise;
    [SerializeField]
    private BoxCollider2D[] optionsoptions;
    [SerializeField]
    private Sprite[] tutimages;
    [SerializeField]
    private string[] tutinfos;
    [SerializeField]
    private SpriteRenderer displayimage;
    [SerializeField]
    private TextMesh displaymessage;
    [SerializeField]
    private TextMesh pagenum;
    // Use this for initialization
    void Start()
    {
        options = GameObject.FindObjectsOfType<BoxCollider2D>();
        optionsmenu.gameObject.SetActive(false);

        for(int i=0;i< tutinfos.Length; ++i)
        {
            tutinfos[i] = tutinfos[i].Replace("$n", "\n");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mp = Input.mousePosition;
        Vector3 mouseworld = cam.ScreenToWorldPoint(mp);
        BoxCollider2D newhovered = null;

        if (!optionsactive)
        {
            for (int i = 0; i < options.Length; ++i)
            {
                if (MouseInside(mouseworld, options[i]))
                {
                    newhovered = options[i];
                    break;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hovered != null)
                {
                    switch (hovered.name)
                    {
                        case "START":
                            SceneManager.LoadScene("level1");
                            break;
                        case "OPTIONS":
                            ChangePage(true);
                            break;
                        case "QUIT":
                            Application.Quit();
                            break;
                    }

                    AudioManager.PlaySoundEffect(clicknoise);
                }
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                ChangePage(false);
                AudioManager.PlaySoundEffect(clicknoise);
            }

            for (int i = 0; i < optionsoptions.Length; ++i)
            {
                if (MouseInside(mouseworld, optionsoptions[i]))
                {
                    newhovered = optionsoptions[i];
                    break;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hovered != null)
                {
                    switch (hovered.name)
                    {
                        case "BACK":
                            ChangePage(false);
                            break;
                        case "NEXT":
                            tutindex++;
                            if (tutindex > tutimages.Length - 1)
                                tutindex = 0;
                            ChangeTutImage();
                            break;
                        case "PREV":
                            tutindex--;
                            if (tutindex < 0)
                                tutindex = tutimages.Length - 1;
                            ChangeTutImage();
                            break;
                    }

                    AudioManager.PlaySoundEffect(clicknoise);
                }
            }
        }

        if (newhovered != null)
        {
            if (hovered != null && hovered != newhovered)
            {
                hovered.GetComponent<SpriteRenderer>().color = Color.white;
            }

            if (newhovered.GetComponent<SpriteRenderer>().color != hovercolor)
                AudioManager.PlaySoundEffect(hovernoise);

            newhovered.GetComponent<SpriteRenderer>().color = hovercolor;
        }
        else if (hovered != null)
        {
            hovered.GetComponent<SpriteRenderer>().color = Color.white;
        }

        hovered = newhovered;
    }

    void ChangePage(bool t)
    {
        optionsmenu.gameObject.SetActive(t);
        optionsactive = t;
        for (int i = 0; i < options.Length; ++i)
        {
            options[i].gameObject.SetActive(!t);
        }
        for (int i = 0; i < optionsoptions.Length; ++i)
        {
            optionsoptions[i].gameObject.SetActive(t);
        }
        title.gameObject.SetActive(!t);

        if(t)
        {
            displayimage.gameObject.SetActive(true);
            displaymessage.gameObject.SetActive(true);
            ChangeTutImage();
        }
        else
        {
            displayimage.gameObject.SetActive(false);
            displaymessage.gameObject.SetActive(false);
        }
    }

    int tutindex = 0;
    void ChangeTutImage()
    {
        displayimage.sprite = tutimages[tutindex];
        displaymessage.text = tutinfos[tutindex];

        pagenum.text = (tutindex + 1).ToString() + "/" + tutimages.Length;
    }

    bool MouseInside(Vector3 mouseworld, BoxCollider2D box)
    {
        Vector3 br = box.bounds.center + box.bounds.extents;
        Vector3 tl = box.bounds.center - box.bounds.extents;

        if (mouseworld.x <= tl.x || mouseworld.x >= br.x ||
            mouseworld.y <= tl.y || mouseworld.y >= br.y)
            return false;

        return true;
    }
}
