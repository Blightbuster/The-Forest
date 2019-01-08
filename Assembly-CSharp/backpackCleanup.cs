using System;
using Bolt;
using UnityEngine;

public class backpackCleanup : EntityBehaviour<IPlayerState>
{
	private void Update()
	{
		if (base.transform.position.y < -2000f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
