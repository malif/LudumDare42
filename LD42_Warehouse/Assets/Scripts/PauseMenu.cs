using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum MenuState
{
    MainMenu,
    Hidden,
    Pause,
    MainMenu_Instructions,
    Pause_Instructions,
    GameOver
}
public class PauseMenu : MonoBehaviour {

    private MenuState state = MenuState.MainMenu;
    public Text ResumeText = null;
    public GameObject RestartButton = null;

    public GameObject InstructionsScreen = null;

    public GameObject InGameGUI = null;

    public GameObject GameOverGUI = null;

    public GameObject GameNameGUI = null;
    // Use this for initialization
	void Start ()
    {
        RestartButton.SetActive(false);
	}

    public void StartGame()
    {
        GameLogic.Instance.UnpauseGame();
    }

    public void Instructions()
    {
        if (state == MenuState.MainMenu)
        {
            state = MenuState.MainMenu_Instructions;
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            InstructionsScreen.SetActive(true);
            GameNameGUI.SetActive(false);
        }
        else if (state == MenuState.Pause)
        {
            state = MenuState.Pause_Instructions;
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            InstructionsScreen.SetActive(true);
            InGameGUI.SetActive(false);
        }
        else if (state == MenuState.MainMenu_Instructions)
        {
            InstructionsScreen.SetActive(false);
            state = MenuState.MainMenu;
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            RestartButton.SetActive(false);
            GameNameGUI.SetActive(true);
        }
        else if (state == MenuState.Pause_Instructions)
        {
            InstructionsScreen.SetActive(false);            
            InGameGUI.SetActive(true);
            state = MenuState.Pause;
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            ResumeText.text = "Resume Game";
        }
    }

    public void NewGame()
    {
        GameLogic.Instance.Reset();

    }

    public void ExitGame()
    {
        Application.Quit();
    }
    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case MenuState.MainMenu:
                {
                    if(GameLogic.Instance.IsGameStarted())
                    {
                        state = MenuState.Hidden;
                        for(int i = 0; i < transform.childCount; ++i)
                        {
                            transform.GetChild(i).gameObject.SetActive(false);
                        }
                        InGameGUI.SetActive(true);
                        GameNameGUI.SetActive(false);
                    }
                }
                break;
            case MenuState.Hidden:
                {
                    if(GameLogic.Instance.IsPaused())
                    {
                        state = MenuState.Pause;
                        for (int i = 0; i < transform.childCount; ++i)
                        {
                            transform.GetChild(i).gameObject.SetActive(true);
                        }
                        ResumeText.text = "Resume Game";
                    }
                    else if(GameLogic.Instance.IsGameOver())
                    {
                        state = MenuState.GameOver;
                        RestartButton.SetActive(true);
                        InGameGUI.SetActive(false);
                        GameOverGUI.SetActive(true);
                    }
                }
                break;
            case MenuState.Pause:
                {
                    if (!GameLogic.Instance.IsPaused())
                    {
                        state = MenuState.Hidden;
                        for (int i = 0; i < transform.childCount; ++i)
                        {
                            transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                }
                break;
            case MenuState.GameOver:
                {
                }
                break;
            default:
                break;
        }
    }
}
