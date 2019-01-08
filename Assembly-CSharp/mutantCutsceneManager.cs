﻿using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class mutantCutsceneManager : MonoBehaviour
{
	private void Start()
	{
		this.Tr = base.transform;
		if (this.typeHitPlayer)
		{
			this.animator = base.GetComponentInChildren<Animator>();
		}
		else
		{
			this.animator = base.GetComponent<Animator>();
		}
		if (this.randomSpeed)
		{
			this.animator.speed = UnityEngine.Random.Range(0.9f, 1.2f);
		}
		else
		{
			this.animator.speed = this.animatorSpeed;
		}
		if (this.typeCarryTimmy && this.startDelay > 0f)
		{
			base.Invoke("pickupTimmy", this.startDelay);
			base.Invoke("turnAround", this.startDelay + 5f);
		}
		if (this.typeIdleTurnAroundWalkAway)
		{
			this.animator.SetIntegerReflected("startInt1", UnityEngine.Random.Range(0, 2));
			this.animator.SetTriggerReflected("startTrigger");
			this.randDelay = UnityEngine.Random.Range(this.startDelay - 0.5f, this.startDelay + 0.5f);
			if (this.startDelay > 0f)
			{
				base.Invoke("turnAround", this.randDelay);
			}
		}
		if (this.typeBackAwayCareful)
		{
			this.animator.SetIntegerReflected("startInt1", 2);
			this.animator.SetTriggerReflected("startTrigger");
		}
		if (this.typeHitPlayer)
		{
			this.setup = base.transform.GetComponentInChildren<mutantScriptSetup>();
			if (this.startDelay > 0f)
			{
				base.Invoke("runHitPlayer", this.startDelay);
			}
		}
	}

	private void Update()
	{
		this.playerDist = Vector3.Distance(LocalPlayer.Transform.position, this.Tr.position);
		if (this.typeCarryTimmy && this.startDelay == 0f)
		{
			if (this.playerDist < this.pickupTimmyDistance)
			{
				this.animator.SetTriggerReflected("startTrigger");
			}
			if (this.playerDist < this.walkAwayDistance && !this.trigger)
			{
				base.Invoke("turnAround", UnityEngine.Random.Range(0f, 1f));
				this.trigger = true;
			}
		}
	}

	private void pickupTimmy()
	{
		this.animator.SetTriggerReflected("startTrigger");
	}

	private void turnAround()
	{
		if (this.typeIdleTurnAroundWalkAway || this.typeCarryTimmy)
		{
			this.animator.SetIntegerReflected("randInt1", UnityEngine.Random.Range(0, 2));
			this.animator.SetBoolReflected("walkBool", true);
			if (this.otherMembers.Length > 0)
			{
				foreach (GameObject value in this.otherMembers)
				{
					base.StartCoroutine("otherTurnAround", value);
				}
			}
		}
	}

	private IEnumerator otherTurnAround(GameObject member)
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
		member.SendMessage("turnAround");
		yield return YieldPresets.WaitThreeSeconds;
		member.SendMessage("runHitPlayer");
		yield break;
	}

	private void runHitPlayer()
	{
		if (this.typeHitPlayer)
		{
			this.setup.pmCombat.SendEvent("doAction");
		}
	}

	private Animator animator;

	private mutantScriptSetup setup;

	private Transform Tr;

	private float playerDist;

	public bool typeCarryTimmy;

	public float pickupTimmyDistance;

	public float walkAwayDistance;

	public bool typeIdleTurnAroundWalkAway;

	public bool typeBackAwayCareful;

	public bool typeHitPlayer;

	public float startDelay;

	private float randDelay;

	public bool randomSpeed;

	public float animatorSpeed;

	public bool showWeapon;

	public bool randomProps;

	public GameObject[] otherMembers;

	private bool trigger;
}
