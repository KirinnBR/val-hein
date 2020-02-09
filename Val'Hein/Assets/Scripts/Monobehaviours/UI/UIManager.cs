using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0649
public class UIManager : Singleton<UIManager>
{
	[SerializeField] private Camera dummyCamera;

	[Header("Input Settings")]
	[SerializeField] private KeyCode keyToPause = KeyCode.Escape;

	[Header("Options Menu Canvas")]
	[SerializeField] private GameObject optionsMenu;
	[SerializeField] private Button backButton;

	[Header("Main Menu Canvas")]
	[SerializeField] private GameObject mainMenu;
	[SerializeField] private Button startButton;
	[SerializeField] private Button optButton;
	[SerializeField] private Button quitButton;

	[Header("Pause Menu Canvas")]
	[SerializeField] private GameObject pauseMenu;

	[Header("In Game Canvas")]
	[SerializeField] private GameObject inGame;

	private void Start()
	{
		backButton.onClick.AddListener(OptionsBackButtonClicked);
		startButton.onClick.AddListener(StartButtonClicked);
		optButton.onClick.AddListener(OptionsButtonClicked);
		quitButton.onClick.AddListener(QuitButtonClicked);
		GameManager.Instance.onSceneLoadedComplete.AddListener(OnLoadInGame);
		mainMenu.SetActive(true);
	}

	private void Update()
	{
		var gameManager = GameManager.Instance;
		var curState = gameManager.CurrentGameState;
		switch (curState)
		{
			case GameManager.GameState.MainMenu:
				//All the code here when in GameMenu.
				break;
			case GameManager.GameState.Running:
				//All the code here when in game.
				if (Input.GetKeyDown(keyToPause))
				{
					gameManager.ChangeGameState(GameManager.GameState.Paused);
				}
				break;
			case GameManager.GameState.Paused:
				//All the code here when paused.
				if (Input.GetKeyDown(keyToPause))
				{
					gameManager.ChangeGameState(GameManager.GameState.Running);
				}
				break;
		}
	}

	public void UpdateUI()
	{
		switch (GameManager.Instance.CurrentGameState)
		{
			case GameManager.GameState.MainMenu:
				pauseMenu.SetActive(false);
				break;
			case GameManager.GameState.Running:
				UnloadAllUI();
				inGame.SetActive(true);
				break;
			case GameManager.GameState.Paused:
				pauseMenu.SetActive(true);
				break;
		}
	}

	private void UnloadAllUI()
	{
		mainMenu.SetActive(false);
		pauseMenu.SetActive(false);
		optionsMenu.SetActive(false);
		inGame.SetActive(false);
		dummyCamera.enabled = false;
	}

	private void StartButtonClicked()
	{
		GameManager.Instance.LoadLevel("SampleScene");
	}

	private void OptionsButtonClicked()
	{
		TransitFromTo(mainMenu, optionsMenu);
	}

	private void OptionsBackButtonClicked()
	{
		TransitFromTo(optionsMenu, mainMenu);
	}

	private void QuitButtonClicked()
	{
		GameManager.Instance.QuitGame();
	}

	private void OnLoadInGame()
	{
		GameManager.Instance.ChangeGameState(GameManager.GameState.Running);
	}

	private void TransitFromTo(GameObject from, GameObject to)
	{
		from.SetActive(false);
		to.SetActive(true);
	}

}
