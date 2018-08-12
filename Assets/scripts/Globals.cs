using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static int gCurrentScore = 0;
    public static int gCurrentLevel = 1;
    public static float kEpsilon = 0.001f;
    public static float kMusicVolume
    {
        get { return _kmusicvolume; }
        set
        {
            _kmusicvolume = value;
            AudioManager.SetMusicVolume(_kmusicvolume);
        }
    }
    public static float kSFXVolume
    {
        get { return _ksfxvolume; }
        set { _ksfxvolume = value; }
    }

    private static float _kmusicvolume = 1.0f;
    private static float _ksfxvolume = 1.0f;

    public static int GetTotalLevels()
    {
        return GridController.instance.levelgridheights.Length;
    }
}
