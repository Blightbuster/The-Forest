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
		GameObject store = UnityEngine.Object.Instantiate<GameObject>(this.storePrefab, base.transform.position, base.transform.rotation);
		storeLocalMutantInfo2 slmi = store.GetComponent<storeLocalMutantInfo2>();
		slmi.identifier = this.cms.storedRagDollName;
		slmi.jointAngles.Clear();
		for (int i = 0; i < this.jointsToSync.Length; i++)
		{
			slmi.jointAngles.Add(this.jointsToSync[i].localRotation);
		}
		CoopMutantMaterialSync cmms = base.transform.parent.GetComponent<CoopMutantMaterialSync>();
		if (cmms)
		{
			slmi.matColor = cmms.storedColor;
		}
		if (this.ast)
		{
			int num = 0;
			foreach (KeyValuePair<Transform, int> keyValuePair in this.ast.stuckArrows)
			{
				if (keyValuePair.Key)
				{
					slmi.stuckArrowsIndex.Add(keyValuePair.Key, keyValuePair.Value);
					slmi.stuckArrowPos.Add(keyValuePair.Key.localPosition);
					slmi.stuckArrowRot.Add(keyValuePair.Key.localRotation);
					if (num < this.ast.stuckArrowsTypeList.Count)
					{
						slmi.stuckArrowType.Add(this.ast.stuckArrowsTypeList[num]);
					}
					num++;
				}
			}
		}
		mutantTransferFire mtf = base.transform.parent.GetComponent<mutantTransferFire>();
		if (mtf)
		{
			for (int j = 0; j < mtf.clientFireGo.Count; j++)
			{
				spawnParticleController component = mtf.clientFireGo[j].GetComponent<spawnParticleController>();
				if (component && component.spawnedPrefab)
				{
					int value = mtf.allBones.IndexOf(mtf.clientFireGo[j].transform.parent);
					slmi.fireIndex.Add(component.spawnedPrefab.transform, value);
					slmi.firePos.Add(mtf.clientFireGo[j].transform.localPosition);
					slmi.fireRot.Add(mtf.clientFireGo[j].transform.localRotation);
					component.spawnedPrefab.SendMessage("disableFollowTarget");
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
