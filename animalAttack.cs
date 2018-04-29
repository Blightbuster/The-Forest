using System;
using UnityEngine;


public class animalAttack : MonoBehaviour
{
	
	private void Start()
	{
		this.rootTr = base.transform.root;
	}

	
	private void OnEnable()
	{
		this.cooldown = false;
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("playerHitDetect") && !this.cooldown)
		{
			other.SendMessageUpwards("getHitDirection", this.rootTr.position, SendMessageOptions.DontRequireReceiver);
			other.SendMessageUpwards("hitFromEnemy", this.attackDamage, SendMessageOptions.DontRequireReceiver);
			this.cooldown = true;
			base.Invoke("resetCooldown", 1f);
		}
	}

	
	private void resetCooldown()
	{
		this.cooldown = false;
	}

	
	public int attackDamage = 5;

	
	private bool cooldown;

	
	private Transform rootTr;

	
	public bool crocodile;
}
