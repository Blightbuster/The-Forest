using System;
using PathologicalGames;
using UnityEngine;


public class TurnOffPool : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("TurnOff", (float)this.Wait);
	}

	
	private void TurnOff()
	{
		PoolManager.Pools["Bushes"].Despawn(base.transform);
	}

	
	public int Wait;
}
