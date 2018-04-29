using System;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class playerRemoteSoundDetect : MonoBehaviour
{
	
	private void Start()
	{
		this.tr = base.transform.parent;
		this.info = base.transform.parent.GetComponent<playerAiInfo>();
		this.prevPos = this.tr.position;
		this.updateInterval = Time.time + 3f;
		this.soundCollider.enabled = false;
	}

	
	private void FixedUpdate()
	{
		this.overallSpeed = (this.tr.position - this.prevPos).magnitude / Time.deltaTime;
		this.prevPos = this.tr.position;
		this.overallSpeed /= this.maxSpeed;
		if (this.overallSpeed > 1.5f)
		{
			base.Invoke("setRunSoundRange", 0.4f);
		}
		else if (this.overallSpeed > 0.9f)
		{
			base.Invoke("setWalkSoundRange", 0.4f);
		}
		else
		{
			this.soundRange = 0f;
		}
		if (Time.time > this.updateInterval)
		{
			if (this.soundRange > 1f)
			{
				this.sendSoundAlertToEntities();
			}
			this.updateInterval = Time.time + 1f;
		}
	}

	
	private void setRunSoundRange()
	{
		if (this.overallSpeed > 1.5f)
		{
			if (this.info.netInCave)
			{
				this.soundRange = 92.4f;
			}
			else
			{
				this.soundRange = 42f;
			}
		}
	}

	
	private void setWalkSoundRange()
	{
		if (this.overallSpeed > 0.9f && this.overallSpeed < 1.5f)
		{
			if (this.info.netInCave)
			{
				this.soundRange = 33.6000023f;
			}
			else
			{
				this.soundRange = 21f;
			}
		}
	}

	
	private void setIdleSoundRange()
	{
		this.soundRange = 0f;
	}

	
	private void sendSoundAlertToEntities()
	{
		if (Scene.SceneTracker.birdController != null)
		{
			for (int i = 0; i < Scene.SceneTracker.birdController.myBirds.Length; i++)
			{
				if (Scene.SceneTracker.birdController.myBirds[i] != null && Scene.SceneTracker.birdController.myBirds[i].activeSelf && (Scene.SceneTracker.birdController.myBirds[i].transform.position - this.tr.position).sqrMagnitude < this.soundRange * this.soundRange)
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
			if (Scene.MutantControler.activeCannibals[j] != null && (Scene.MutantControler.activeCannibals[j].transform.position - this.tr.position).sqrMagnitude < this.soundRange * this.soundRange)
			{
				Scene.MutantControler.activeCannibals[j].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
		for (int k = 0; k < Scene.MutantControler.activeInstantSpawnedCannibals.Count; k++)
		{
			if (Scene.MutantControler.activeInstantSpawnedCannibals[k] != null && (Scene.MutantControler.activeInstantSpawnedCannibals[k].transform.position - this.tr.position).sqrMagnitude < this.soundRange * this.soundRange)
			{
				Scene.MutantControler.activeInstantSpawnedCannibals[k].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
		for (int l = 0; l < Scene.MutantControler.activeBabies.Count; l++)
		{
			if (Scene.MutantControler.activeBabies[l] != null && (Scene.MutantControler.activeBabies[l].transform.position - this.tr.position).sqrMagnitude < this.soundRange * this.soundRange)
			{
				Scene.MutantControler.activeBabies[l].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
		for (int m = 0; m < PoolManager.Pools["creatures"].Count; m++)
		{
			if (PoolManager.Pools["creatures"][m] != null && PoolManager.Pools["creatures"][m].gameObject.activeSelf && (PoolManager.Pools["creatures"][m].transform.position - this.tr.position).sqrMagnitude < this.soundRange * this.soundRange)
			{
				PoolManager.Pools["creatures"][m].SendMessage("onSoundAlert", this.soundCollider, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private Transform tr;

	
	private playerAiInfo info;

	
	public SphereCollider soundCollider;

	
	public float maxSpeed;

	
	private float overallSpeed;

	
	private Vector3 prevPos;

	
	private float soundRange;

	
	private float updateInterval;
}
