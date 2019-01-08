using System;
using PathologicalGames;
using UnityEngine;

public class TurnOffPoolPS : MonoBehaviour
{
	private void Start()
	{
		base.Invoke("TurnOff", (float)this.Wait);
	}

	private void OnEnable()
	{
		if (this.useEnable)
		{
			base.Invoke("TurnOff", (float)this.Wait);
		}
	}

	private void TurnOff()
	{
		if (PoolManager.Pools == null)
		{
			return;
		}
		if (this.isGreeble)
		{
			PoolManager.Pools["Greebles"].Despawn(base.transform);
		}
		else
		{
			PoolManager.Pools["Particles"].Despawn(base.transform);
		}
	}

	public int Wait;

	public bool useEnable;

	public bool isGreeble;
}
