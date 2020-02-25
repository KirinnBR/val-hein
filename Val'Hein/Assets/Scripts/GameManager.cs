using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;



public class GameManager : Singleton<GameManager>
{

	[System.Serializable]
	public enum GameState
	{
		MainMenu = 0, Running = 1, Paused = 2
	}

	public GameObject[] SystemPrefabs;
	public GameState CurrentGameState { get; private set; }
	public UnityEvent onSceneLoadedComplete;

	private List<GameObject> instancedSystemPrefabs;
	private List<AsyncOperation> loadOperations;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		loadOperations = new List<AsyncOperation>();
		instancedSystemPrefabs = new List<GameObject>();
		
		InstantiateSystemPrefabs();
	}

	public void OnLoadOperationComplete(AsyncOperation ao)
	{
		if (loadOperations.Contains(ao))
		{
			loadOperations.Remove(ao);

			//Dispatch messages.
			//Transition between scenes.
		}
		Debug.Log(SceneManager.GetActiveScene().name + " loaded.");
		onSceneLoadedComplete.Invoke();
	}

	public void OnUnloadOperationComplete(AsyncOperation ao)
	{
		Debug.Log("Unloaded");
	}

	public void LoadLevel(string levelName, LoadSceneMode loadSceneMode)
	{
		var ao = SceneManager.LoadSceneAsync(levelName, loadSceneMode);

		if (ao == null)
		{
			Debug.LogError("[GameManager] unable to load level.");
			return;
		}
		ao.completed += OnLoadOperationComplete;
		loadOperations.Add(ao);
	}

	public void UnloadLevel(string levelName)
	{
		var ao = SceneManager.UnloadSceneAsync(levelName);

		if (ao == null)
		{
			Debug.LogError("[GameManager] unable to unload level.");
			return;
		}

		ao.completed += OnUnloadOperationComplete;
	}

	public void PauseGame()
	{
		if (CurrentGameState == GameState.Paused) return;

		MouseManager.Instance.SetLock(false);
		Time.timeScale = 0f;
		CurrentGameState = GameState.Paused;
	}

	public void ResumeGame()
	{
		if (CurrentGameState == GameState.Running) return;

		MouseManager.Instance.SetLock(true);
		Time.timeScale = 1f;
		CurrentGameState = GameState.Running;
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	private void InstantiateSystemPrefabs()
	{
		GameObject prefabInstance;
		foreach (var prefab in SystemPrefabs)
		{
			prefabInstance = Instantiate(prefab);
			instancedSystemPrefabs.Add(prefabInstance);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		foreach (var prefab in instancedSystemPrefabs)
		{
			Destroy(prefab);
		}
		instancedSystemPrefabs.Clear();
	}

}
