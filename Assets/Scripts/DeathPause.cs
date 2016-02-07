using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathPause : MonoBehaviour
{
    public static DeathPause S;
    
    /* A GUI Death Message */
    private Canvas self;
    public Text countdown;
    public float respawnTimer = 1.0f;

    // Happens before the thing starts up
    void Awake()
    {
        S = this;
    }

    // Use this for initialization
    void Start()
    {
        self = GameObject.FindGameObjectWithTag("DeathDisplay").GetComponent<Canvas>();
        self.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }


    /* Co-Routines */

    // This co-routine pauses the game, restarting the level after 3 seconds or when the player hits the spacebar
    IEnumerator PauseWhenDead()
    {
        // Pause the game and display a message
        Time.timeScale = 0;
        self.enabled = true;
        float endPause = Time.realtimeSinceStartup + respawnTimer;

        // After 3 seconds or if the user presses [SPACEBAR], reload the level
        while (Time.realtimeSinceStartup < endPause && !Input.GetKeyDown("space"))
        {
            countdown.text = "" + (Mathf.Floor(endPause - Time.realtimeSinceStartup) + 1);
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    // Runs the coroutine
    public void PauseDead()
    {
        StartCoroutine(PauseWhenDead());
    }
}