using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
public class HitMarker : MonoBehaviour
{
	[Header("Hit Marker Settings")]
	public float radius = 0.05f;
	public LayerMask hitLayer { get; set; }
	public string ownerTag { get; set; } = "Entity";
	public QueryTriggerInteraction triggerInteraction { get; set; } = QueryTriggerInteraction.Ignore;
#if UNITY_EDITOR
	private List<Vector3> previoustPoints = new List<Vector3>();
#endif

	public bool TryGetDamageable(out IDamageable dmg)
	{
		List<Collider> targets = new List<Collider>(Physics.OverlapSphere(transform.position, radius, hitLayer, triggerInteraction));
		targets.RemoveAll(x => x.CompareTag(ownerTag));
		foreach (var target in targets)
		{
			if (target.TryGetComponent(out dmg))
				return true;
		}
		dmg = null;
		return false;
	}

	private void Update()
	{
#if UNITY_EDITOR
		previoustPoints.Add(transform.position);
		StartCoroutine(ClearPoint());
#endif
	}

#if UNITY_EDITOR
	private IEnumerator ClearPoint()
	{
		yield return new WaitForSeconds(0.3f);
		previoustPoints.RemoveAt(0);
	}
#endif

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red - new Color(0, 0, 0, 0.5f);
		if (!Application.isPlaying)
			Gizmos.DrawSphere(transform.position, radius);

#if UNITY_EDITOR
		UnityEditor.Handles.color = Color.white;
		UnityEditor.Handles.DrawAAPolyLine(previoustPoints.ToArray());
#endif
	}

}
