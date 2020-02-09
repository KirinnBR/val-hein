using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	public static T Instance { get { return instance; } }

	private static T instance;
	public static bool IsInitialized { get { return Instance != null; } }

	protected virtual void Awake()
	{
		if (instance != null)
		{
			Debug.LogError("[Singleton] Trying to instantiate a second instance of a singleton class.");
		}
		else
		{
			instance = this as T;
		}
	}

	protected virtual void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

}
