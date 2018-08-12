using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {

    public static GridController instance = null;
    public Selector selector;
    public Transform topleftanchor;
    public Camera cam;
    public int[] levelgridwidths;
    public int[] levelgridheights;
    public float gridentrysize = 0.32f;
    // -- x , y
    public GridLocation[,] grid;
    private GridLocation door;

    public GameObject[] floortileprefabs;
    public GameObject[] wallpaperprefabs;
    public GameObject doorexitprefab;
    [HideInInspector]
    public PlayerController player;

    private ItemQueue currentlevelqueue;
    private int rotationpreview = 0;
    bool canplaceobject = false;

    Vector3 breakposition;
    public GameObject breakparticleprefab;
    public AudioClip placeregular, placewalkable, placebreakable;
    public AudioClip breakablebreak, cannotplace;

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            GameObject.Destroy(this);
        }

        selector.gameObject.SetActive(false);
        InitGrid();
    }

    private void Start()
    {
        currentlevelqueue = GameObject.FindObjectOfType<ItemQueue>();
        player = GameObject.FindObjectOfType<PlayerController>();
    }

    void InitGrid()
    {
        int gridwidth = levelgridwidths[Globals.gCurrentLevel - 1];
        int gridheight = levelgridheights[Globals.gCurrentLevel - 1];
        // -- allocate grid
        grid = new GridLocation[gridwidth, gridheight];
        int floortype = Random.Range(0, floortileprefabs.Length);

        // -- initialize grid
        for (int i = 0; i < gridwidth; ++i)
        {
            for (int j = 0; j < gridheight; ++j)
            {
                // -- create tile
                Vector3 offset = new Vector3(gridentrysize * i, gridentrysize * -j);
                GameObject floortile = GameObject.Instantiate(floortileprefabs[floortype], topleftanchor.position + offset, Quaternion.identity);
                floortile.transform.SetParent(topleftanchor);
                // -- assign gridloc
                grid[i, j] = new GridLocation(i, j, topleftanchor.position + offset);
            }
        }


        // -- create wallpaper
        int walltype = Random.Range(0, wallpaperprefabs.Length);
        for (int i = 0; i < gridwidth; ++i)
        {
            Vector3 offset = new Vector3(gridentrysize * i, gridentrysize * 1.5f);
            GameObject wallpapertile = GameObject.Instantiate(wallpaperprefabs[walltype], topleftanchor.position + offset, Quaternion.identity);
            wallpapertile.transform.SetParent(topleftanchor);
        }

        // -- init door
        Vector3 dooroffset = new Vector3(gridentrysize * (gridwidth / 2f), -gridentrysize * gridheight);
        GameObject doorinstance = GameObject.Instantiate(doorexitprefab, topleftanchor.position + dooroffset, Quaternion.identity);
        doorinstance.transform.SetParent(topleftanchor);

        door = grid[gridwidth / 2, gridheight - 1];

    }

    public static Vector3 GetWorldPosition(int x, int y)
    {
        return instance.grid[x, y].anchorpoint;
    }

    public static bool CanPlaceAt(GridLocation loc)
    {
        if (loc == null)
            return false;

        // -- name data and grab bc things are getting complicated
        bool objectwalkable = instance.currentlevelqueue.GetNextItemCost().walkable;
        bool gridwalkable = instance.grid[loc.x, loc.y].walkable;
        bool breakable = instance.currentlevelqueue.GetNextItemCost().breakable;
        bool squarealreadyhasbreakable = instance.grid[loc.x, loc.y].hasbreakableitem;
        bool occupied = instance.grid[loc.x, loc.y].occupied;

        bool squareclear = !occupied || (gridwalkable && !objectwalkable) || breakable;

        // -- make sure the player isnt in or traveling to this location
        bool playerclear = !loc.Equals(instance.player.GetCurrentGridLocation()) &&
            !loc.Equals(instance.player.GetNextGridLocation());

        return squareclear && playerclear;
    }

    public static bool CanWalkOn(GridLocation loc)
    {
        if (loc == null)
            return false;

        return !instance.grid[loc.x, loc.y].occupied || (instance.grid[loc.x, loc.y].walkable);
    }

    public static GridLocation Above(GridLocation loc)
    {
        if (loc == null)
            return null;
        if (loc.y - 1 < 0)
            return null;

        return instance.grid[loc.x, loc.y - 1];
    }

    public static GridLocation Below(GridLocation loc)
    {
        int gridheight = instance.levelgridheights[Globals.gCurrentLevel - 1];

        if (loc == null)
            return null;
        if (loc.y + 1 > gridheight - 1)
            return null;

        return instance.grid[loc.x, loc.y + 1];
    }

    public static GridLocation Left(GridLocation loc)
    {
        if (loc == null)
            return null;
        if (loc.x - 1 < 0)
            return null;

        return instance.grid[loc.x - 1, loc.y];
    }

    public static GridLocation Right(GridLocation loc)
    {
        int gridwidth = instance.levelgridwidths[Globals.gCurrentLevel - 1];

        if (loc == null)
            return null;
        if (loc.x + 1 > gridwidth - 1)
            return null;

        return instance.grid[loc.x + 1, loc.y];
    }

    public static GridLocation GetDoorLocation()
    {
        return instance.door;
    }

    public static bool IsBreakable(GridLocation loc)
    {
        return instance.grid[loc.x, loc.y].hasbreakableitem;
    }

    public bool PlayerNextTo(GridLocation loc)
    {
        GridLocation pl = player.GetCurrentGridLocation();
        GridLocation pln = player.GetNextGridLocation();

        return pl == Above(loc) || pl == Right(loc) ||
            pl == Below(loc) || pl == Left(loc) ||

            pln == Above(loc) || pln == Right(loc) ||
            pln == Below(loc) || pln == Left(loc);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlevelqueue.RoundOver() || !currentlevelqueue.RoundStarted())
        {
            selector.gameObject.SetActive(false);
            return;
        }

        GridLocation center = null;
        UpdateSelectorStatus(out center);
        CheckSelectorTouchesPlayer(center);

        // -- rotate selector 90deg on left click
        if (Input.GetMouseButtonDown(1))
        {
            RotateSelector();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (canplaceobject && center != null)
                PlaceFurniture(center);
            else
                AudioManager.PlaySoundEffect(cannotplace);
        }
    }

    void CheckSelectorTouchesPlayer(GridLocation center)
    {
        GridSpace nextitemspace = currentlevelqueue.GetNextItemCost();
        // -- if we can place the object make sure the player is near it
        if (canplaceobject)
        {
            bool touchesplayer = false;
            switch (rotationpreview)
            {
                case 0:
                    for (int i = 0; i < nextitemspace.shape.Length; ++i)
                    {
                        if (nextitemspace.shape[i])
                        {
                            if (AnyGridNearPlayer0Rot(center, i))
                            {
                                // -- we are good, we can break
                                touchesplayer = true;
                                break;
                            }
                        }
                    }
                    break;
                case 90:
                    for (int i = 0; i < nextitemspace.shape.Length; ++i)
                    {
                        if (nextitemspace.shape[i])
                        {
                            if (AnyGridNearPlayer90Rot(center, i))
                            {
                                // -- we are good, we can break
                                touchesplayer = true;
                                break;
                            }
                        }
                    }
                    break;
                case 180:
                    for (int i = 0; i < nextitemspace.shape.Length; ++i)
                    {
                        if (nextitemspace.shape[i])
                        {
                            if (AnyGridNearPlayer180Rot(center, i))
                            {
                                // -- we are good, we can break
                                touchesplayer = true;
                                break;
                            }
                        }
                    }
                    break;
                case 270:
                    for (int i = 0; i < nextitemspace.shape.Length; ++i)
                    {
                        if (nextitemspace.shape[i])
                        {
                            if (AnyGridNearPlayer270Rot(center, i))
                            {
                                // -- we are good, we can break
                                touchesplayer = true;
                                break;
                            }
                        }
                    }
                    break;
            }

            if (!touchesplayer)
            {
                // - bummer. turn selector yellow so they know
                canplaceobject = false;

                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        selector.selectorpreview[i].color = Color.yellow;
                    }
                }
            }
        }
    }

    void UpdateSelectorStatus(out GridLocation cent)
    {
        int gridwidth = levelgridwidths[Globals.gCurrentLevel - 1];
        int gridheight = levelgridheights[Globals.gCurrentLevel - 1];

        Vector2 mp = Input.mousePosition;
        Vector3 mouseworld = cam.ScreenToWorldPoint(mp);
        mouseworld += new Vector3(gridentrysize / 2f, gridentrysize / 2f);

        GridLocation center = null;
        int j = 0;
        for (int i = 0; i < gridwidth * gridheight; ++i)
        {
            if (i > 0 && i % gridwidth == 0)
                j++;

            // -- check intersection with this gridloc
            if (MouseInside(mouseworld, grid[i % gridwidth, j]))
            {
                center = grid[i % gridwidth, j];
                break;
            }
        }

        if (center != null)
        {
            // -- set selector attrib
            selector.gameObject.SetActive(true);
            selector.transform.position = center.anchorpoint;

            // -- anchor next furniture sprite to selector 
            GridSpace nextitemspace = currentlevelqueue.GetNextItemCost();
            selector.SetSelectorFor(currentlevelqueue.GetNextItemCost());

            Vector3 fo = GetOffsetForFurniture(nextitemspace.widthoffset, nextitemspace.heightoffset);
            currentlevelqueue.GetNextItem().transform.position = selector.transform.position + fo;
        }
        else selector.gameObject.SetActive(false);

        UpdatePlaceableStatus(center);
        cent = center;
    }

    void UpdatePlaceableStatus(GridLocation center)
    {
        bool canplaceobjcached = true;
        if (!selector.gameObject.activeSelf)
        {
            canplaceobjcached = false;
        }

        GridSpace nextitemspace = currentlevelqueue.GetNextItemCost();
        // -- must go through all selector squares and check for overlaps of existing objects or null grid spots
        switch (rotationpreview)
        {
            case 0:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        if (CanPlaceOnGrid0Rot(center, i))
                        {
                            selector.selectorpreview[i].color = Color.green;
                        }
                        else
                        {
                            selector.selectorpreview[i].color = Color.red;
                            canplaceobjcached = false;
                        }
                    }
                }
                break;
            case 90:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        if (CanPlaceOnGrid90Rot(center, i))
                        {
                            selector.selectorpreview[i].color = Color.green;
                        }
                        else
                        {
                            selector.selectorpreview[i].color = Color.red;
                            canplaceobjcached = false;
                        }
                    }
                }
                break;
            case 180:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        if (CanPlaceOnGrid180Rot(center, i))
                        {
                            selector.selectorpreview[i].color = Color.green;
                        }
                        else
                        {
                            selector.selectorpreview[i].color = Color.red;
                            canplaceobjcached = false;
                        }
                    }
                }
                break;
            case 270:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        if (CanPlaceOnGrid270Rot(center, i))
                        {
                            selector.selectorpreview[i].color = Color.green;
                        }
                        else
                        {
                            selector.selectorpreview[i].color = Color.red;
                            canplaceobjcached = false;
                        }
                    }
                }
                break;
        }

        canplaceobject = canplaceobjcached;
    }

    void RotateSelector()
    {
        selector.transform.Rotate(Vector3.forward, 90f);
        currentlevelqueue.GetNextItem().transform.Rotate(Vector3.forward, 90f);
        rotationpreview += 90;
        currentlevelqueue.GetNextItem().SwapWidthHeightOffset();

        if (rotationpreview == 360)
            rotationpreview = 0;
    }

    void PlaceFurniture(GridLocation center)
    {
        bool breakoccur = false;
        //update grid with new furniture info
        UpdateGrid(center, out breakoccur);

        GridSpace cost = currentlevelqueue.GetNextItemCost();
        if (!cost.breakable && !cost.walkable)
            AudioManager.PlaySoundEffect(placeregular);
        else if (cost.walkable)
            AudioManager.PlaySoundEffect(placewalkable);
        else if (cost.breakable)
            AudioManager.PlaySoundEffect(placebreakable);

        if (breakoccur)
        {
            BreakFX(breakposition);
        }

        currentlevelqueue.PlaceNextItem();
        selector.transform.eulerAngles = Vector3.zero;
        rotationpreview = 0;
    }

    public static void BreakFX(Vector3 breakpos)
    {
        AudioManager.PlaySoundEffect(instance.breakablebreak);
        GameObject breakpsi = GameObject.Instantiate(instance.breakparticleprefab, breakpos, Quaternion.identity);

        instance.currentlevelqueue.BrokeVase();
    }

    void UpdateGrid(GridLocation center, out bool breakoccur)
    {
        breakoccur = false;
        GridSpace nextitemspace = currentlevelqueue.GetNextItemCost();
        switch (rotationpreview)
        {
            case 0:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        PlaceOnGrid0Rot(center, i, nextitemspace.walkable, nextitemspace.breakable, out breakoccur);
                    }
                }
                break;
            case 90:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        PlaceOnGrid90Rot(center, i, nextitemspace.walkable, nextitemspace.breakable, out breakoccur);
                    }
                }
                break;
            case 180:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        PlaceOnGrid180Rot(center, i, nextitemspace.walkable, nextitemspace.breakable, out breakoccur);
                    }
                }
                break;
            case 270:
                for (int i = 0; i < nextitemspace.shape.Length; ++i)
                {
                    if (nextitemspace.shape[i])
                    {
                        PlaceOnGrid270Rot(center, i, nextitemspace.walkable, nextitemspace.breakable, out breakoccur);
                    }
                }
                break;
        }
    }

    Vector3 GetOffsetForFurniture(int w, int h)
    {
        switch(rotationpreview)
        {
            case 0:
                return new Vector3((w - 1) * gridentrysize / 2f, (h - 1) * gridentrysize / 2f);
            case 90:
                return new Vector3(-(w - 1) * gridentrysize / 2f, (h - 1) * gridentrysize / 2f);
            case 180:
                return new Vector3(-(w - 1) * gridentrysize / 2f, -(h - 1) * gridentrysize / 2f);
            case 270:
                return new Vector3((w - 1) * gridentrysize / 2f, -(h - 1) * gridentrysize / 2f);
        }

        Debug.LogError("PreviewFurnitureRotation value does not exist.");
        return Vector3.zero;
    }

    bool MouseInside(Vector3 mouseworld, GridLocation loc)
    {
        Vector3 br = loc.anchorpoint + new Vector3(gridentrysize, gridentrysize);

        if (mouseworld.x <= loc.anchorpoint.x || mouseworld.x >= br.x ||
            mouseworld.y <= loc.anchorpoint.y || mouseworld.y >= br.y)
            return false;

        return true;
    }

    static bool playerclosedin = true;
    public static bool PlayerTrapped()
    {
        playerclosedin = true;
        instance.PathFromExitToPlayerExists(instance.player.GetCurrentGridLocation(), new List<GridLocation>());
        return playerclosedin;
    }

    // -- camefrom: 0 is right, 1 is down, 2 is left, 3 is up
    void PathFromExitToPlayerExists(GridLocation playerloc, List<GridLocation> traveled)
    {
        if (traveled.Contains(playerloc))
            return;

        traveled.Add(playerloc);

        if (playerloc == door)
        {
            playerclosedin = false;
            return;
        }

        GridLocation above = Above(playerloc);
        GridLocation right = Right(playerloc);
        GridLocation below = Below(playerloc);
        GridLocation left = Left(playerloc);

        if (above != null && CanWalkOn(above))
        {
            PathFromExitToPlayerExists(above, traveled);
        }
        if (right != null && CanWalkOn(right))
        {
            PathFromExitToPlayerExists(right, traveled);
        }
        if (below != null && CanWalkOn(below))
        {
            PathFromExitToPlayerExists(below, traveled);
        }
        if (left != null && CanWalkOn(left))
        {
            PathFromExitToPlayerExists(left, traveled);
        }

        return;
    }

    // ================================================================================================
    // -- LOTS OF GRID CHECKS
    // ================================================================================================

    bool CanPlaceOnGrid0Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 0:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return upperleft != null && CanPlaceAt(upperleft);
            // -- top
            case 1:
                first = Above(center);
                return first != null && CanPlaceAt(first);
            // -- top right
            case 2:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return upperright != null && CanPlaceAt(upperright);

            // -- left
            case 3:
                first = Left(center);
                return first != null && CanPlaceAt(first);
            // -- middle
            case 4:
                return CanPlaceAt(center);
            // -- right
            case 5:
                first = Right(center);
                return first != null && CanPlaceAt(first);

            // -- bottom left
            case 6:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return bottomleft != null && CanPlaceAt(bottomleft);
            // -- bottom
            case 7:
                first = Below(center);
                return first != null && CanPlaceAt(first);
            // -- bottom right
            case 8:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return bottomright != null && CanPlaceAt(bottomright);
        }

        return false;
    }

    bool CanPlaceOnGrid90Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 2:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return upperleft != null && CanPlaceAt(upperleft);
            // -- top
            case 5:
                first = Above(center);
                return first != null && CanPlaceAt(first);
            // -- top right
            case 8:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return upperright != null && CanPlaceAt(upperright);

            // -- left
            case 1:
                first = Left(center);
                return first != null && CanPlaceAt(first);
            // -- middle
            case 4:
                return CanPlaceAt(center);
            // -- right
            case 7:
                first = Right(center);
                return first != null && CanPlaceAt(first);

            // -- bottom left
            case 0:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return bottomleft != null && CanPlaceAt(bottomleft);
            // -- bottom
            case 3:
                first = Below(center);
                return first != null && CanPlaceAt(first);
            // -- bottom right
            case 6:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return bottomright != null && CanPlaceAt(bottomright);
        }

        return false;
    }

    bool CanPlaceOnGrid180Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 8:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return upperleft != null && CanPlaceAt(upperleft);
            // -- top
            case 7:
                first = Above(center);
                return first != null && CanPlaceAt(first);
            // -- top right
            case 6:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return upperright != null && CanPlaceAt(upperright);

            // -- left
            case 5:
                first = Left(center);
                return first != null && CanPlaceAt(first);
            // -- middle
            case 4:
                return CanPlaceAt(center);
            // -- right
            case 3:
                first = Right(center);
                return first != null && CanPlaceAt(first);

            // -- bottom left
            case 2:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return bottomleft != null && CanPlaceAt(bottomleft);
            // -- bottom
            case 1:
                first = Below(center);
                return first != null && CanPlaceAt(first);
            // -- bottom right
            case 0:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return bottomright != null && CanPlaceAt(bottomright);
        }

        return false;
    }

    bool CanPlaceOnGrid270Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 6:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return upperleft != null && CanPlaceAt(upperleft);
            // -- top
            case 3:
                first = Above(center);
                return first != null && CanPlaceAt(first);
            // -- top right
            case 0:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return upperright != null && CanPlaceAt(upperright);

            // -- left
            case 7:
                first = Left(center);
                return first != null && CanPlaceAt(first);
            // -- middle
            case 4:
                return CanPlaceAt(center);
            // -- right
            case 1:
                first = Right(center);
                return first != null && CanPlaceAt(first);

            // -- bottom left
            case 8:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return bottomleft != null && CanPlaceAt(bottomleft);
            // -- bottom
            case 5:
                first = Below(center);
                return first != null && CanPlaceAt(first);
            // -- bottom right
            case 2:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return bottomright != null && CanPlaceAt(bottomright);
        }

        return false;
    }

    bool AnyGridNearPlayer0Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 0:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return PlayerNextTo(upperleft);
            // -- top
            case 1:
                first = Above(center);
                return PlayerNextTo(first);
            // -- top right
            case 2:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return PlayerNextTo(upperright);
            // -- left
            case 3:
                first = Left(center);
                return PlayerNextTo(first);
            // -- middle
            case 4:
                return PlayerNextTo(center);
            // -- right
            case 5:
                first = Right(center);
                return PlayerNextTo(first);

            // -- bottom left
            case 6:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return PlayerNextTo(bottomleft);
            // -- bottom
            case 7:
                first = Below(center);
                return PlayerNextTo(first);
            // -- bottom right
            case 8:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return PlayerNextTo(bottomright);
        }

        return false;
    }

    bool AnyGridNearPlayer90Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 2:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return PlayerNextTo(upperleft);
            // -- top
            case 5:
                first = Above(center);
                return PlayerNextTo(first);
            // -- top right
            case 8:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return PlayerNextTo(upperright);
            // -- left
            case 1:
                first = Left(center);
                return PlayerNextTo(first);
            // -- middle
            case 4:
                return PlayerNextTo(center);
            // -- right
            case 7:
                first = Right(center);
                return PlayerNextTo(first);

            // -- bottom left
            case 0:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return PlayerNextTo(bottomleft);
            // -- bottom
            case 3:
                first = Below(center);
                return PlayerNextTo(first);
            // -- bottom right
            case 6:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return PlayerNextTo(bottomright);
        }

        return false;
    }

    bool AnyGridNearPlayer180Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 8:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return PlayerNextTo(upperleft);
            // -- top
            case 7:
                first = Above(center);
                return PlayerNextTo(first);
            // -- top right
            case 6:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return PlayerNextTo(upperright);
            // -- left
            case 5:
                first = Left(center);
                return PlayerNextTo(first);
            // -- middle
            case 4:
                return PlayerNextTo(center);
            // -- right
            case 3:
                first = Right(center);
                return PlayerNextTo(first);

            // -- bottom left
            case 2:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return PlayerNextTo(bottomleft);
            // -- bottom
            case 1:
                first = Below(center);
                return PlayerNextTo(first);
            // -- bottom right
            case 0:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return PlayerNextTo(bottomright);
        }

        return false;
    }

    bool AnyGridNearPlayer270Rot(GridLocation center, int i)
    {
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 6:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                return PlayerNextTo(upperleft);
            // -- top
            case 3:
                first = Above(center);
                return PlayerNextTo(first);
            // -- top right
            case 0:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                return PlayerNextTo(upperright);
            // -- left
            case 7:
                first = Left(center);
                return PlayerNextTo(first);
            // -- middle
            case 4:
                return PlayerNextTo(center);
            // -- right
            case 1:
                first = Right(center);
                return PlayerNextTo(first);

            // -- bottom left
            case 8:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                return PlayerNextTo(bottomleft);
            // -- bottom
            case 5:
                first = Below(center);
                return PlayerNextTo(first);
            // -- bottom right
            case 2:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                return PlayerNextTo(bottomright);
        }

        return false;
    }

    void PlaceForLocation(GridLocation gl, bool walkable, bool breakable, out bool breakoccur)
    {
        breakoccur = false;
        bool shatterpossible = grid[gl.x, gl.y].hasbreakableitem;

        grid[gl.x, gl.y].occupied |= !breakable;
        grid[gl.x, gl.y].walkable = breakable ?
            grid[gl.x, gl.y].walkable : walkable;
        grid[gl.x, gl.y].hasbreakableitem |= breakable;

        if (shatterpossible)
        {
            breakoccur = true;
            breakposition = grid[gl.x, gl.y].anchorpoint;
            if(grid[gl.x, gl.y].RemoveBreakable())
                GridController.BreakFX(grid[gl.x, gl.y].anchorpoint);
        }
    }

    void PlaceOnGrid0Rot(GridLocation center, int i, bool walkable, bool breakable, out bool breakoccur)
    {
        breakoccur = false;
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 0:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                PlaceForLocation(upperleft, walkable, breakable, out breakoccur);
                return;
            // -- top
            case 1:
                first = Above(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- top right
            case 2:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                PlaceForLocation(upperright, walkable, breakable, out breakoccur);
                return;

            // -- left
            case 3:
                first = Left(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- middle
            case 4:
                PlaceForLocation(center, walkable, breakable, out breakoccur);
                return;
            // -- right
            case 5:
                first = Right(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;

            // -- bottom left
            case 6:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                PlaceForLocation(bottomleft, walkable, breakable, out breakoccur);
                return;
            // -- bottom
            case 7:
                first = Below(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- bottom right
            case 8:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                PlaceForLocation(bottomright, walkable, breakable, out breakoccur);
                return;
        }

    }

    void PlaceOnGrid90Rot(GridLocation center, int i, bool walkable, bool breakable, out bool breakoccur)
    {
        breakoccur = false;
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 2:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                PlaceForLocation(upperleft, walkable, breakable, out breakoccur);
                return;
            // -- top
            case 5:
                first = Above(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- top right
            case 8:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                PlaceForLocation(upperright, walkable, breakable, out breakoccur);
                return;

            // -- left
            case 1:
                first = Left(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- middle
            case 4:
                PlaceForLocation(center, walkable, breakable, out breakoccur);
                return;
            // -- right
            case 7:
                first = Right(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;

            // -- bottom left
            case 0:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                PlaceForLocation(bottomleft, walkable, breakable, out breakoccur);
                return;
            // -- bottom
            case 3:
                first = Below(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- bottom right
            case 6:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                PlaceForLocation(bottomright, walkable, breakable, out breakoccur);
                return;
        }

        return;
    }

    void PlaceOnGrid180Rot(GridLocation center, int i, bool walkable, bool breakable, out bool breakoccur)
    {
        breakoccur = false;
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 8:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                PlaceForLocation(upperleft, walkable, breakable, out breakoccur);
                return;
            // -- top
            case 7:
                first = Above(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- top right
            case 6:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                PlaceForLocation(upperright, walkable, breakable, out breakoccur);
                return;

            // -- left
            case 5:
                first = Left(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- middle
            case 4:
                PlaceForLocation(center, walkable, breakable, out breakoccur);
                return;
            // -- right
            case 3:
                first = Right(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;

            // -- bottom left
            case 2:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                PlaceForLocation(bottomleft, walkable, breakable, out breakoccur);
                return;
            // -- bottom
            case 1:
                first = Below(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- bottom right
            case 0:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                PlaceForLocation(bottomright, walkable, breakable, out breakoccur);
                return;
        }

        return;
    }

    void PlaceOnGrid270Rot(GridLocation center, int i, bool walkable, bool breakable, out bool breakoccur)
    {
        breakoccur = false;
        GridLocation first;
        switch (i)
        {
            // -- top left
            case 6:
                first = Above(center);
                GridLocation upperleft = first != null ? Left(first) : null;
                PlaceForLocation(upperleft, walkable, breakable, out breakoccur);
                return;
            // -- top
            case 3:
                first = Above(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- top right
            case 0:
                first = Above(center);
                GridLocation upperright = first != null ? Right(first) : null;
                PlaceForLocation(upperright, walkable, breakable, out breakoccur);
                return;

            // -- left
            case 7:
                first = Left(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- middle
            case 4:
                PlaceForLocation(center, walkable, breakable, out breakoccur);
                return;
            // -- right
            case 1:
                first = Right(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;

            // -- bottom left
            case 8:
                first = Below(center);
                GridLocation bottomleft = first != null ? Left(first) : null;
                PlaceForLocation(bottomleft, walkable, breakable, out breakoccur);
                return;
            // -- bottom
            case 5:
                first = Below(center);
                PlaceForLocation(first, walkable, breakable, out breakoccur);
                return;
            // -- bottom right
            case 2:
                first = Below(center);
                GridLocation bottomright = first != null ? Right(first) : null;
                PlaceForLocation(bottomright, walkable, breakable, out breakoccur);
                return;
        }

        return;
    }
}