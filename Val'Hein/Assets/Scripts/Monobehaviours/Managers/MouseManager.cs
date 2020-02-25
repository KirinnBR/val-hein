using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : Singleton<MouseManager>
{
	public bool IsMouseLocked { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
		SetLock(false);
    }

	public void SetLock(bool locked)
	{
		Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !locked;
		IsMouseLocked = locked;
	}




}
