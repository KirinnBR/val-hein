using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionMarker : MonoBehaviour
{
	public float markerRadius = 0.05f;
	public LayerMask enemiesLayer;
	public string ownerTag;

	public bool TryGetDamageable(out IDamageable dmg)
	{
		var targets = Physics.OverlapSphere(transform.position, markerRadius, enemiesLayer, QueryTriggerInteraction.Collide);
		foreach (var target in targets)
		{
			if (target.TryGetComponent(out dmg) && target.tag != ownerTag)
			{
				Debug.Log("Target found: " + target.name);
				return true;
			}
		}
		dmg = null;
		Debug.Log("Target not found.");
		return false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red - new Color(0, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, markerRadius);
	}


}
