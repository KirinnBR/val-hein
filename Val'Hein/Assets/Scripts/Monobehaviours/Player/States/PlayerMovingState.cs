using UnityEngine;

public class PlayerMovingState : BaseState<PlayerControllerSystem>
{
	public override void Update(PlayerControllerSystem player)
	{
		player.ProccessInput();
		player.ApplyGravity();
		player.Move();
		player.UpdateAnimations();
	}
}
