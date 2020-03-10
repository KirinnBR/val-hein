using UnityEngine;

public abstract class BaseState<T> where T : MonoBehaviour
{
    public abstract void Update(T owner);
    public virtual void EnterState(T owner) { }
}
