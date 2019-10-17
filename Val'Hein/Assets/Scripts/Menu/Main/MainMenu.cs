﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649
public class MainMenu : MonoBehaviour
{
	[SerializeField] private Button startButton;
	[SerializeField] private Button optButton;
	[SerializeField] private Button quitButton;

	private void Awake()
	{
		startButton.onClick.AddListener(StartButtonClicked);
		optButton.onClick.AddListener(OptionsButtonClicked);
		quitButton.onClick.AddListener(QuitButtonClicked);
	}

	public void StartButtonClicked()
	{
		GameManager.Instance.LoadLevel("SampleScene");
		GameManager.Instance.ChangeGameState(GameManager.GameState.Running);
	}

	public void OptionsButtonClicked()
	{
		UIManager.Instance.LoadOptionsMenu();
		UIManager.Instance.UnloadMainMenu();
	}

	public void QuitButtonClicked()
	{
		GameManager.Instance.QuitGame();
	}

}
