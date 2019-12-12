using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
public class HitMarker : MonoBehaviour
{
	public float radius = 0.05f;
	public LayerMask hitLayer;
	public string ownerTag = "Entity";
	public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
	
	public bool TryGetDamageable(out IDamageable dmg)
	{
		List<Collider> targets = new List<Collider>(Physics.OverlapSphere(transform.position, radius, hitLayer, triggerInteraction));
		targets.RemoveAll(x => x.CompareTag(ownerTag));
		foreach (var target in targets)
		{
			if (target.TryGetComponent(out dmg))
			{
				Debug.Log($"{name} hit {target.name}");
				return true;
			}
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
