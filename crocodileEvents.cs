using System;
using UnityEngine;


public class crocodileEvents : MonoBehaviour
{
	
	private void Start()
	{
		this.weaponCollider.enabled = false;
	}

	
	private void enableAttack()
	{
		this.weaponCollider.enabled = true;
		base.Invoke("disableAttack", 2f);
	}

	
	private void disableAttack()
	{
		this.weaponCollider.enabled = false;
	}

	
	public SphereCollider weaponCollider;
}
