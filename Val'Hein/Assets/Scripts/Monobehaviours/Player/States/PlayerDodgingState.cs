using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodgingState : BaseState<PlayerControllerSystem>
{
	private float timeDodging;
	private Vector3 dir;

	public override void EnterState(PlayerControllerSystem player)
    {
		timeDodging = 0f;
		dir = player.Dodge();
	}

    public override void Update(PlayerControllerSystem player)
    {
		player.UpdateDodge(dir, ref timeDodging);
    }

	/*private IEnumerator OnDodge()
	{
		combat.CanAttack = false;
		MovementBlocked = true;
		RotationBlocked = true;
		IsDodging = true;
		float timeDodging = dodgeTime;
		Vector3 dir = motionHorizontal.normalized;
		
		Vector3 destination = dir * dodgeForce;
		anim.SetTrigger("Dodge");
		while (timeDodging >= 0)
		{
			Quaternion rot = Quaternion.LookRotation(motionHorizontal == Vector3.zero ? transform.forward : motionHorizontal);
			if (!combat.HasTarget)
				transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
			timeDodging -= Time.deltaTime;
			controller.SimpleMove(destination);
			yield return null;
		}
		combat.CanAttack = true;
		MovementBlocked = false;
		RotationBlocked = false;
		IsDodging = false;
	}*/

}
