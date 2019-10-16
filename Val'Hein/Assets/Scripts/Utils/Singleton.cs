using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{

	private static T instance;
	public static T Instance { get { return instance; } }
	public static bool IsInitialized => instance != null;

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
