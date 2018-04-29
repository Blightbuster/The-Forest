using System;
using HutongGames.PlayMaker;
using UnityEngine;


public class enemyDeathTrigger : MonoBehaviour
{
	
	private void OnDeserialized()
	{
		this.doStart();
	}

	
	private void Start()
	{
		this.doStart();
	}

	
	private void doStart()
	{
		this.player = GameObject.FindWithTag("Player");
		PlayMakerFSM[] componentsInChildren = this.player.transform.GetComponentsInChildren<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in componentsInChildren)
		{
			if (playMakerFSM.FsmName == "controlFSM")
			{
				this.pmControl = playMakerFSM;
			}
		}
		this.fsmDeathTrigger = this.pmControl.FsmVariables.GetFsmBool("deathTrigger");
		this.fsmTargetGO = this.pmControl.FsmVariables.GetFsmGameObject("targetGO");
		base.gameObject.SetActive(false);
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			this.fsmDeathTrigger.Value = true;
			this.fsmTargetGO.Value = other.gameObject;
		}
	}

	
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			this.fsmDeathTrigger.Value = false;
		}
	}

	
	private PlayMakerFSM pmControl;

	
	private FsmBool fsmDeathTrigger;

	
	private FsmGameObject fsmTargetGO;

	
	private GameObject player;
}
