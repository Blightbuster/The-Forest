using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;


public class activateGirlPickup : EntityEventListener
{
	
	private void Awake()
	{
		base.enabled = false;
	}

	
	private void GrabEnter()
	{
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
		base.enabled = true;
	}

	
	private void GrabExit()
	{
		this.Sheen.SetActive(true);
		this.MyPickUp.SetActive(false);
		base.enabled = false;
	}

	
	private void Update()
	{
		if ((LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).tagHash != this.idleHash || (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).tagHash != this.idleHash && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).tagHash != this.heldHash)) && !this.startedPickup && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.onRaft)
		{
			this.MyPickUp.SetActive(false);
			return;
		}
		if (!this.startedPickup)
		{
			this.MyPickUp.SetActive(true);
		}
		if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false) && !this.startedPickup)
		{
			LocalPlayer.Sfx.PlayWhoosh();
			base.StartCoroutine(this.setPickupRoutine());
			this.startedPickup = true;
		}
	}

	
	private IEnumerator setPickupRoutine()
	{
		if (this.fromMutantRagdoll)
		{
			if (this._rootGo && !BoltNetwork.isClient)
			{
				destroyAfter destroyAfter = this._rootGo.AddComponent<destroyAfter>();
				destroyAfter.destroyTime = 60f;
			}
			GameObject value = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_Pickup"), LocalPlayer.PlayerBase.transform.position, LocalPlayer.PlayerBase.transform.rotation);
			LocalPlayer.SpecialActions.SendMessage("setGirlGo", value, SendMessageOptions.DontRequireReceiver);
			LocalPlayer.SpecialActions.SendMessage("setGirlTrigger", base.gameObject, SendMessageOptions.DontRequireReceiver);
			this.ragdollGo.SetActive(false);
		}
		else
		{
			LocalPlayer.SpecialActions.SendMessage("setGirlGo", this.girlGo, SendMessageOptions.DontRequireReceiver);
			LocalPlayer.SpecialActions.SendMessage("setGirlTrigger", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		LocalPlayer.SpecialActions.SendMessage("pickupGirlRoutine", base.transform.position, SendMessageOptions.DontRequireReceiver);
		if (BoltNetwork.isRunning)
		{
			base.entity.Freeze(false);
			syncGirlPickup syncGirlPickup = syncGirlPickup.Create(GlobalTargets.Everyone);
			syncGirlPickup.target = base.transform.GetComponentInParent<BoltEntity>();
			syncGirlPickup.disableTrigger = true;
			syncGirlPickup.Send();
		}
		base.gameObject.SetActive(false);
		yield return null;
		yield break;
	}

	
	private void resetPickup()
	{
		this.startedPickup = false;
	}

	
	public GameObject _rootGo;

	
	public GameObject girlGo;

	
	public GameObject ragdollGo;

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public bool startedPickup;

	
	private int idleHash = Animator.StringToHash("idling");

	
	private int heldHash = Animator.StringToHash("held");

	
	private AnimatorStateInfo pState;

	
	public bool fromMutantRagdoll;
}
