using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;

public class Molotov : EntityBehaviour<IMolotovState>
{
	private void Start()
	{
		if (BoltNetwork.isRunning && base.entity && !base.entity.isAttached)
		{
			BoltNetwork.Attach(base.entity);
		}
	}

	private void OnEnable()
	{
		if (!CoopPeerStarter.DedicatedHost && ForestVR.Enabled)
		{
			BoxCollider component = base.transform.GetComponent<BoxCollider>();
			CapsuleCollider component2 = base.transform.GetComponent<CapsuleCollider>();
			if (component != null)
			{
				Physics.IgnoreCollision(component, LocalPlayer.AnimControl.playerCollider, true);
				Physics.IgnoreCollision(component, LocalPlayer.AnimControl.playerHeadCollider, true);
			}
			if (component2 != null)
			{
				Physics.IgnoreCollision(component2, LocalPlayer.AnimControl.playerCollider, true);
				Physics.IgnoreCollision(component2, LocalPlayer.AnimControl.playerHeadCollider, true);
			}
		}
	}

	public override void Detached()
	{
		if (base.state.Broken && !this._broken)
		{
			this.doBreakReal();
		}
	}

	public override void Attached()
	{
		Rigidbody componentInParent = base.GetComponentInParent<Rigidbody>();
		if (!base.entity.isOwner)
		{
			if (componentInParent)
			{
				componentInParent.useGravity = false;
				componentInParent.isKinematic = true;
			}
			FMOD_StudioEventEmitter.CreateStartOnAwakeEmitter(base.transform, "event:/combat/molotov_held");
		}
		base.state.Transform.SetTransforms((!componentInParent) ? base.transform : componentInParent.transform);
		base.state.AddCallback("Broken", new PropertyCallbackSimple(this.doBreakReal));
	}

	private void OnTriggerEnter(Collider other)
	{
		if (BoltNetwork.isRunning && base.entity && (!base.entity.isAttached || !base.entity.isOwner))
		{
			return;
		}
		if (other.CompareTag("Water"))
		{
			this.Wet = true;
		}
		else if (other.CompareTag("enemyCollide"))
		{
			this.doBreak(true);
			if (this.canDouse && !this.alreadyDoused)
			{
				other.gameObject.SendMessageUpwards("douseEnemy", SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessageUpwards("getAttackDirection", 3, SendMessageOptions.DontRequireReceiver);
				other.gameObject.SendMessage("getAttackerType", 4, SendMessageOptions.DontRequireReceiver);
				other.transform.SendMessageUpwards("Hit", 1, SendMessageOptions.DontRequireReceiver);
				this.alreadyDoused = true;
			}
			base.Invoke("CleanUp", 5f);
		}
		else if (other.CompareTag("FireTrigger"))
		{
			this.Wet = true;
			this.doBreak(false);
			if (BoltNetwork.isRunning)
			{
				FireAddFuelEvent fireAddFuelEvent = FireAddFuelEvent.Raise(GlobalTargets.OnlyServer);
				fireAddFuelEvent.Target = other.GetComponentInParent<BoltEntity>();
				fireAddFuelEvent.CanSetAlight = this.isLit;
				fireAddFuelEvent.Send();
			}
			else
			{
				if (this.isLit)
				{
					other.SendMessage("Burn");
				}
				other.SendMessage("Action_AddFuel");
			}
		}
		else if (this.DousedMolotovPrefab && other.CompareTag("BrokenMolotovUnlit"))
		{
			this.doBreak(false);
			if (!BoltNetwork.isRunning)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.DousedMolotovPrefab, other.transform.position, other.transform.rotation);
				UnityEngine.Object.Destroy(other.gameObject);
			}
			else
			{
				BoltNetwork.Instantiate(this.DousedMolotovPrefab, other.transform.position, other.transform.rotation);
				BoltNetwork.Destroy(other.gameObject);
			}
			this.DousedMolotovPrefab = null;
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		this.doMolotovCollision(collision);
	}

	public void doMolotovCollision(Collision collision)
	{
		if (BoltNetwork.isRunning && !base.entity.isOwner)
		{
			return;
		}
		if (!collision.gameObject.CompareTag("Player") && collision.transform.root != base.transform.root)
		{
			if (!this.Wet)
			{
				this.doBreak(false);
			}
			this.StartCleanUp();
		}
	}

	private void OnArrowHit()
	{
		base.transform.parent = null;
		this.doBreak(false);
	}

	public void IncendiaryBreak()
	{
		base.transform.parent = null;
		this.doBreak(false);
	}

	private void doBreak(bool shortFire = false)
	{
		if (BoltNetwork.isRunning && base.state != null)
		{
			base.state.Broken = true;
			base.transform.root.SendMessage("setStateBroken", true, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			base.transform.root.SendMessage("setStateBroken", true, SendMessageOptions.DontRequireReceiver);
			this.setShortFire = shortFire;
			this.doBreakReal();
		}
	}

	public void doBreakReal()
	{
		if (!this._broken)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.soundGo, base.transform.position, base.transform.rotation);
			this._broken = true;
			base.gameObject.GetComponentInParent<Rigidbody>().isKinematic = true;
			this.Bottle.SetActive(false);
			if (this.setShortFire)
			{
				MasterFireSpread component = this.BottleBroken.GetComponent<MasterFireSpread>();
				if (component)
				{
					component.useShortFire = true;
				}
			}
			this.BottleBroken.SetActive(true);
			if (!BoltNetwork.isRunning || base.entity.isOwner)
			{
				this.StartCleanUp();
			}
		}
	}

	private void StartCleanUp()
	{
		if (!this.CleanUpActive)
		{
			this.CleanUpActive = true;
			base.Invoke("CleanUp", this.DelayBeforeCleanup);
			destroyAfter destroyAfter = base.GetComponentInParent<Rigidbody>().gameObject.AddComponent<destroyAfter>();
			destroyAfter.destroyTime = this.DelayBeforeCleanup;
		}
	}

	private void CleanUp()
	{
		if (BoltNetwork.isRunning)
		{
			if (!base.entity.isAttached || base.entity.isOwner)
			{
				UnityEngine.Object.Destroy(base.GetComponentInParent<Rigidbody>().gameObject);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.GetComponentInParent<Rigidbody>().gameObject);
		}
	}

	private bool _broken;

	private bool CleanUpActive;

	private bool Wet;

	public bool canDouse;

	public bool isLit;

	private bool alreadyDoused;

	private bool setShortFire;

	public GameObject Bottle;

	public GameObject BottleBroken;

	public GameObject soundGo;

	public GameObject DousedMolotovPrefab;

	public float DelayBeforeCleanup = 4f;
}
