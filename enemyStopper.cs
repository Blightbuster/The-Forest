using System;
using UnityEngine;


public class enemyStopper : MonoBehaviour
{
	
	private void Start()
	{
		this.thisCollider = base.transform.GetComponent<Collider>();
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("enemyCollide"))
		{
			mutantHitReceiver component = other.GetComponent<mutantHitReceiver>();
			if (component)
			{
				component.forceEnemyStop();
			}
		}
	}

	
	private void Update()
	{
		if (this.tt)
		{
			if (this.tt.sprung)
			{
				this.thisCollider.enabled = false;
			}
			else
			{
				this.thisCollider.enabled = true;
			}
		}
	}

	
	public trapTrigger tt;

	
	private Collider thisCollider;
}
