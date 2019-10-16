using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{

	[SerializeField] private MainMenu mainMenu;
	[SerializeField] private Camera dummyCamera;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			mainMenu.FadeOut();
		}
	}


	public void SetCameraActive( bool active )
	{
		dummyCamera.gameObject.SetActive(active);
	}

}
