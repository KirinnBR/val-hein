using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class MainMenu : MonoBehaviour
{
	[SerializeField] private AnimationClip fadeInAnimation;
	[SerializeField] private AnimationClip fadeOutAnimation;
	
	private Animation mainMenuAnimator;

	private void Start()
	{
		mainMenuAnimator = GetComponent<Animation>();
		FadeIn();
	}

	public void OnFadeOutComplete()
	{
		UIManager.Instance.SetCameraActive(false);
	}

	public void OnFadeInComplete()
	{
		UIManager.Instance.SetCameraActive(true);
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
