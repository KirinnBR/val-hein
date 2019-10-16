using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
[RequireComponent(typeof(Animation))]
public class MainMenu : MonoBehaviour
{
	[SerializeField] private AnimationClip fadeInAnimation;
	[SerializeField] private AnimationClip fadeOutAnimation;
	
	private Animation mainMenuAnimator;

	private void Awake()
	{
		mainMenuAnimator = GetComponent<Animation>();
	}

	public void OnFadeOutComplete()
	{
		GameManager.Instance.LoadLevel("SampleScene");
		GameManager.Instance.ChangeGameState(GameManager.GameState.Running);
	}

	public void FadeIn()
	{
		mainMenuAnimator.Stop();
		mainMenuAnimator.clip = fadeInAnimation;
		mainMenuAnimator.Play();
	}

	public void FadeOut()
	{
		mainMenuAnimator.Stop();
		mainMenuAnimator.clip = fadeOutAnimation;
		mainMenuAnimator.Play();
	}

}
