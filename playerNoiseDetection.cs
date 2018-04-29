using System;
using System.Collections;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class playerNoiseDetection : MonoBehaviour
{
	
	private void Awake()
	{
		if (this.soundDetect)
		{
			this.soundCollider = this.soundDetect.GetComponent<SphereCollider>();
		}
		this.initRange = 0f;
	}

	
	private IEnumerator Start()
	{
		this.tr = base.transform;
		this.pulseInterval = Time.time + 3f;
		this.soundRange = this.initRange;
		this.soundCollider.enabled = false;
		base.enabled = false;
		while (!PoolManager.Pools.ContainsKey("creatures"))
		{
			yield return null;
		}
		base.enabled = true;
		yield break;
	}

	
	private IEnumerator setNoiseRange(float range)
	{
		if (LocalPlayer.IsInCaves)
		{
			if (LocalPlayer.FpCharacter.run)
			{
				this.soundRange = range * 2.2f * LocalPlayer.Stats.SoundRangeDampFactor;
			}
			else
			{
				this.soundRange = range * 1.65f * LocalPlayer.Stats.SoundRangeDampFactor;
			}
		}
		else
		{
			this.soundRange = range * 1.4f * LocalPlayer.Stats.SoundRangeDampFactor;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator setWeaponNoiseRange(float range)
	{
		if (LocalPlayer.IsInCaves)
		{
			if (LocalPlayer.FpCharacter.run)
			{
				this.soundRange = range * 2.2f * LocalPlayer.Stats.SoundRangeDampFactor;
			}
			else
			{
				this.soundRange = range * 1.65f * LocalPlayer.Stats.SoundRangeDampFactor;
			}
		}
		else
		{
			this.soundRange = range * 1.4f * LocalPlayer.Stats.SoundRangeDampFactor;
		}
		yield return null;
		this.sendSoundAlertToAnimals();
		yield break;
	}

	
	private IEnumerator resetNoiseRange()
	{
		this.soundRange = 0f;
		yield return null;
		yield break;
	}

	
	private void Update()
	{
		if (Time.time > this.pulseInterval)
		{
			if (this.soundRange > 1f)
			{
				this.sendSoundAlertToEntities();
			}
			this.pulseInterval = Time.time + 0.5f;
		}
	}

	
	public void sendSoundAlertToAnimals()
	{
		if (!Scene.MutantControler.gameObject.activeSelf)
		{
			return;
		}
		this.sqrtSoundRange = this.soundRange * this.soundRange;
		if (this.soundRange < 3f)
		{
			this.sqrtSoundRange = 625f;
		}
		if (Scene.SceneTracker.birdController != null)
		{
			for (int i = 0; i < Scene.SceneTracker.birdController.myBirds.Length; i++)
			{
				if (Scene.SceneTracker.birdController.myBirds[i] != null && Scene.SceneTracker.birdController.myBirds[i].activeSelf && (Scene.SceneTracker.birdController.myBirds[i].transform.position - this.tr.position).sqrMagnitude < this.sqrtSoundRange)
				{
					Scene.SceneTracker.birdController.myBirds[i].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (PoolManager.Pools == null)
		{
			return;
		}
		for (int j = 0; j < PoolManager.Pools["creatures"].Count; j++)
		{
			Transform transform = PoolManager.Pools["creatures"][j];
			if (transform != null && transform.gameObject.activeSelf && (transform.position - this.tr.position).sqrMagnitude < this.sqrtSoundRange)
			{
				transform.SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private void sendSoundAlertToEntities()
	{
		if (!Scene.MutantControler.gameObject.activeSelf)
		{
			return;
		}
		this.sqrtSoundRange = this.soundRange * this.soundRange;
		if (Scene.SceneTracker.birdController != null)
		{
			for (int i = 0; i < Scene.SceneTracker.birdController.myBirds.Length; i++)
			{
				if (Scene.SceneTracker.birdController.myBirds[i] != null && Scene.SceneTracker.birdController.myBirds[i].activeSelf && (Scene.SceneTracker.birdController.myBirds[i].transform.position - this.tr.position).sqrMagnitude < this.sqrtSoundRange)
				{
					Scene.SceneTracker.birdController.myBirds[i].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (BoltNetwork.isClient)
		{
			return;
		}
		for (int j = 0; j < Scene.MutantControler.activeCannibals.Count; j++)
		{
			if (Scene.MutantControler.activeCannibals[j] != null && (Scene.MutantControler.activeCannibals[j].transform.position - this.tr.position).sqrMagnitude < this.sqrtSoundRange)
			{
				Scene.MutantControler.activeCannibals[j].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
		for (int k = 0; k < Scene.MutantControler.activeInstantSpawnedCannibals.Count; k++)
		{
			if (Scene.MutantControler.activeInstantSpawnedCannibals[k] != null && (Scene.MutantControler.activeInstantSpawnedCannibals[k].transform.position - this.tr.position).sqrMagnitude < this.sqrtSoundRange)
			{
				Scene.MutantControler.activeInstantSpawnedCannibals[k].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
		for (int l = 0; l < Scene.MutantControler.activeBabies.Count; l++)
		{
			if (Scene.MutantControler.activeBabies[l] != null && (Scene.MutantControler.activeBabies[l].transform.position - this.tr.position).sqrMagnitude < this.sqrtSoundRange)
			{
				Scene.MutantControler.activeBabies[l].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (PoolManager.Pools == null)
		{
			return;
		}
		for (int m = 0; m < PoolManager.Pools["creatures"].Count; m++)
		{
			Transform transform = PoolManager.Pools["creatures"][m];
			if (transform != null && transform.gameObject.activeSelf && (transform.position - this.tr.position).sqrMagnitude < this.sqrtSoundRange)
			{
				transform.SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private TargetTracker tracker;

	
	public GameObject soundDetect;

	
	private SphereCollider soundCollider;

	
	private Transform tr;

	
	private float pulseInterval;

	
	private float initRange;

	
	public float soundRange;

	
	private float sqrtSoundRange;
}
