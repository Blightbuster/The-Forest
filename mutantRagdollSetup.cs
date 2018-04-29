using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;


public class mutantRagdollSetup : MonoBehaviour
{
	
	private void Start()
	{
		this.cms = base.transform.parent.GetComponent<CoopMutantSetup>();
		this.ast = base.transform.GetComponentInChildren<arrowStickToTarget>();
	}

	
	private void OnEnable()
	{
		base.StartCoroutine(this.enableRagDollParts(this.legParts, this.enableLegsTime, false));
		base.StartCoroutine(this.enableRagDollParts(this.armParts, this.enableArmsTime, false));
		base.StartCoroutine(this.enableRagDollParts(this.headParts, this.enableHeadTime, false));
		this.doneGenerateList = false;
	}

	
	public void setupRagDollParts(bool onoff)
	{
		base.StartCoroutine(this.enableRagDollParts(this.legParts, this.enableLegsTime, onoff));
		base.StartCoroutine(this.enableRagDollParts(this.armParts, this.enableArmsTime, onoff));
		base.StartCoroutine(this.enableRagDollParts(this.headParts, this.enableHeadTime, onoff));
	}

	
	public IEnumerator generateStoredJointList()
	{
		yield return new WaitForEndOfFrame();
		if (this.doneGenerateList)
		{
			yield break;
		}
		GameObject store = UnityEngine.Object.Instantiate(this.storePrefab, base.transform.position, base.transform.rotation) as GameObject;
		storeLocalMutantInfo2 slmi = store.GetComponent<storeLocalMutantInfo2>();
		slmi.identifier = this.cms.storedRagDollName;
		slmi.jointAngles.Clear();
		for (int x = 0; x < this.jointsToSync.Length; x++)
		{
			slmi.jointAngles.Add(this.jointsToSync[x].localRotation);
		}
		CoopMutantMaterialSync cmms = base.transform.parent.GetComponent<CoopMutantMaterialSync>();
		if (cmms)
		{
			slmi.matColor = cmms.storedColor;
		}
		if (this.ast)
		{
			foreach (KeyValuePair<Transform, int> attachStat in this.ast.stuckArrows)
			{
				if (attachStat.Key)
				{
					slmi.stuckArrowsIndex.Add(attachStat.Key, attachStat.Value);
					slmi.stuckArrowPos.Add(attachStat.Key.localPosition);
					slmi.stuckArrowRot.Add(attachStat.Key.localRotation);
				}
			}
		}
		Scene.SceneTracker.storedRagDollPrefabs.Add(store);
		this.doneGenerateList = true;
		yield break;
	}

	
	private IEnumerator enableRagDollParts(GameObject[] parts, float enableDelay, bool onoff)
	{
		if (onoff)
		{
			yield return new WaitForSeconds(enableDelay);
		}
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].SetActive(onoff);
		}
		yield return null;
		yield break;
	}

	
	private CoopMutantSetup cms;

	
	private arrowStickToTarget ast;

	
	public Transform[] jointsToSync;

	
	public GameObject[] legParts;

	
	public GameObject[] armParts;

	
	public GameObject[] headParts;

	
	public float enableLegsTime;

	
	public float enableArmsTime;

	
	public float enableHeadTime;

	
	public GameObject storePrefab;

	
	private bool doneGenerateList;
}
