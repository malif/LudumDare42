using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance = null;
    private bool Paused = true;
    private bool GameOver = false;

    private bool GameStarted = false;

    public GameObject canvas = null;

    private int Score = 0;
    private int MissedArrivals = 0;

    public float ArrivalInterval = 10.0f;
    public float TruckWaitTime = 5.0f;

    public float ArrivalModifier = 0.5f;
    public float TruckModifier = 1.0f;

    public float MinArrivalInterval = 3.0f;
    public float MaxTruckWaitTime = 30.0f;

    public float ArrivalMissBonus = 2.0f;
    public float TruckMissBonus = 5.0f;

    private float StartInterval;
    private float StartWaitTime;

    void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
        {

            //if not, set instance to this
            Instance = this;
        }
        else if (Instance != this)
        { 
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }

        StartInterval = ArrivalInterval;
        StartWaitTime = TruckWaitTime;

        DontDestroyOnLoad(gameObject);
    }
    // Use this for initialization
    void Start ()
    {
		if(GameStarted)
        {
            UnpauseGame();
        }
	}

    public void AddMissedArrival()
    {
        ++MissedArrivals;
        ArrivalInterval += ArrivalMissBonus;
        TruckWaitTime -= TruckMissBonus;
        if (MissedArrivals >= 5)
        {
            Paused = true;
            GameOver = true;
        }
    }

    public int GetMissedArrivals()
    {
        
        return MissedArrivals;
    }

    public void AddDeliveryScore(int count)
    {
        Score += (100 * count);
        ArrivalInterval = Mathf.Max(ArrivalInterval - ArrivalModifier, MinArrivalInterval);
        TruckWaitTime = Mathf.Min(TruckWaitTime + TruckWaitTime, MaxTruckWaitTime);
    }

    public int GetScore()
    {
        return Score;
    }

    public bool InputAllowed()
    {
        return !Paused;
    }

    public bool IsGameStarted()
    {
        return GameStarted;
    }
    public bool IsPaused()
    {
        return Paused && GameStarted && !GameOver;
    }

    public bool IsGameOver()
    {
        return GameOver;
    }

    public void UnpauseGame()
    {
        Paused = false;
        GameStarted = true;
    }

    public void Reset()
    {
        Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
        UnpauseGame();
        Score = 0;
        GameOver = false;
        ArrivalInterval = StartInterval;
        TruckWaitTime = StartWaitTime;
    }


    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Paused = !Paused;
        }
    }

    public float DeltaTime()
    {
        if(Paused)
            return 0.0f;
        return Time.deltaTime;
    }
}
