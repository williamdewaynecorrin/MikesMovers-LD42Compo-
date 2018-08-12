using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    private AudioSource musicsrc;
    private static AudioManager instance = null;
    [SerializeField]
    private AudioClip musicclip;
    [SerializeField]
    private GameObject poptextprefab;
	// Use this for initialization
	void Awake ()
    {
        if (instance == null)
        {
            instance = this;
            musicsrc = GetComponent<AudioSource>();
            musicsrc.clip = musicclip;
            musicsrc.Play();
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (instance.musicsrc.clip != musicclip)
            {
                instance.musicsrc.clip = musicclip;
                instance.musicsrc.Play();
            }
            GameObject.Destroy(this.gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static PopText PopText(Vector3 pos, Color c, string text)
    {
        GameObject pinst = GameObject.Instantiate(instance.poptextprefab, pos, Quaternion.identity);
        PopText pt = pinst.GetComponent<PopText>();
        pt.textmesh.color = c;
        pt.textmesh.text = text;

        return pt;
    }

    // -- changes the music volume
    public static void SetMusicVolume(float v)
    {
        if(instance != null)
        {
            instance.musicsrc.volume = v;
        }
    }

    // -- creates a one shot audio and then plays and deletes after done
    public static AudioSource PlaySoundEffect(AudioClip clip)
    {
        return PlaySoundEffect(clip, true);
    }

    // -- creates a one shot audio and then plays and deletes after done if desired
    public static AudioSource PlaySoundEffect(AudioClip clip, bool delete)
    {
        GameObject newAudio = new GameObject("ONE SHOT AUDIO");
        AudioSource asrc = newAudio.AddComponent<AudioSource>();
        asrc.clip = clip;
        asrc.volume = Globals.kSFXVolume;
        asrc.Play();

        if (delete)
            GameObject.Destroy(newAudio, clip.length);

        return asrc;
    }
}
