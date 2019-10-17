using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649
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