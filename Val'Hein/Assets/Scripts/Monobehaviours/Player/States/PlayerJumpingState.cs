using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpingState : BaseState<PlayerControllerSystem>
{

	public override void EnterState(PlayerControllerSystem player)
	{
		player.Jump();
	}

	public override void Update(PlayerControllerSystem player)
	{
		player.UpdateJump();
	}


}
