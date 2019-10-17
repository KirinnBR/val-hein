using UnityEngine;
using UnityEngine.UI;


public class PauseMenu : MonoBehaviour
{
	[SerializeField] private Button resumeButton;

	private void Start()
	{
		resumeButton.onClick.AddListener(ResumeGame);
	}

	public void ResumeGame()
	{
		GameManager.Instance.ResumeGame();
	}

}