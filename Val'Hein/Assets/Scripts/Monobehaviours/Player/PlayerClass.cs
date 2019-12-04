using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClass : MonoBehaviour
{
	[SerializeField]
	private LevelDefinitions playerLevels;

	public int Level { get { return playerLevels.level; } }

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
