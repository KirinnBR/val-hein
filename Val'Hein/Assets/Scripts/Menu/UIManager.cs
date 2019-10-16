using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
public class UIManager : Singleton<UIManager>
{
	[Header("Input Settings")]
	[SerializeField] private KeyCode keyToPause = KeyCode.Escape;
	[Space]
	[Header("Variables")]
	[SerializeField] private MainMenu mainMenu;
	[SerializeField] private PauseMenu pauseMenu;
	[SerializeField] private Camera dummyCamera;

	private void Start()
	{
		LoadMainMenu();
	}

	private void Update()
	{
		if (GameManager.Instance.CurrentGameState == GameManager.GameState.MainMenu)
		{
			//All the code here when in GameMenu.
			if (Input.GetKeyDown(KeyCode.Space))
			{
				mainMenu.FadeOut();
			}
		}
		else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Running)
		{
			//All the code here when in game.
		}
		else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
		{
			//All the code here when paused.
			if (Input.GetKeyDown(keyToPause))
			{
				if (GameManager.Instance.CurrentGameState == GameManager.GameState.Running)
				{
					GameManager.Instance.ChangeGameState(GameManager.GameState.Paused);
				}
				else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
				{
					GameManager.Instance.ChangeGameState(GameManager.GameState.Running);
				}
			}
		}
	}

	public void UpdateUI()
	{
		switch (GameManager.Instance.CurrentGameState)
		{
			case GameManager.GameState.MainMenu:
				UnloadPauseMenu();
				break;
			case GameManager.GameState.Running:
				UnloadAllUI();
				break;
			case GameManager.GameState.Paused:
				LoadPauseMenu();
				break;
		}
	}

	private void UnloadAllUI()
	{
		UnloadMainMenu();
		UnloadPauseMenu();
	}


	private void LoadPauseMenu()
	{
		pauseMenu.GetComponent<Canvas>().enabled = true;
	}

	private void UnloadPauseMenu()
	{
		pauseMenu.GetComponent<Canvas>().enabled = false;
	}

	private void LoadMainMenu()
	{
		mainMenu.FadeIn();
		UnloadPauseMenu();
	}

	private void UnloadMainMenu()
	{
		mainMenu.GetComponent<Canvas>().enabled = false;
		dummyCamera.GetComponent<Camera>().enabled = false;
	}



}
