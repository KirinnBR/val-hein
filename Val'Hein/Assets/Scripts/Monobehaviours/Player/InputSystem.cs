using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{

    #region References

    public bool Jump => Input.GetAxis("Jump") > 0;
    public bool Run => Input.GetAxis("Run") > 0;
    public bool Dodge => Input.GetAxis("Dodge") > 0;
    public bool Target => Input.GetAxis("Target") > 0;
    public bool Attack => Input.GetAxis("Fire1") > 0;
    public bool Pause => Input.GetAxis("Pause") > 0;
    public float Horizontal => Input.GetAxisRaw("Horizontal");
    public float Vertical => Input.GetAxisRaw("Vertical");
    public float MouseScrollWheel => -Input.GetAxisRaw("Mouse ScrollWheel");
    //public bool OpenInventory => Input.GetAxis("Inventory") > 0;
    
    public bool Interact => Input.GetAxis("Interact") > 0;

    #endregion

    private void Update()
    {
        /*if (Pause)
        {
            GameManager.Instance.PauseGame();
        }

        if (GetKeyDown("Inventory"))
        {
            ui.ToggleInventory();
        }*/
    }

}
