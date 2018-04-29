using System;
using Bolt;
using UnityEngine;


public class sharkHitReceiver : MonoBehaviour
{
	
	private void Start()
	{
		this.fishScript = base.transform.root.GetComponent<Fish>();
	}

	
	private void Hit(int d)
	{
		if (this.fishScript)
		{
			this.fishScript.Hit(d);
		}
		if (BoltNetwork.isClient)
		{
			PlayerHitEnemy playerHitEnemy = PlayerHitEnemy.Create(GlobalTargets.OnlyServer);
			playerHitEnemy.Target = base.transform.GetComponentInParent<BoltEntity>();
			playerHitEnemy.Hit = d;
		}
	}

	
	private Fish fishScript;
}
