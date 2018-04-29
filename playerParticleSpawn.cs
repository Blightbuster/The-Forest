using System;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class playerParticleSpawn : MonoBehaviour
{
	
	private void Start()
	{
		this.events = base.transform.GetComponentInChildren<animEventsManager>();
		if (Scene.RainTypes != null && Scene.RainTypes.bubbles != null)
		{
			this.doingBubbles = false;
			Scene.RainTypes.bubbles.SetActive(false);
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (!this.events)
		{
			return;
		}
		bool flag = this.events.IsOnSnow();
		if (other.gameObject.CompareTag("SmallTree") && flag && other.transform.GetComponent<BushDamage>() && PoolManager.Pools.ContainsKey("Particles"))
		{
			PoolManager.Pools["Particles"].Spawn(this.snowPlantDust.transform, other.transform.position, Quaternion.identity);
		}
	}

	
	private void Update()
	{
		if (this.net)
		{
			return;
		}
		if (LocalPlayer.WaterViz.ScreenCoverage > LocalPlayer.Stats.AirBreathing.ScreenCoverageThreshold + 0.25f)
		{
			if (!this.doingBubbles)
			{
				Scene.RainTypes.bubbles.SetActive(true);
				this.doingBubbles = true;
			}
		}
		else if (this.doingBubbles)
		{
			Scene.RainTypes.bubbles.SetActive(false);
			this.doingBubbles = false;
		}
	}

	
	public GameObject snowPlantDust;

	
	private animEventsManager events;

	
	private bool doingBubbles;

	
	public bool net;
}
