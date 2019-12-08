using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
public class CollisionMarker : MonoBehaviour
{
	[SerializeField]
	private float markerRadius = 0.05f;
	[SerializeField]
	private LayerMask enemiesLayer;
	[SerializeField]
	private string ownerTag = "Entity";
	[SerializeField]
	private QueryTriggerInteraction triggerCollision = QueryTriggerInteraction.Ignore;

	public bool TryGetDamageable(out IDamageable dmg)
	{
		var targets = Physics.OverlapSphere(transform.position, markerRadius, enemiesLayer, triggerCollision);
		foreach (var target in targets)
		{
			if (target.TryGetComponent(out dmg) && target.tag != ownerTag)
				return true;
		}
		dmg = null;
		return false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red - new Color(0, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, markerRadius);
	}


}
