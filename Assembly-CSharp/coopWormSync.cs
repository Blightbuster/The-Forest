using System;
using Bolt;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;

public class coopWormSync : EntityBehaviour<IMutantWormState>
{
	private void Start()
	{
		this.health = base.transform.GetComponent<wormHealth>();
		if (!CoopPeerStarter.DedicatedHost)
		{
			this.wormLoopInstance = FMOD_StudioSystem.instance.GetEvent("event:/mutants/creepies/Worm/worm_idle");
		}
		this.animator = base.transform.GetComponent<Animator>();
		this.baseSkin = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		this.controller = base.transform.GetComponent<wormSingleController>();
		this.skinColorBlock = new MaterialPropertyBlock();
		this._colorPropertyId = Shader.PropertyToID("_Color");
	}

	public override void Attached()
	{
		base.state.Transform.SetTransforms(base.transform);
		if (!base.entity.isOwner)
		{
			base.state.AddCallback("mecanimParam", new PropertyCallbackSimple(this.paramChanged));
			base.state.AddCallback("localOffset", new PropertyCallbackSimple(this.attachPosChanged));
			base.state.AddCallback("burning", new PropertyCallbackSimple(this.burningChanged));
		}
	}

	private void OnDestroy()
	{
		this.setWormIdleLoop(false);
	}

	private void Update()
	{
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
		{
			if (base.entity.isOwner)
			{
				if (this.animator.GetBool("wiggle"))
				{
					base.state.mecanimParam = 1;
				}
				else if (this.animator.GetBool("attached"))
				{
					base.state.mecanimParam = 2;
				}
				else if (this.animator.GetBool("birth"))
				{
					base.state.mecanimParam = 3;
				}
				else if (this.animator.GetBool("jump"))
				{
					base.state.mecanimParam = 4;
				}
				else
				{
					base.state.mecanimParam = 0;
				}
				base.state.localOffset = this.controller.storeAttachPos;
				base.state.burning = this.health.onFire;
			}
			else
			{
				switch (this.mecanimParam)
				{
				case 0:
					this.animator.SetBool("wiggle", false);
					this.animator.SetBool("attached", false);
					this.animator.SetBool("birth", false);
					this.animator.SetBool("jump", false);
					break;
				case 1:
					this.animator.SetBool("wiggle", true);
					this.animator.SetBool("attached", false);
					this.animator.SetBool("birth", false);
					this.animator.SetBool("jump", false);
					break;
				case 2:
					this.animator.SetBool("wiggle", false);
					this.animator.SetBool("attached", true);
					this.animator.SetBool("birth", false);
					this.animator.SetBool("jump", false);
					break;
				case 3:
					this.animator.SetBool("wiggle", false);
					this.animator.SetBool("attached", false);
					this.animator.SetBool("birth", true);
					this.animator.SetBool("jump", false);
					break;
				case 4:
					this.animator.SetBool("wiggle", false);
					this.animator.SetBool("attached", false);
					this.animator.SetBool("birth", false);
					this.animator.SetBool("jump", true);
					break;
				}
				bool flag = false || this.animator.GetBool("attached") || this.animator.GetBool("birth");
				this._lerpColor = Color.Lerp(this._lerpColor, (!flag) ? this.NormalSkin : this.AngrySkin, Time.deltaTime / 2f);
				if (this.baseSkin)
				{
					this.baseSkin.GetPropertyBlock(this.skinColorBlock);
					this.skinColorBlock.SetColor(this._colorPropertyId, this._lerpColor);
					this.baseSkin.SetPropertyBlock(this.skinColorBlock);
				}
				if (LocalPlayer.Transform == null)
				{
					return;
				}
				float sqrMagnitude = (LocalPlayer.Transform.position - base.transform.position).sqrMagnitude;
				this.setWormIdleLoop(sqrMagnitude <= 1600f);
				this.updateAttachPosition();
				this.fireGo.SetActive(this.burning);
			}
		}
	}

	private void updateAttachPosition()
	{
		Vector3 zero = Vector3.zero;
		if (this.animator.GetBool("attached"))
		{
			zero = this.attachPos;
		}
		this.offsetTr.localPosition = Vector3.Lerp(this.offsetTr.localPosition, zero, Time.deltaTime);
	}

	private void paramChanged()
	{
		this.mecanimParam = base.state.mecanimParam;
	}

	private void attachPosChanged()
	{
		this.attachPos = base.state.localOffset;
	}

	private void burningChanged()
	{
		this.burning = base.state.burning;
	}

	public void setWormIdleLoop(bool onOff)
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		PLAYBACK_STATE state;
		UnityUtil.ERRCHECK(this.wormLoopInstance.getPlaybackState(out state));
		if (onOff)
		{
			UnityUtil.ERRCHECK(this.wormLoopInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			if (!state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.wormLoopInstance.start());
			}
		}
		else if (state.isPlaying())
		{
			UnityUtil.ERRCHECK(this.wormLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
		}
	}

	private wormHealth health;

	public GameObject fireGo;

	public Color NormalSkin;

	public Color AngrySkin;

	private Color _lerpColor;

	private int _colorPropertyId;

	public Transform offsetTr;

	private MaterialPropertyBlock skinColorBlock;

	private int mecanimParam;

	private Vector3 attachPos;

	private bool burning;

	private Animator animator;

	private SkinnedMeshRenderer baseSkin;

	private wormSingleController controller;

	private EventInstance wormLoopInstance;
}
