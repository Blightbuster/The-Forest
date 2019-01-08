using System;
using System.Collections.Generic;
using Bolt;
using TheForest.Utils;
using UnityEngine;

public class CoopAnimalFX : EntityBehaviour<IAnimalState>
{
	private void Awake()
	{
		this.ast = base.transform.GetComponentInChildren<arrowStickToTarget>();
		this.ca = base.transform.GetComponent<CoopAnimal>();
	}

	public override void Attached()
	{
		if (this.FX_Fire)
		{
			this.FX_Fire.SetActive(false);
		}
		if (this.FX_PickupTrigger != null && this.FX_PickupTrigger.activeSelf)
		{
			this.FX_PickupTrigger.SendMessage("OnSpawned", SendMessageOptions.DontRequireReceiver);
			this.FX_PickupTrigger.SetActive(false);
		}
		if (!BoltNetwork.isClient)
		{
			base.state.AddCallback("FX_Fire", new PropertyCallbackSimple(this.OnReceivedFire));
			base.state.AddCallback("FX_PickupTrigger", new PropertyCallbackSimple(this.OnReceivedPickupTrigger));
		}
	}

	public override void Detached()
	{
		if (!Scene.SceneTracker)
		{
			return;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("storeRagDollPrefab"), base.transform.position, base.transform.rotation);
		gameObject.transform.position = base.transform.position;
		gameObject.transform.rotation = base.transform.rotation;
		storeLocalMutantInfo2 component = gameObject.GetComponent<storeLocalMutantInfo2>();
		Scene.SceneTracker.storedRagDollPrefabs.Add(gameObject);
		if (this.ca != null && this.ca.isSnow)
		{
			component.isSnow = true;
		}
		if (!this.ast)
		{
			this.ast = base.transform.GetComponentInChildren<arrowStickToTarget>();
		}
		if (!this.ast)
		{
			this.ast = base.transform.GetComponent<arrowStickToTarget>();
		}
		if (this.ast)
		{
			int num = 0;
			foreach (KeyValuePair<Transform, int> keyValuePair in this.ast.stuckArrows)
			{
				if (keyValuePair.Key)
				{
					component.stuckArrowsIndex.Add(keyValuePair.Key, keyValuePair.Value);
					component.stuckArrowPos.Add(keyValuePair.Key.localPosition);
					component.stuckArrowRot.Add(keyValuePair.Key.localRotation);
					if (num < this.ast.stuckArrowsTypeList.Count)
					{
						component.stuckArrowType.Add(this.ast.stuckArrowsTypeList[num]);
					}
					num++;
				}
			}
		}
		mutantTransferFire component2 = base.transform.GetComponent<mutantTransferFire>();
		if (component2)
		{
			for (int i = 0; i < component2.clientFireGo.Count; i++)
			{
				spawnParticleController component3 = component2.clientFireGo[i].GetComponent<spawnParticleController>();
				if (component3 && component3.spawnedPrefab)
				{
					component.fire.Add(component3.spawnedPrefab.transform);
					component.firePos.Add(component2.clientFireGo[i].transform.localPosition);
					component.fireRot.Add(component2.clientFireGo[i].transform.localRotation);
					component3.spawnedPrefab.SendMessage("disableFollowTarget");
					Debug.Log("detached client fire");
				}
			}
		}
	}

	private void OnReceivedFire()
	{
		if (this.FX_Fire)
		{
			this.FX_Fire.SetActive(base.state.FX_Fire);
		}
	}

	private void OnReceivedPickupTrigger()
	{
		if (this.FX_PickupTrigger)
		{
			this.FX_PickupTrigger.SetActive(base.state.FX_PickupTrigger);
		}
	}

	public void Update()
	{
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && !base.entity.isOwner && this.FX_Fire)
		{
			this.FX_Fire.SetActive(base.state.FX_Fire);
		}
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			if (this.FX_Fire)
			{
				base.state.FX_Fire = this.FX_Fire.activeInHierarchy;
			}
			if (this.FX_PickupTrigger)
			{
				base.state.FX_PickupTrigger = this.FX_PickupTrigger.activeInHierarchy;
			}
		}
	}

	public GameObject FX_Fire;

	public GameObject FX_PickupTrigger;

	private arrowStickToTarget ast;

	private CoopAnimal ca;
}
