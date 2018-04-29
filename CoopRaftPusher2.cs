using System;
using Bolt;
using TheForest.Utils;
using UniLinq;
using UnityEngine;


public class CoopRaftPusher2 : EntityBehaviour<IRaftState>
{
	
	private void Awake()
	{
		base.enabled = !BoltNetwork.isRunning;
	}

	
	public override void Attached()
	{
		base.enabled = true;
	}

	
	private void Update()
	{
		if ((!BoltNetwork.isRunning || BoltNetwork.isServer) && this.raftPush.InWater && this.raftMainBody.constraints != RigidbodyConstraints.None)
		{
			this.raftMainBody.constraints = RigidbodyConstraints.None;
		}
		if (this.enteredTransform && this.raftMainTransform)
		{
			float magnitude = (this.raftMainTransform.position - this.enteredTransform.position).magnitude;
			if (magnitude > 30f || (this.enteredTransform.tag != "Player" && this.enteredTransform.tag != "PlayerNet"))
			{
				this.forcePlayerExit();
			}
		}
		if (this.enteredTransform && LocalPlayer.AnimControl && !LocalPlayer.AnimControl.swimming && !Grabber.IsFocused && !this.IsUpsideDown)
		{
			if (BoltNetwork.isRunning)
			{
				if (base.state.GrabbedBy.Any((BoltEntity g) => g))
				{
					goto IL_1B8;
				}
			}
			if (!this.billboardIconPush.activeSelf)
			{
				this.billboardIconPush.SetActive(true);
			}
			if (TheForest.Utils.Input.GetButtonDown("Take"))
			{
				if (BoltNetwork.isRunning)
				{
					PushRaft pushRaft = global::PushRaft.Create(GlobalTargets.OnlyServer);
					pushRaft.Direction = this.GetPushDirection();
					pushRaft.Raft = base.entity;
					pushRaft.Send();
				}
				else
				{
					this.PushRaft(this.GetPushDirection());
				}
			}
			return;
		}
		IL_1B8:
		if (this.billboardIconPush.activeSelf)
		{
			this.billboardIconPush.SetActive(false);
		}
	}

	
	private Vector3 GetPushDirection()
	{
		return this.raftMainTransform.position - this.enteredTransform.position;
	}

	
	private void OnTriggerEnter(Collider collider)
	{
		if (base.enabled)
		{
			if (collider.gameObject.CompareTag("Player"))
			{
				this.enteredTransform = collider.transform;
				this.enteredCollider = collider;
				this.billboardIconPush.transform.position = Vector3.Lerp(base.transform.position, collider.transform.position, this.iconPositionAlpha);
			}
			if ((collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("PlayerNet")) && !BoltNetwork.isClient && ++this.playersClose > 0)
			{
				this.raftMainBody.constraints = RigidbodyConstraints.FreezeAll;
			}
		}
	}

	
	private void OnTriggerExit(Collider collider)
	{
		if (base.enabled)
		{
			if (collider.gameObject.CompareTag("Player"))
			{
				this.enteredTransform = null;
				this.enteredCollider = null;
				this.billboardIconPush.SetActive(false);
			}
			if ((collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("PlayerNet")) && !BoltNetwork.isClient && --this.playersClose < 1)
			{
				this.raftMainBody.constraints = RigidbodyConstraints.None;
			}
		}
	}

	
	private void forcePlayerExit()
	{
		this.enteredTransform = null;
		this.enteredCollider = null;
		this.billboardIconPush.SetActive(false);
		if (!BoltNetwork.isClient && --this.playersClose < 1)
		{
			this.raftMainBody.constraints = RigidbodyConstraints.None;
		}
	}

	
	public void PushRaft(Vector3 worldDirection)
	{
		if (this.IsRaftGrabbed)
		{
			return;
		}
		this.raftMainBody.constraints = RigidbodyConstraints.None;
		this.raftMainBody.AddForce(worldDirection.normalized * this.pushForce * (0.016666f / Time.fixedDeltaTime), this.pushForceMode);
		Buoyancy componentInParent = base.GetComponentInParent<Buoyancy>();
		bool onWater = componentInParent != null && componentInParent.InWater;
		if (LocalPlayer.Transform)
		{
			LocalPlayer.Sfx.PlayPushRaft(onWater, base.gameObject);
		}
	}

	
	
	private bool IsRaftGrabbed
	{
		get
		{
			bool result;
			if (BoltNetwork.isRunning && base.entity.isAttached)
			{
				result = base.state.GrabbedBy.Any((BoltEntity g) => g);
			}
			else
			{
				result = false;
			}
			return result;
		}
	}

	
	
	private bool IsUpsideDown
	{
		get
		{
			return Vector3.Angle(this.raftMainTransform.transform.up, Vector3.up) > 90f;
		}
	}

	
	public Transform enteredTransform;

	
	private Collider enteredCollider;

	
	public int playersClose;

	
	[SerializeField]
	private GameObject billboardIconPush;

	
	[SerializeField]
	private Rigidbody raftMainBody;

	
	[SerializeField]
	private Transform raftMainTransform;

	
	[SerializeField]
	private RaftPush raftPush;

	
	[SerializeField]
	private float pushForce = 20f;

	
	[SerializeField]
	private ForceMode pushForceMode = ForceMode.VelocityChange;

	
	[SerializeField]
	private float iconPositionAlpha = 0.5f;
}
