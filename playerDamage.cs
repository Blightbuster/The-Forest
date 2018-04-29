using System;
using UnityEngine;


public class playerDamage : MonoBehaviour
{
	
	private void Awake()
	{
		this.hitReactions = base.transform.root.GetComponent<playerHitReactions>();
	}

	
	private void lookAtExplosion(Vector3 pos)
	{
		this.hitReactions.lookAtExplosion(pos);
	}

	
	private playerHitReactions hitReactions;
}
