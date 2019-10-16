using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;


[System.Serializable]
public class GameStateEvent : UnityEvent<GameStates> { }

[System.Serializable]
public enum GameStates
{
	Resumed = 0, Paused = 1
}

public class GameController : MonoBehaviour
{
	#region Singleton
	public static GameController instance;
	#endregion

	public GameStates CurrentState { get; private set; }
	public GameStateEvent onGameStateChanged;

	private void Awake()
	{
		instance = this;
	}

	public void PauseGame()
	{
		Time.timeScale = 0f;
		CurrentState = GameStates.Paused;
		onGameStateChanged.Invoke(CurrentState);
	}

	public void ResumeGame()
	{
		Time.timeScale = 1f;
		CurrentState = GameStates.Resumed;
		onGameStateChanged.Invoke(CurrentState);
	}

	public void QuitGame()
	{
	#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
	#else
      Application.Quit();
	#endif
	}


}
