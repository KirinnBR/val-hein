using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
public class PlayerClass : MonoBehaviour
{

	public int Level { get { return Player.Instance.playerLevel.level; } }

    // Start is called before the first frame update
    void Start()
    {

    }

	// Update is called once per frame
	void Update()
	{
		if (GameManager.IsInitialized && GameManager.Instance.CurrentGameState != GameManager.GameState.Running) return;

	}

	public override string ToString()
	{
		return $"Level: {Level}";
	}
}
