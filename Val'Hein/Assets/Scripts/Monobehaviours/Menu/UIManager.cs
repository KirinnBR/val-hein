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
	[SerializeField] private OptionsMenu optMenu;
	[SerializeField] private MainMenu mainMenu;
	[SerializeField] private PauseMenu pauseMenu;
	[SerializeField] private UnityEngine.Camera dummyCamera;

	private void Start()
	{
		LoadMainMenu();
	}

	private void Update()
	{
		if (GameManager.Instance.CurrentGameState == GameManager.GameState.MainMenu)
		{
			//All the code here when in GameMenu.
			
		}
		else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Running)
		{
			//All the code here when in game.
			if (Input.GetKeyDown(keyToPause))
			{
				GameManager.Instance.ChangeGameState(GameManager.GameState.Paused);
			}
		}
		else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
		{
			//All the code here when paused.
			if (Input.GetKeyDown(keyToPause))
			{
				GameManager.Instance.ChangeGameState(GameManager.GameState.Running);
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

	public void UnloadAllUI()
	{
		UnloadMainMenu();
		UnloadPauseMenu();
		UnloadOptionsMenu();
		dummyCamera.GetComponent<UnityEngine.Camera>().enabled = false;
	}

	public void LoadMainMenu()
	{
		mainMenu.gameObject.SetActive(true);
	}

	public void UnloadMainMenu()
	{
		mainMenu.gameObject.SetActive(false);
	}

	public void LoadPauseMenu()
	{
		pauseMenu.gameObject.SetActive(true);
	}

	public void UnloadPauseMenu()
	{
		pauseMenu.gameObject.SetActive(false);
	}

	public void LoadOptionsMenu()
	{
		optMenu.gameObject.SetActive(true);
	}

	public void UnloadOptionsMenu()
	{
		optMenu.gameObject.SetActive(false);
	}

}
