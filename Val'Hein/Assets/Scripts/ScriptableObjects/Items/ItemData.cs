using UnityEngine;


public abstract class ItemData : ScriptableObject
{
    protected PlayerCenterControl player => PlayerCenterControl.Instance;

    [Header("Basic Item Settings")]

    public string name = "Item";
    public Sprite icon;
    [Range(0.001f, 100f)]
    public float dropRate = 100f;
    public int sellValue = 100;
    public float weight = 1f;

    public abstract bool Use();
    public abstract bool Sell();

}
