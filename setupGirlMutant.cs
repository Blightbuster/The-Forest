using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class setupGirlMutant : MonoBehaviour
{
	
	private void Start()
	{
		this.updateTimer = Time.time + 4f;
	}

	
	private void Update()
	{
		if (Scene.SceneTracker.endBossSpawned)
		{
			UnityEngine.Object.Destroy(this.placedPrefab);
			base.enabled = false;
			return;
		}
		if (Time.time > this.updateTimer)
		{
			this.updateTimer = Time.time + 1f;
			this.closestPlayerDist = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position);
			if (this.closestPlayerDist < 350f)
			{
				this.spawnRealGirl();
			}
		}
	}

	
	private void spawnRealGirl()
	{
		if (BoltNetwork.isRunning)
		{
			setupEndBoss setupEndBoss = setupEndBoss.Create(GlobalTargets.Everyone);
			setupEndBoss.spawnBoss = true;
			setupEndBoss.pos = this.placedPrefab.transform.position;
			setupEndBoss.rot = this.placedPrefab.transform.rotation;
			setupEndBoss.Send();
			UnityEngine.Object.Destroy(this.placedPrefab);
			base.enabled = false;
			return;
		}
		if (!Scene.SceneTracker.endBossSpawned)
		{
			this.updateTimer = Time.time + 1f;
			GameObject gameObject = UnityEngine.Object.Instantiate(this.realPrefab, this.placedPrefab.transform.position, this.placedPrefab.transform.rotation) as GameObject;
			UnityEngine.Object.Destroy(this.placedPrefab);
			this.activateGirlScript.girlAnimator = gameObject.GetComponentInChildren<Animator>();
			base.enabled = false;
		}
	}

	
	public GameObject placedPrefab;

	
	public GameObject realPrefab;

	
	public activateGirlTransform activateGirlScript;

	
	private float updateTimer = 4f;

	
	public float closestPlayerDist;
}
