using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSpace
{
    public int widthoffset = 1;
    public int heightoffset = 1;
    public bool walkable = true;
    public bool breakable = false;
    public bool[] shape = new bool[9];

    //public GridSpace(bool canwalkover,
    //                 bool tl, bool t, bool tr,
    //                 bool l, bool m, bool r, 
    //                 bool bl, bool b, bool br)
    //{
    //    this.walkable = canwalkover;
    //    shape = new bool[9]
    //    {
    //        tl, t, tr, l, m, r, bl, b, br
    //    };
    //}
}
