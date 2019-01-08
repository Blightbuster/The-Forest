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
		bool underWater = playerParticleSpawn.IsUnderWater();
		playerParticleSpawn.UpdateBubbles(underWater);
		playerParticleSpawn.UpdateCausticLight(underWater);
	}

	private static void UpdateCausticLight(bool underWater)
	{
		GameObject gameObject = (!(Scene.RainTypes == null)) ? Scene.RainTypes.CausticLight : null;
		if (gameObject == null)
		{
			return;
		}
		bool flag = !LocalPlayer.IsInCaves && !LocalPlayer.IsInEndgame && underWater;
		if (flag == gameObject.activeSelf)
		{
			return;
		}
		if (!flag)
		{
			gameObject.SetActive(false);
			gameObject.transform.parent = Scene.RainTypes.transform;
			return;
		}
		gameObject.SetActive(true);
		gameObject.transform.parent = null;
		Vector3 position = LocalPlayer.Transform.position;
		position.y += 30f;
		gameObject.transform.position = position;
	}

	private static void UpdateBubbles(bool underWater)
	{
		GameObject gameObject = (!(Scene.RainTypes == null)) ? Scene.RainTypes.bubbles : null;
		if (gameObject != null && gameObject.activeSelf != underWater)
		{
			gameObject.SetActive(underWater);
		}
	}

	private static bool IsUnderWater()
	{
		return LocalPlayer.WaterViz != null && LocalPlayer.Stats != null && LocalPlayer.WaterViz.ScreenCoverage > LocalPlayer.Stats.AirBreathing.ScreenCoverageThreshold + 0.25f;
	}

	public GameObject snowPlantDust;

	private animEventsManager events;

	private bool doingBubbles;

	public bool net;
}
