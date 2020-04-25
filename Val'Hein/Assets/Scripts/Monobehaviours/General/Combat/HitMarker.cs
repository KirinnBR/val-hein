using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMarker : MonoBehaviour
{
	[Header("Hit Marker Settings")]

	public float radius = 0.05f;

	[SerializeField]
	private bool showHitLines = false;
	public LayerMask hitLayer { get; set; }
	public string ownerTag { get; set; } = "Entity";
	public QueryTriggerInteraction triggerInteraction { get; set; } = QueryTriggerInteraction.Ignore;
#if UNITY_EDITOR
	private List<Vector3> previoustPoints = new List<Vector3>();
#endif

	public bool TryGetDamageable(out IDamageable dmg)
	{
		Collider[] targets = new Collider[2];
		int targetHit = Physics.OverlapSphereNonAlloc(transform.position, radius, targets, hitLayer, triggerInteraction);
		for (int i = 0; i < targetHit; i++)
		{
			var target = targets[i];
			if (target.CompareTag(ownerTag))
				continue;
			if (target.TryGetComponent(out dmg))
				return true;
		}
		dmg = null;
		return false;
	}

	private void Update()
	{
		if (showHitLines)
		{
#if UNITY_EDITOR
			previoustPoints.Add(transform.position);
			StartCoroutine(ClearPoint());
#endif
		}
	}

#if UNITY_EDITOR
	private IEnumerator ClearPoint()
	{
		yield return new WaitForSeconds(0.4f);
		previoustPoints.RemoveAt(0);
	}
#endif

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red - new Color(0, 0, 0, 0.5f);
		Gizmos.DrawSphere(transform.position, radius);

		if (showHitLines)
		{
#if UNITY_EDITOR
			UnityEditor.Handles.color = Color.white;
			UnityEditor.Handles.DrawAAPolyLine(previoustPoints.ToArray());
#endif
		}
	}

}
