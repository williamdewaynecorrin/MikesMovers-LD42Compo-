using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemQueue : MonoBehaviour {

    public LevelFurnitureList[] levelprefablists;
    public Transform[] movingtruckspawn;
    public int[] levelclocktimeseconds;
    private int queuelength = 5;
    private int clocktimeticks;
    public TextMesh clocktext;
    public TextMesh queuetext;
    public TextMesh countdowntext;
    public TextMesh scoretext;
    [HideInInspector]
    public List<Furniture> levelqueue;
    [HideInInspector]
    public bool roundover = false;
    bool countdown = true;
    int cd = 60 * 3;
    int ticksincelastplace = 0;
    bool won = false;

    [SerializeField]
    private AudioClip[] hurryup;
    [SerializeField]
    private AudioClip win;
    [SerializeField]
    private AudioClip lose;

    private void Awake()
    {
        vasebreakcount = 0;
        OnLevelStart();
    }

    void OnLevelStart()
    {
        List<Furniture> prefablist = levelprefablists[Globals.gCurrentLevel - 1].furniturelist;
        queuelength = prefablist.Count;
        for (int i = 0; i < queuelength; ++i)
        {
            int r = Random.Range(0, prefablist.Count);
            Vector3 offscreen = new Vector3(10000f, 10000f, 0f);
            Furniture instance = GameObject.Instantiate(prefablist[r], offscreen, Quaternion.identity);

            // -- show next 3 (first one is active)
            if (i < movingtruckspawn.Length)
            {
                instance.transform.position = movingtruckspawn[i].position;
            }

            levelqueue.Add(instance);

            prefablist.RemoveAt(r);
        }
        clocktimeticks = levelclocktimeseconds[Globals.gCurrentLevel - 1] * 60;
        this.scoretext.text = "SCORE: " + Globals.gCurrentScore.ToString();
    }

    private void Start()
    {
        countdowntext.color = Color.white;
        queuetext.text = string.Format("NEXT: {0}/{1}", (queuelength - levelqueue.Count) + 1, queuelength);
    }

    private void FixedUpdate()
    {
        if(countdown)
        {
            cd--;
            countdowntext.text = "READY? " + (((int)(cd / 60f)) + 1).ToString();

            if (cd == 0)
            {
                countdown = false;
                countdowntext.text = "GO!";
            }
            return;
        }

        if (!roundover)
        {
            if (clocktimeticks == (levelclocktimeseconds[Globals.gCurrentLevel - 1] - 3) * 60)
                countdowntext.text = "";

            clocktimeticks--;
            float totalsec = (float)clocktimeticks;
            totalsec /= 60f;
            int min = (int)(totalsec / 60f);
            int sec = (int)(totalsec - min * 60);

            clocktext.text = sec < 10 ? string.Format("TIME: 0{0}:0{1}", min, sec) :
                string.Format("TIME: 0{0}:{1}", min, sec);

            if (min == 0)
            {
                CheckPlayAnnouncerSound();
            }

            ticksincelastplace++;

            if (clocktimeticks == 0)
            {
                GameOver("You ran out of time.", "Yer fired!");
                AudioManager.PlaySoundEffect(lose);
            }
        }
    }

    IEnumerator WaitForTransition()
    {
        yield return new WaitForSeconds(5f);

        if(won)
        {
            if (Globals.GetTotalLevels() == Globals.gCurrentLevel)
            {
                //you win
                if (Globals.gCurrentScore > PlayerPrefs.GetInt("highscore"))
                    PlayerPrefs.SetInt("highscore", Globals.gCurrentScore);
                if (Globals.gCurrentLevel > PlayerPrefs.GetInt("highlevel"))
                    PlayerPrefs.SetInt("highlevel", Globals.gCurrentLevel);

                Globals.gCurrentScore = 0;
                Globals.gCurrentLevel = 1;
                UnityEngine.SceneManagement.SceneManager.LoadScene("menu");
            }
            else
            {
                Globals.gCurrentLevel++;
                //keep going
                UnityEngine.SceneManagement.SceneManager.LoadScene("level1");
            }
        }
        else
        {
            if(Globals.gCurrentScore > PlayerPrefs.GetInt("highscore"))
                PlayerPrefs.SetInt("highscore", Globals.gCurrentScore);
            if (Globals.gCurrentLevel > PlayerPrefs.GetInt("highlevel"))
                PlayerPrefs.SetInt("highlevel", Globals.gCurrentLevel);

            Globals.gCurrentScore = 0;
            Globals.gCurrentLevel = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene("menu");
        }
    }

    void CheckPlayAnnouncerSound()
    {
        switch (clocktimeticks)
        {
            case 12 * 60:
                AudioManager.PlaySoundEffect(hurryup[0]);
                break;
            case 11 * 60:
                AudioManager.PlaySoundEffect(hurryup[1]);
                break;
            case 10 * 60:
                AudioManager.PlaySoundEffect(hurryup[2]);
                break;
            case 9 * 60:
                AudioManager.PlaySoundEffect(hurryup[3]);
                break;
            case 8 * 60:
                AudioManager.PlaySoundEffect(hurryup[4]);
                break;
            case 7 * 60:
                AudioManager.PlaySoundEffect(hurryup[5]);
                break;
            case 6 * 60:
                AudioManager.PlaySoundEffect(hurryup[6]);
                break;
            case 5 * 60:
                AudioManager.PlaySoundEffect(hurryup[7]);
                break;
            case 4 * 60:
                AudioManager.PlaySoundEffect(hurryup[8]);
                break;
            case 3 * 60:
                AudioManager.PlaySoundEffect(hurryup[9]);
                break;
            case 2 * 60:
                AudioManager.PlaySoundEffect(hurryup[10]);
                break;
        }
    }

    public bool RoundStarted()
    {
        return !countdown;
    }

    public bool RoundOver()
    {
        return roundover;
    }

    public Furniture GetNextItem()
    {
        return levelqueue[0];
    }

    void GameOver(string reason, string demotivator)
    {
        roundover = true;
        won = false;
        countdowntext.text = "GAME OVER.\n" + reason;
        countdowntext.color = Color.red;
        queuetext.text = string.Format(demotivator);

        StartCoroutine(WaitForTransition());
    }

    void RoundWon(string tagline, string motivator)
    {
        //YOU WIN
        roundover = true;
        won = true;
        countdowntext.text = "ROUND OVER!\n" + tagline;
        countdowntext.color = Color.green;
        queuetext.text = string.Format(motivator);
        AudioManager.PlaySoundEffect(win);
        StartCoroutine(WaitForTransition());
    }

    public void PlaceNextItem()
    {
        levelqueue.RemoveAt(0);

        // -- did you just break the path
        if(GridController.PlayerTrapped())
        {
            GameOver("You trapped yourself inside the house.", "What were you thinking?!");
            AudioManager.PlaySoundEffect(lose);
        }

        GiveScore();

        if (levelqueue.Count == 0)
        {
            RoundWon("Nice Movin'.", "You deserve a raise.");
        }
        else
        {
            queuetext.text = string.Format("NEXT: {0}/{1}", (queuelength - levelqueue.Count) + 1, queuelength);
            //move em down the list
            for (int i = 0; i < levelqueue.Count; ++i)
            {
                // -- we only care about displayable items
                if (i == movingtruckspawn.Length)
                    break;

                levelqueue[i].transform.position = movingtruckspawn[i].position;
            }
        }
    }

    void GiveScore()
    {
        int buff = 2;
        int score = ((int)Mathf.Sqrt(ticksincelastplace * ticksincelastplace) * 1 * buff);
        score = score >= 1000 * buff ? 10 * buff : 1000 * buff - score;
        ticksincelastplace = 0;
        Globals.gCurrentScore += score;
        scoretext.text = "SCORE: " + Globals.gCurrentScore.ToString();

        AudioManager.PopText(GridController.instance.player.transform.position + Vector3.up * 0.25f,
            Color.black, "+" + score.ToString() + "!");
    }

    [SerializeField]
    private AudioClip[] vasebrokennoises;
    int vasebreakcount = 0;
    public void BrokeVase()
    {
        vasebreakcount++;
        int minusscore = 10000 * vasebreakcount * vasebreakcount;
        Globals.gCurrentScore -= minusscore;
        scoretext.text = "SCORE: " + Globals.gCurrentScore.ToString();

        int r = Random.Range(0, vasebrokennoises.Length);
        AudioManager.PlaySoundEffect(vasebrokennoises[r]);

        AudioManager.PopText(GridController.instance.player.transform.position + Vector3.up * 0.25f,
            Color.red, "-" + minusscore.ToString() + "! :(");
    }

    public GridSpace GetNextItemCost()
    {
        return GetNextItem().cost;
    }
}

[System.Serializable]
public class LevelFurnitureList
{
    public List<Furniture> furniturelist;
}