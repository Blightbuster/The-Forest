using System;
using System.Collections.Generic;
using UnityEngine;


public class mutantRagdollFollow : MonoBehaviour
{
	
	private void Start()
	{
		if (this.debugRagdollSYNC)
		{
			return;
		}
		if (BoltNetwork.isClient)
		{
			this.mrs = base.transform.GetComponentInParent<mutantRagdollSetup>();
			this.startDelay = 1f;
			this.blendSpeed = 7f;
		}
		else
		{
			this.startDelay = 0.6f;
			this.blendSpeed = 6f;
		}
		for (int i = 0; i < this.ragdollJoints.Length; i++)
		{
			this.initRotations.Add(this.ragdollJoints[i].localRotation);
		}
	}

	
	private void OnEnable()
	{
		this.setupRagdollParams();
	}

	
	public void setupRagdollParams()
	{
		if (this.debugRagdollSYNC)
		{
			return;
		}
		if (this.initRotations.Count > 0)
		{
			for (int i = 0; i < this.ragdollJoints.Length; i++)
			{
				this.ragdollJoints[i].localRotation = this.initRotations[i];
			}
		}
		this.disableForMp = false;
		this.actualDelay = Time.time + this.startDelay;
	}

	
	private void OnDisable()
	{
		this.resetRagDollParams();
	}

	
	public void resetRagDollParams()
	{
		this.disableForMp = true;
		this.doMatch = false;
		this.dummyDropped = false;
		this.storeRot.Clear();
		if (this.initRotations.Count > 0)
		{
			for (int i = 0; i < this.ragdollJoints.Length; i++)
			{
				this.ragdollJoints[i].localRotation = this.initRotations[i];
			}
		}
	}

	
	public void setDropped()
	{
		if (this.initRotations.Count > 0)
		{
			for (int i = 0; i < this.ragdollJoints.Length; i++)
			{
				this.ragdollJoints[i].localRotation = this.initRotations[i];
			}
		}
		this.actualDelay = Time.time + this.startDelay;
		this.dropTime = Time.time + 6f;
		this.dummyDropped = true;
		this.disableForMp = false;
	}

	
	private void LateUpdate()
	{
		if (this.disableForMp)
		{
			return;
		}
		if (this.debugRagdollSYNC)
		{
			for (int i = 0; i < this.ragdollJoints.Length; i++)
			{
				this.targetJoints[i].rotation = this.ragdollJoints[i].rotation;
			}
			return;
		}
		if (this.dummy)
		{
			if (this.dummyDropped && Time.time < this.dropTime)
			{
				if (!this.doMatch)
				{
					if (Time.time < this.actualDelay)
					{
						return;
					}
					for (int j = 0; j < this.ragdollJoints.Length; j++)
					{
						this.ragdollJoints[j].rotation = this.targetJoints[j].rotation;
						this.storeRot.Add(this.targetJoints[j].rotation);
					}
					this.doMatch = true;
				}
				for (int k = 0; k < this.ragdollJoints.Length; k++)
				{
					this.storeRot[k] = Quaternion.Lerp(this.storeRot[k], this.ragdollJoints[k].rotation, Time.deltaTime * this.blendSpeed);
					this.targetJoints[k].rotation = this.storeRot[k];
				}
			}
			else
			{
				this.dummyDropped = false;
			}
			return;
		}
		if (Time.time < this.actualDelay)
		{
			return;
		}
		if (!this.doMatch)
		{
			for (int l = 0; l < this.ragdollJoints.Length; l++)
			{
				this.ragdollJoints[l].rotation = this.targetJoints[l].rotation;
				this.storeRot.Add(this.targetJoints[l].rotation);
				this.actualSmoothBlendTime = Time.time + 2.3f;
				this.finalRot.Add(this.targetJoints[l].rotation);
			}
			this.doMatch = true;
		}
		if (Time.time < this.actualSmoothBlendTime)
		{
			for (int m = 0; m < this.ragdollJoints.Length; m++)
			{
				this.storeRot[m] = Quaternion.Lerp(this.storeRot[m], this.ragdollJoints[m].rotation, Time.deltaTime * this.blendSpeed);
				this.targetJoints[m].rotation = this.storeRot[m];
				this.finalRot[m] = this.targetJoints[m].rotation;
			}
		}
		else
		{
			for (int n = 0; n < this.ragdollJoints.Length; n++)
			{
				if (BoltNetwork.isClient && !this.doneStoreJoints)
				{
					this.mrs.StartCoroutine("generateStoredJointList");
					this.doneStoreJoints = true;
				}
				this.storeRot[n] = Quaternion.Lerp(this.storeRot[n], this.finalRot[n], Time.deltaTime * this.blendSpeed);
				this.targetJoints[n].rotation = this.storeRot[n];
			}
		}
	}

	
	private float startDelay;

	
	private float actualDelay;

	
	private float actualSmoothBlendTime;

	
	private float dropTime;

	
	public bool dummyDropped;

	
	public bool dummy;

	
	public bool disableForMp;

	
	public Transform[] ragdollJoints;

	
	public Transform[] targetJoints;

	
	public bool doMatch;

	
	private int length;

	
	private float blendSpeed;

	
	private List<Quaternion> storeRot = new List<Quaternion>();

	
	private List<Quaternion> finalRot = new List<Quaternion>();

	
	private List<Quaternion> initRotations = new List<Quaternion>();

	
	private mutantRagdollSetup mrs;

	
	private bool doneStoreJoints;

	
	public bool debugRagdollSYNC;
}
