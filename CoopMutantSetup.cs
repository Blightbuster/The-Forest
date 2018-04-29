using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class CoopMutantSetup : EntityEventListener<IMutantState>
{
	
	private void Start()
	{
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.ai_net = base.transform.GetComponentInChildren<mutantAI_net>();
		this.mrs = base.transform.GetComponentInChildren<mutantRagdollSetup>();
		this.ast = base.transform.GetComponentInChildren<arrowStickToTarget>();
	}

	
	public override void Detached()
	{
		if (BoltNetwork.isClient && Scene.MutantControler.activeNetCannibals.Contains(base.gameObject))
		{
			Scene.MutantControler.activeNetCannibals.Remove(base.gameObject);
		}
		if (this.creepy && BoltNetwork.isClient)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.storePrefab, base.transform.position, base.transform.rotation);
			storeLocalMutantInfo2 component = gameObject.GetComponent<storeLocalMutantInfo2>();
			Scene.SceneTracker.storedRagDollPrefabs.Add(gameObject);
			component.identifier = this.storedRagDollName;
			component.rootRotation = this.rootTr.rotation;
			component.rootPosition = this.rootTr.position;
			mutantTransferFire component2 = base.transform.GetComponent<mutantTransferFire>();
			if (component2)
			{
				for (int i = 0; i < component2.clientFireGo.Count; i++)
				{
					spawnParticleController component3 = component2.clientFireGo[i].GetComponent<spawnParticleController>();
					if (component3 && component3.spawnedPrefab)
					{
						int value = component2.allBones.IndexOf(component2.clientFireGo[i].transform.parent);
						component.fireIndex.Add(component3.spawnedPrefab.transform, value);
						component.firePos.Add(component2.clientFireGo[i].transform.localPosition);
						component.fireRot.Add(component2.clientFireGo[i].transform.localRotation);
						component3.spawnedPrefab.SendMessage("disableFollowTarget");
					}
				}
			}
			CoopMutantMaterialSync component4 = base.transform.GetComponent<CoopMutantMaterialSync>();
			if (component4)
			{
				component.matColor = component4.storedColor;
			}
			for (int j = 0; j < this.storedCreepyJoints.Length; j++)
			{
				component.jointAngles.Add(this.storedCreepyJoints[j].localRotation);
			}
			if (this.ast)
			{
				foreach (KeyValuePair<Transform, int> keyValuePair in this.ast.stuckArrows)
				{
					if (keyValuePair.Key)
					{
						component.stuckArrowsIndex.Add(keyValuePair.Key, keyValuePair.Value);
						component.stuckArrowPos.Add(keyValuePair.Key.localPosition);
						component.stuckArrowRot.Add(keyValuePair.Key.localRotation);
					}
				}
			}
		}
	}

	
	public void getRagDollName(int name)
	{
		this.storedRagDollName = name;
	}

	
	public override void OnEvent(FxEnemeyBlood evnt)
	{
		this.BloodActual();
	}

	
	public override void Attached()
	{
		if (BoltNetwork.isClient && !Scene.MutantControler.activeNetCannibals.Contains(base.gameObject))
		{
			Scene.MutantControler.activeNetCannibals.Add(base.gameObject);
		}
	}

	
	private void enableNetRagDoll()
	{
		this.mrs.setupRagDollParts(true);
	}

	
	private void setClientNoosePivot(Transform tr)
	{
		this.nooseTrapPivot = tr;
	}

	
	private void setClientTrigger(GameObject go)
	{
		this.nooseTrapGo = go;
		this.nooseFixer = go.GetComponent<clientNooseTrapFixer>();
	}

	
	public void BloodActual()
	{
	}

	
	private IEnumerator fixBloodPosition(Transform tr)
	{
		float t = 0f;
		while (t < 0.2f)
		{
			tr.position = this.BloodPos.transform.position;
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	private Animator animator;

	
	private mutantAI_net ai_net;

	
	public bool creepy;

	
	public GameObject BloodPos;

	
	public GameObject BloodSplat;

	
	public Transform toothPickup;

	
	public Transform nooseTrapPivot;

	
	public GameObject nooseTrapGo;

	
	public clientNooseTrapFixer nooseFixer;

	
	public Transform rootTr;

	
	private mutantRagdollSetup mrs;

	
	private arrowStickToTarget ast;

	
	public GameObject storePrefab;

	
	public int storedRagDollName;

	
	public Transform[] storedCreepyJoints;

	
	public bool net;

	
	private float teethCoolDown;
}
