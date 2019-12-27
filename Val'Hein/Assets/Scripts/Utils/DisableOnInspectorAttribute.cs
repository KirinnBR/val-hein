using UnityEngine;

public class DisableOnInspectorAttribute : PropertyAttribute
{
	public bool Enable { get; private set; }
	public DisableOnInspectorAttribute() { Enable = true; }
}
