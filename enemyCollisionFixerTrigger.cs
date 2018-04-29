using System;
using TheForest.Utils;
using UnityEngine;


public class enemyCollisionFixerTrigger : MonoBehaviour
{
	
	private void Start()
	{
		this.thisCollider = base.transform.GetComponent<CapsuleCollider>();
		if (BoltNetwork.isClient)
		{
			this.thisCollider.radius = 2.3f;
		}
		else
		{
			this.thisCollider.radius = 2.5f;
		}
	}

	
	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("enemyRoot"))
		{
			targetStats component = other.GetComponent<targetStats>();
			if (component && component.targetDown && !component.inNooseTrap)
			{
				return;
			}
			if (!this.netPrefab)
			{
				LocalPlayer.FpCharacter.clampVelocityTowardEnemy(other.transform.position);
			}
		}
	}

	
	public void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("enemyRoot"))
		{
			targetStats component = other.GetComponent<targetStats>();
			if (component && component.targetDown && !component.inNooseTrap)
			{
				return;
			}
			if (!this.netPrefab)
			{
				LocalPlayer.FpCharacter.clampVelocityTowardEnemy(other.transform.position);
				this.atEnemy = true;
			}
		}
	}

	
	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("enemyRoot") && !this.netPrefab)
		{
			LocalPlayer.FpCharacter.setEnemyContact(false);
		}
	}

	
	private void LateUpdate()
	{
		if (this.atEnemy)
		{
			this.exitCheck = true;
		}
		else if (this.exitCheck)
		{
			LocalPlayer.FpCharacter.setEnemyContact(false);
			this.exitCheck = false;
		}
		this.atEnemy = false;
	}

	
	public bool netPrefab;

	
	private CapsuleCollider thisCollider;

	
	private bool atEnemy;

	
	private bool exitCheck;
}
