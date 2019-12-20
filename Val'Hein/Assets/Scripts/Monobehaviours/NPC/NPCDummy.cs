using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class NPCDummy : NPC
{

	private Vector3 inicialPosition;
	private Quaternion inicialRotation;

	protected override void Start()
	{
		base.Start();
		agent.stoppingDistance = 2f;
		inicialPosition = transform.position;
		inicialRotation = transform.rotation;
	}

	// Update is called once per frame
	private void Update()
    {
		ApplyWalkAnimation();
		if (visibleObjects.Count > 0)
		{
			foreach (var visibleObject in visibleObjects)
			{
				//Condicao para ver se o inimigo se interessa em atacar aquele objeto.
				//Atualmente, ele procura o primeiro objeto que seja um player e ataca.
				if (visibleObject.tag == "Player")
				{
					StartPursuit(visibleObject.transform);
				}
			}
		}
		else
		{
			agent.destination = inicialPosition;
			if (IsCloseEnoughToTarget(agent.destination))
			{
				transform.rotation = inicialRotation;
			}
		}
	}

	private void ApplyWalkAnimation()
	{
		anim.SetFloat("Speed", agent.velocity.magnitude);
	}

	private void StartPursuit(Transform target)
	{
		transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
		agent.speed = battleSpeed;
		agent.SetDestination(target.position);
		if (IsCloseEnoughToTarget(target.position))
		{
			//Can attack.
		}
	}

	

	

	

}
