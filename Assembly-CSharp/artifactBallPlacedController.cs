using System;
using Bolt;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;

[DoNotSerializePublic]
public class artifactBallPlacedController : EntityBehaviour<IArtifactBallState>
{
	private void Awake()
	{
		this._ballPropertyBlock = new MaterialPropertyBlock();
	}

	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
		}
	}

	private void OnEnable()
	{
		this.artifactSetup = base.transform.GetComponentInChildren<artifactBallEffectSetup>();
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.animator.CrossFade(this.idleTransformedHash, 0f, 0);
		if (!this.firstTimePickup)
		{
			this.animator.SetBool("transform", true);
		}
		this.mat = this.ballRenderer.sharedMaterial;
		if (this.firstTimePickup)
		{
			this.animator.SetFloat("spinSpeed", 7f);
			this.setArtifactState(2);
			this.currentState = 2;
			if (this.LastBuiltLocationGo)
			{
				this.LastBuiltLocationGo.SetActive(false);
			}
		}
		if (this.TriggerGo)
		{
			this.TriggerGo.SetActive(false);
			base.Invoke("EnablePickupTrigger", 4f);
		}
	}

	private void EnablePickupTrigger()
	{
		if (this.TriggerGo)
		{
			this.TriggerGo.SetActive(true);
		}
	}

	private void OnDisable()
	{
		this.StopPowerOnEvent();
	}

	private void Update()
	{
		if (this.firstTimePickup && !CoopPeerStarter.DedicatedHost && Time.time > this.disableUpdateRate && Scene.SceneTracker && Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position) > 100f)
		{
			this.disableUpdateRate = Time.time + 3f;
			return;
		}
		if (this.firstTimePickup)
		{
			this.setArtifactState(2);
		}
		this.updateArtifactState();
	}

	private void updateArtifactState()
	{
		if (Time.time > this.updateRate)
		{
			if (this.currentState == 1)
			{
				this.targetColor = this.attractColor;
				this.targetColorLight = this.attractColorLight;
				if (!this.firstTimePickup)
				{
					this.artifactSetup.sendEnemyState(true);
					this.artifactSetup.effigyEffectGo.SetActive(false);
				}
				this.AffirmPowerOnEvent();
			}
			else if (this.currentState == 2)
			{
				this.targetColor = this.repelColor;
				this.targetColorLight = this.repelColorLight;
				if (!this.firstTimePickup)
				{
					this.artifactSetup.sendEnemyState(false);
					this.artifactSetup.effigyEffectGo.SetActive(true);
				}
				this.AffirmPowerOnEvent();
			}
			else
			{
				this.targetColor = this.idleColor;
				this.artifactSetup.effigyEffectGo.SetActive(false);
			}
			this.updateRate = Time.time + 5f;
		}
		Color emissionColorProperty = this.targetColor * Mathf.LinearToGammaSpace(1.8f);
		this.SetEmissionColorProperty(emissionColorProperty);
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner && base.state.artifactStateMP != this.currentState)
		{
			base.state.artifactStateMP = this.currentState;
		}
		if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && !base.entity.isOwner)
		{
			this.currentState = base.state.artifactStateMP;
			if (this.currentState == 1)
			{
				this.animator.SetFloat("spinSpeed", -10f);
			}
			else if (this.currentState == 2)
			{
				this.animator.SetFloat("spinSpeed", 10f);
			}
			else if (this.currentState == 0)
			{
				this.animator.SetFloat("spinSpeed", -1f);
			}
		}
		if (this.artifactLight)
		{
			this.artifactLight.color = this.targetColorLight;
			this.artifactLight.intensity = 0.4f;
		}
	}

	private void SetEmissionColorProperty(Color setColor)
	{
		this.ballRenderer.GetPropertyBlock(this._ballPropertyBlock);
		this._ballPropertyBlock.SetColor("_EmissionColor", setColor);
		this.ballRenderer.SetPropertyBlock(this._ballPropertyBlock);
	}

	public void setSpinSpeed(float val)
	{
		if (!this.animator)
		{
			this.animator = base.transform.GetComponentInChildren<Animator>();
		}
		this.animator.SetFloat("spinSpeed", val);
	}

	public void setArtifactState(int _state)
	{
		this.currentState = _state;
		this.updateRate = 0f;
	}

	private void artifactStateChanged()
	{
		this.currentState = base.state.artifactStateMP;
	}

	private void AffirmPowerOnEvent()
	{
		if (this.powerOnInstance == null && !CoopPeerStarter.DedicatedHost)
		{
			this.powerOnInstance = FMODCommon.PlayOneshot("event:/combat/weapons/artifact/artifact_dropped_loop", base.transform);
		}
	}

	private void StopPowerOnEvent()
	{
		if (this.powerOnInstance != null)
		{
			UnityUtil.ERRCHECK(this.powerOnInstance.stop(STOP_MODE.IMMEDIATE));
			this.powerOnInstance.release();
			this.powerOnInstance = null;
		}
	}

	public void setHeldArtifactState()
	{
		if (LocalPlayer.Transform)
		{
			LocalPlayer.SpecialActions.SendMessage("setHeldArtifactState", this.currentState);
		}
	}

	private Animator animator;

	public GameObject TriggerGo;

	public GameObject LastBuiltLocationGo;

	private artifactBallEffectSetup artifactSetup;

	public Renderer ballRenderer;

	public Light artifactLight;

	public int artifactState;

	private float updateRate;

	private float disableUpdateRate;

	[SerializeThis]
	public int currentState;

	private Material mat;

	public Color targetColor;

	public Color attractColor;

	public Color repelColor;

	public Color idleColor;

	public Color attractColorLight;

	public Color repelColorLight;

	private Color baseColorLight;

	private Color targetColorLight;

	private MaterialPropertyBlock _ballPropertyBlock;

	public bool firstTimePickup;

	private EventInstance powerOnInstance;

	private EventInstance powerOffInstance;

	private int idleTransformedHash = Animator.StringToHash("idleTransformed");
}
