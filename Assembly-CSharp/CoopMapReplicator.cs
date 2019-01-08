using System;
using Bolt;
using UnityEngine;

public class CoopMapReplicator : EntityBehaviour<IPlayerState>
{
	private void Start()
	{
		UnityEngine.Object.Destroy(this);
	}
}
