using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0649
public class UIManager : Singleton<UIManager>
{

	[Header("Options Menu Canvas")]
	[SerializeField]
	private GameObject optionsMenu;
	[SerializeField]
	private Button backButton;

	[Header("Main Menu Canvas")]
	[SerializeField]private GameObject mainMenu;
	[SerializeField]
	private Button startButton;
	[SerializeField]
	private Button optButton;
	[SerializeField]
	private Button quitButton;

	[Header("Pause Menu Canvas")]
	[SerializeField] private GameObject pauseMenu;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		//Options canvas buttons.
		backButton.onClick.AddListener(OptionsBackButtonClicked);

		//Main menu canvas buttons.
		startButton.onClick.AddListener(StartButtonClicked);
		optButton.onClick.AddListener(OptionsButtonClicked);
		quitButton.onClick.AddListener(QuitButtonClicked);

		//Starting main menu environment.
		mainMenu.SetActive(true);
		optionsMenu.SetActive(false);
		pauseMenu.SetActive(false);
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
	}

	private void StartButtonClicked()
	{
		GameManager.Instance.LoadLevel("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
		UnloadAllUI();
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

	private void TransitFromTo(GameObject from, GameObject to)
	{
		from.SetActive(false);
		to.SetActive(true);
	}

}
