using System;
using Bolt;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

public class activateGirlToMachine : EntityEventListener
{
	private void Awake()
	{
		base.enabled = false;
	}

	private void GrabEnter()
	{
		if (this.Sheen)
		{
			this.Sheen.SetActive(false);
		}
		this.MyPickUp.SetActive(true);
		base.enabled = true;
	}

	private void GrabExit()
	{
		if (this.Sheen)
		{
			this.Sheen.SetActive(true);
		}
		this.MyPickUp.SetActive(false);
		base.enabled = false;
	}

	private void Update()
	{
		if (!LocalPlayer.AnimControl.holdingGirl || this.startedPickup || LocalPlayer.AnimControl.onRope || LocalPlayer.AnimControl.onRaft || !LocalPlayer.FpCharacter.Grounded)
		{
			this.MyPickUp.SetActive(false);
			return;
		}
		if (!this.startedPickup && LocalPlayer.AnimControl.holdingGirl)
		{
			this.MyPickUp.SetActive(true);
		}
		if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false) && !this.startedPickup && LocalPlayer.AnimControl.holdingGirl)
		{
			this.startedPickup = true;
			LocalPlayer.SpecialActions.SendMessage("setSpectator", false, SendMessageOptions.DontRequireReceiver);
			if (this.sequence)
			{
				this.sequence.BeginStage(1);
			}
			else
			{
				this.DoActorAnimation();
			}
		}
	}

	public void DoActorAnimation()
	{
		LocalPlayer.SpecialActions.SendMessage("setSpectator", false, SendMessageOptions.DontRequireReceiver);
		this.InitAnim();
	}

	public void DoSpectatorAnimation()
	{
		if (this.sequence.Proxy.state.Actor)
		{
			Animator componentInChildren = this.sequence.Proxy.state.Actor.GetComponentInChildren<Animator>();
			LocalPlayer.SpecialActions.SendMessage("setActorAnimator", componentInChildren, SendMessageOptions.DontRequireReceiver);
		}
		if (LocalPlayer.SpecialActions)
		{
			LocalPlayer.SpecialActions.SendMessage("setSpectator", true, SendMessageOptions.DontRequireReceiver);
		}
		this.InitAnim();
	}

	private void InitAnim()
	{
		LocalPlayer.Sfx.PlayWhoosh();
		this.setPickupRoutine();
		this.startedPickup = true;
	}

	public void DoLateCompletion()
	{
		EventRegistry.Endgame.Publish(TfEvent.StoryProgress, GameStats.StoryElements.MeganFound);
	}

	private void setPickupRoutine()
	{
		LocalPlayer.SpecialActions.SendMessage("setCurrentSequence", this.sequence);
		LocalPlayer.SpecialActions.SendMessage("girlToMachineRoutine", this.playerPos, SendMessageOptions.DontRequireReceiver);
		base.gameObject.SetActive(false);
		base.Invoke("Finished", 12f);
	}

	private void Finished()
	{
		EventRegistry.Player.Publish(TfEvent.StoryProgress, GameStats.StoryElements.MeganFound);
		if (this.sequence)
		{
			this.sequence.CompleteStage(1);
		}
	}

	public Transform playerPos;

	public GameObject Sheen;

	public GameObject MyPickUp;

	public bool startedPickup;

	private int idleHash = Animator.StringToHash("idling");

	private int heldHash = Animator.StringToHash("held");

	private AnimatorStateInfo pState;

	public GameObject FinalPoseMegan;

	public bool fromMutantRagdoll;

	public AnimationSequence sequence;
}
