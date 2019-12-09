using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
public class HitMarker : MonoBehaviour
{
	public float radius = 0.05f;
	public LayerMask enemiesLayer;
	public string ownerTag = "Entity";
	public QueryTriggerInteraction triggerCollision = QueryTriggerInteraction.Ignore;
	
	public bool TryGetDamageable(out IDamageable dmg)
	{
		var targets = Physics.OverlapSphere(transform.position, radius, enemiesLayer, triggerCollision);
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
		Gizmos.DrawSphere(transform.position, radius);
	}

}
