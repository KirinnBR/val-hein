using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
	#region Input Settings

	[Header("Input Settings")]

    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;
    [SerializeField]
    private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField]
    private bool invertRun = true;
    [SerializeField]
    private KeyCode dodgeKey = KeyCode.LeftControl;
    [SerializeField]
    private KeyCode targetKey = KeyCode.F;
    [SerializeField]
    private MouseButtonCode attackButton = MouseButtonCode.LeftButton;

	#endregion

	#region References

	public bool Jump => Input.GetKeyDown(jumpKey);
    public bool Run
    { 
        get
        {
            var run = Input.GetKey(runKey);
            if (invertRun)
                run = !run;
            return run;
        } 
    }
    public bool Dodge => Input.GetKeyDown(dodgeKey);
    public bool Target => Input.GetKeyDown(targetKey);
    public bool Attack => Input.GetMouseButtonDown((int)attackButton);
    public float Horizontal => Input.GetAxisRaw("Horizontal");
    public float Vertical => Input.GetAxisRaw("Vertical");
    public float MouseScrollWheel => -Input.GetAxisRaw("Mouse ScrollWheel");

	#endregion
}
