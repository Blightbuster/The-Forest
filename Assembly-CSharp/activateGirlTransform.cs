using System;
using TheForest.Utils;
using UnityEngine;

public class activateGirlTransform : MonoBehaviour
{
	private void Start()
	{
		base.enabled = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && !this.pickup)
		{
			this.pickup = true;
			if (this.sequence)
			{
				this.sequence.BeginStage(0);
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
		this.pickup = true;
		base.GetComponent<Collider>().enabled = false;
		if (!this.girlAnimator)
		{
			GameObject endgameBoss = Scene.SceneTracker.EndgameBoss;
			if (endgameBoss)
			{
				this.girlAnimator = endgameBoss.GetComponentInChildren<Animator>();
			}
		}
		if (BoltNetwork.isRunning)
		{
			BoltEntity component = this.girlAnimator.transform.root.GetComponent<BoltEntity>();
			if (component && component.isAttached)
			{
				component.Freeze(false);
			}
		}
		LocalPlayer.SpecialActions.SendMessage("setGirlAnimator", this.girlAnimator);
		LocalPlayer.SpecialActions.SendMessage("doGirlTransformRoutine", this.markTr);
		LocalPlayer.SpecialActions.SendMessage("setCurrentSequence", this.sequence);
	}

	public Transform markTr;

	public Animator girlAnimator;

	public bool pickup;

	public AnimationSequence sequence;
}
