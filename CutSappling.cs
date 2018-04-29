using System;
using PathologicalGames;
using UnityEngine;


public class CutSappling : MonoBehaviour
{
	
	private void Awake()
	{
		this.startHealth = this.Health;
	}

	
	private void OnEnable()
	{
		this.Health = this.startHealth;
	}

	
	public void SetLodBase(LOD_Base lb)
	{
		this.LodBase = lb;
	}

	
	private void Hit()
	{
		if (this.Health > 0)
		{
			this.Health--;
			if (this.Health <= 0)
			{
				this.CutDown();
			}
		}
	}

	
	private void Explosion(float dist)
	{
		if (this.Health > 0)
		{
			this.Health = 0;
			this.CutDown();
		}
	}

	
	private void CutDown()
	{
		if (FMOD_StudioSystem.instance && this.breakEvent != null)
		{
			FMOD_StudioSystem.instance.PlayOneShot(this.breakEvent, base.transform.position, null);
		}
		if (this.LodBase)
		{
			this.LodBase.CurrentLodTransform = null;
			this.LodBase = null;
		}
		if (PoolManager.Pools["Bushes"].IsSpawned(base.transform))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.Sticks);
			gameObject.SetActive(true);
			gameObject.transform.parent = null;
			PoolManager.Pools["Bushes"].Despawn(base.transform);
		}
		else
		{
			this.Sticks.SetActive(true);
			this.Sticks.transform.parent = null;
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	public GameObject Sticks;

	
	private int Health = 2;

	
	[Header("FMOD")]
	private string breakEvent;

	
	private int startHealth;

	
	private LOD_Base LodBase;
}
