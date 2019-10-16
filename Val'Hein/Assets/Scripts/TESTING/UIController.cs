using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable CS0649
public class UIController : MonoBehaviour
{
	[SerializeField]
	private KeyCode keyToPauseGame = KeyCode.Escape;
	[SerializeField]
	private Canvas pauseCanvas;

	private void Start()
	{
		
	}

	// Update is called once per frame
	private void Update()
    {
		if (Input.GetKeyDown(keyToPauseGame))
		{
			var currentState = GameController.instance.CurrentState;
			if (currentState == GameStates.Paused)
			{
				GameController.instance.ResumeGame();
			}
			else
			{
				GameController.instance.PauseGame();
			}
		}
    }

	public void PauseUISetActive (GameStates state)
	{
		bool active;
		if (state == GameStates.Paused)
			active = true;
		else
			active = false;
		pauseCanvas.gameObject.SetActive(active);
	}

}