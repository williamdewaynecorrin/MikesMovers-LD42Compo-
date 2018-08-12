using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLocation
{
    public int x = 0;
    public int y = 0;
    public bool occupied = false;
    public bool walkable = true;
    public bool hasbreakableitem = false;
    public Vector3 anchorpoint;
    public bool door = false;

    public GridLocation(int x, int y, Vector3 anchor)
    {
        this.x = x;
        this.y = y;
        this.anchorpoint = anchor;
    }

    public GridLocation(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.anchorpoint = GridController.GetWorldPosition(x, y);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }

    public override bool Equals(object other)
    {
        GridLocation obj = (other as GridLocation);
        return obj != null && obj.x == this.x && obj.y == this.y;
    }

    public static GridLocation Zero
    {
        get
        {
            return new GridLocation(0, 0, GridController.GetWorldPosition(0, 0));
        }
    }

    public bool RemoveBreakable()
    {
        Furniture[] furniturelist = GameObject.FindObjectsOfType<Furniture>();

        for (int i = 0; i < furniturelist.Length; ++i)
        {
            if (furniturelist[i].cost.breakable)
            {
                if (Mathf.Abs(furniturelist[i].transform.position.x - this.anchorpoint.x) < 0.001f &&
                    Mathf.Abs(furniturelist[i].transform.position.y - this.anchorpoint.y) < 0.001f)
                {
                    this.hasbreakableitem = false;
                    GameObject.Destroy(furniturelist[i].gameObject);
                    return true;
                }
            }
        }

        return false;
    }
}