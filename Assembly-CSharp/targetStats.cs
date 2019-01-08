using System;
using Bolt;
using UnityEngine;

public class targetStats : EntityBehaviour<IPlayerState>
{
	public override void Attached()
	{
		if (!this.setPlayerType)
		{
			return;
		}
		if (!base.entity.isOwner)
		{
			base.state.AddCallback("inWater", new PropertyCallbackSimple(this.inWaterChanged));
			base.state.AddCallback("onRaft", new PropertyCallbackSimple(this.onRaftChanged));
			base.state.AddCallback("isRed", new PropertyCallbackSimple(this.onRedChanged));
			base.state.AddCallback("arrowFire", new PropertyCallbackSimple(this.onArrowFireChanged));
			base.state.AddCallback("inYacht", new PropertyCallbackSimple(this.inYachtChanged));
			base.state.AddCallback("VREnabled", new PropertyCallbackSimple(this.VrChanged));
			base.state.AddCallback("rightHandActive", new PropertyCallbackSimple(this.rightHandChanged));
		}
	}

	private void Start()
	{
		if (this.setPlayerType)
		{
			this.animator = base.transform.GetComponentInChildren<Animator>();
		}
		this.targetDown = false;
		this.inNooseTrap = false;
	}

	private void Update()
	{
		if (this.setPlayerType)
		{
			this.state2 = this.animator.GetCurrentAnimatorStateInfo(2);
			if (this.state2.tagHash == this.deathTag)
			{
				this.targetDown = true;
			}
			else
			{
				this.targetDown = false;
			}
		}
		if (!this.setPlayerType)
		{
			return;
		}
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			base.state.inWater = this.inWater;
			base.state.onRaft = this.onRaft;
			base.state.isRed = this.isRed;
			base.state.arrowFire = this.arrowFire;
			base.state.inYacht = this.inYacht;
			base.state.VREnabled = this.VREnabled;
			base.state.rightHandActive = this.RightHandActive;
		}
	}

	public void setTargetDown()
	{
		this.targetDown = true;
	}

	public void setTargetUp()
	{
		this.targetDown = false;
	}

	private void inWaterChanged()
	{
		this.inWater = base.state.inWater;
	}

	private void inYachtChanged()
	{
		this.inYacht = base.state.inYacht;
	}

	private void onRaftChanged()
	{
		this.onRaft = base.state.onRaft;
	}

	private void onRedChanged()
	{
		this.isRed = base.state.isRed;
	}

	private void onArrowFireChanged()
	{
		this.arrowFire = base.state.arrowFire;
	}

	private void setInYacht(bool onoff)
	{
		if (onoff)
		{
			this.inYacht = true;
		}
		else
		{
			this.inYacht = false;
		}
	}

	private void VrChanged()
	{
		this.VREnabled = base.state.VREnabled;
	}

	private void rightHandChanged()
	{
		this.RightHandActive = base.state.rightHandActive;
	}

	public bool setPlayerType;

	private Animator animator;

	private AnimatorStateInfo state2;

	private int deathTag = Animator.StringToHash("death");

	public bool targetDown;

	public bool inNooseTrap;

	public bool onStructure;

	public bool inWater;

	public bool onRaft;

	public bool isRed;

	public bool onRope;

	public bool arrowFire;

	public bool inYacht;

	public bool VREnabled;

	public bool RightHandActive;
}
