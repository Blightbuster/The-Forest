using System;
using HutongGames.PlayMaker;
using UnityEngine;


public class lizardTargetFunctions : MonoBehaviour
{
	
	private void Start()
	{
		this.thisTr = base.transform.root;
		this.thisGO = base.transform.root.gameObject;
		PlayMakerFSM[] components = base.transform.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "aiBaseFSM")
			{
				this.pmBase = playMakerFSM;
			}
		}
		this.fsmTargetObjectVec = this.pmBase.FsmVariables.GetFsmVector3("targetObjectVec");
		this.fsmTargetLookPos = this.pmBase.FsmVariables.GetFsmVector3("targetLookPos");
		this.fsmTargetObjectGO = this.pmBase.FsmVariables.GetFsmGameObject("targetObjectGO");
	}

	
	private void OnTriggerEnter(Collider source)
	{
		if (source.gameObject.CompareTag("Tree") && UnityEngine.Random.Range(0, 1) == 0)
		{
			Vector3 vector = this.thisTr.InverseTransformPoint(source.transform.position);
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
			if (num > -50f && num < 50f && !this.fsmTargetObjectGO.Value)
			{
				this.fsmTargetObjectGO.Value = source.gameObject;
				this.fsmTargetLookPos.Value = source.bounds.center;
				Vector3 normalized = (this.thisTr.position - source.bounds.center).normalized;
				Vector3 value = source.bounds.center + normalized * 1.6f;
				this.fsmTargetObjectVec.Value = value;
			}
		}
	}

	
	private Transform thisTr;

	
	private GameObject thisGO;

	
	private PlayMakerFSM pmBase;

	
	private FsmVector3 fsmTargetObjectVec;

	
	private FsmVector3 fsmTargetLookPos;

	
	private FsmGameObject fsmTargetObjectGO;

	
	private bool detected;
}
