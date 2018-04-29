using System;
using Bolt;
using UnityEngine;


public class BoltHeldItem : EntityBehaviour
{
	
	private void OnEnable()
	{
		if (!BoltNetwork.isRunning || this.entity.isOwner)
		{
		}
	}

	
	private void OnDisable()
	{
		if (!BoltNetwork.isRunning || this.entity.isOwner)
		{
		}
	}

	
	[SerializeField]
	public int Id;
}
