using System;
using Bolt;
using UnityEngine;

public class BoltHeldItem : EntityBehaviour
{
	private void OnEnable()
	{
		if (!BoltNetwork.isRunning || base.entity.isOwner)
		{
		}
	}

	private void OnDisable()
	{
		if (!BoltNetwork.isRunning || base.entity.isOwner)
		{
		}
	}

	[SerializeField]
	public int Id;
}
