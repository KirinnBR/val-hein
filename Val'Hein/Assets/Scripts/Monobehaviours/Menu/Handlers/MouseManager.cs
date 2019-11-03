using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
	public bool IsMouseLocked { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
		IsMouseLocked = false;
		ChangeLock();
    }

	public void ChangeLock()
	{
		IsMouseLocked = !IsMouseLocked;
		Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = !Cursor.visible;
	}




}
