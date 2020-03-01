using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputSystem : MonoBehaviour
{

    public bool Jump => Input.GetKeyDown(KeyCode.Space) && !isPaused;
    public bool Run => Input.GetKey(KeyCode.LeftShift) && !isPaused;
    public bool Dodge => Input.GetKeyDown(KeyCode.LeftControl) && !isPaused;
    public bool Target => Input.GetKeyDown(KeyCode.F) && !isPaused;
    public bool Attack => Input.GetMouseButtonDown(0) && !isPaused;
    public bool Pause => Input.GetKeyDown(KeyCode.Escape);
    public float Horizontal => Input.GetAxisRaw("Horizontal");
    public float Vertical => Input.GetAxisRaw("Vertical");
    public float MouseScrollWheel => -Input.GetAxisRaw("Mouse ScrollWheel");
    public bool OpenInventory => Input.GetKeyDown(KeyCode.I) && !isPaused;
    public bool Interact => Input.GetKeyDown(KeyCode.E) && !isPaused;

    private bool isPaused = false;

    private void Update()
    {
        if (Pause)
        {
            if (isPaused)
            {
                isPaused = false;
                GameManager.Instance.ResumeGame();
            }
            else
            {
                isPaused = true;
                GameManager.Instance.PauseGame();
            }
            
        }
    }

}
