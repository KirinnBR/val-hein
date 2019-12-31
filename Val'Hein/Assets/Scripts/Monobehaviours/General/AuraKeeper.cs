using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraKeeper : MonoBehaviour
{
	public Renderer rend;
	public AuraType aura;

    // Start is called before the first frame update
    void Start()
    {
		rend.material = aura.auraMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
