using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    [Range(0.0001f, 360f)]
    private float angle = 90f;


    private Quaternion lookA, lookB;
    private float startAngle;
    private bool rewinding = false;
    // Start is called before the first frame update
    void Awake()
    {
        startAngle = transform.eulerAngles.y;
        lookA = Quaternion.Euler(0f, transform.eulerAngles.y + angle / 2, 0f);
        lookB = Quaternion.Euler(0f, transform.eulerAngles.y - angle / 2, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (angle > 180)
        {

        }
        else
        {
            if (rewinding)
            {
                if (Quaternion.Angle(transform.rotation, lookB) <= 1f)
                {
                    rewinding = false;
                    transform.rotation = lookB;
                    return;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, lookB, speed * Time.deltaTime);
            }
            else
            {
                if (Quaternion.Angle(transform.rotation, lookA) <= 1f)
                {
                    transform.rotation = lookA;
                    rewinding = true;
                    return;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, lookA, speed * Time.deltaTime);
            }
        }
        
    }

    private IEnumerator AnimateAroundAxis(Transform trans, Vector3 axis, float changeInAngle, float duration)
    {
        var start = trans.rotation;
        float t = 0f;
        while (t < duration)
        {
            trans.rotation = start * Quaternion.AngleAxis(changeInAngle * t / duration, axis);
            yield return null;
            t += Time.deltaTime;
        }
        trans.rotation = start * Quaternion.AngleAxis(changeInAngle, axis);
    }

    private void OnDrawGizmosSelected()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            float _angle = -startAngle / 2 - angle / 2;
            Vector3 dir = new Vector3(Mathf.Sin(_angle * Mathf.Deg2Rad), 0f, Mathf.Cos(_angle * Mathf.Deg2Rad));
            UnityEditor.Handles.color = Color.green - new Color(0f, 0f, 0f, 0.8f);
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, dir, angle, 1f);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawLine(transform.position, transform.position + transform.forward * 1f);
        }
        else
        {
            float _angle = -angle/2 + transform.eulerAngles.y;
            Vector3 dir = new Vector3(Mathf.Sin(_angle * Mathf.Deg2Rad), 0f, Mathf.Cos(_angle * Mathf.Deg2Rad));
            UnityEditor.Handles.color = Color.green - new Color(0f, 0f, 0f, 0.8f);
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, dir, angle, 1f);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawLine(transform.position, transform.position + transform.forward * 1f);
        }
    }
}
