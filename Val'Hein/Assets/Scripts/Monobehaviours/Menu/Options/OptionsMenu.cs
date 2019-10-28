﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0649
public class OptionsMenu : MonoBehaviour
{
	[SerializeField] private Button backButton;

    // Start is called before the first frame update
    void Start()
    {
		backButton.onClick.AddListener(GoBack);
    }

    public void GoBack()
	{
		UIManager.Instance.UnloadOptionsMenu();
		UIManager.Instance.LoadMainMenu();
	}
}