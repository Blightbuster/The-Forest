using System;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;

public class Mud : MonoBehaviour
{
	private void Awake()
	{
		base.enabled = false;
	}

	private void GrabEnter()
	{
		base.enabled = true;
		this.Sheen.SetActive(false);
		this.PickUp.SetActive(true);
	}

	private void GrabExit()
	{
		base.enabled = false;
		this.Sheen.SetActive(true);
		this.PickUp.SetActive(false);
	}

	private void Update()
	{
		if (TheForest.Utils.Input.GetButtonDown("Take"))
		{
			LocalPlayer.GameObject.SendMessage(this.Message);
			SpawnPool spawnPool;
			if (PoolManager.Pools.TryGetValue("Greebles", out spawnPool) && spawnPool.IsSpawned(base.transform.parent))
			{
				this.GrabExit();
				spawnPool.Despawn(base.transform.parent);
			}
			else
			{
				UnityEngine.Object.Destroy(base.transform.parent.gameObject);
			}
		}
	}

	public GameObject Sheen;

	public GameObject PickUp;

	public string Message = "GotMud";
}
