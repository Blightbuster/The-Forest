using System;
using System.Collections;
using FMOD.Studio;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class artifactBallController : MonoBehaviour
{
	private void Awake()
	{
		this._ballPropertyBlock = new MaterialPropertyBlock();
		if (!this.remote)
		{
			this.playerAnimator = LocalPlayer.Animator;
		}
		this.artifactAnimator = base.transform.GetComponent<Animator>();
		this.mat = this.ballRenderer.sharedMaterial;
		this.mat.EnableKeyword("_EMISSION");
	}

	private void OnEnable()
	{
		if (!this.iconSetup && Scene.HudGui)
		{
			this.iconSetup = Scene.HudGui.PlaceArtifactButton.GetComponent<artifactIconSetup>();
		}
		if (!this.effectSetup)
		{
			this.effectSetup = base.transform.GetComponent<artifactBallEffectSetup>();
		}
		if (!this.artifactAnimator)
		{
			this.artifactAnimator = base.transform.GetComponent<Animator>();
		}
		if (!this.remote)
		{
			if (LocalPlayer.Animator.GetBool("firstTimePickup"))
			{
				this.artifactAnimator.SetBool("pickup", true);
				base.Invoke("resetPickup", 1f);
				Mood.StopConstantVibratation();
			}
			else
			{
				this.artifactAnimator.SetBool("pickup", false);
			}
		}
		if (this._mode == artifactBallController.artifactMode.idle)
		{
			this.targetSpin = 1f;
		}
		if (this._mode == artifactBallController.artifactMode.attract || this._mode == artifactBallController.artifactMode.repel)
		{
			this.AffirmPoweredUpEvent();
		}
		this.artifactAnimator.SetBool("open", false);
		this.artifactAnimator.CrossFade(this.idleHash, 0f, 0);
	}

	private void OnDisable()
	{
		this.StopPowerOnEvent();
		this.StopPowerOffEvent();
		this.StopPoweredUpEvent();
		this.artifactAnimator.SetBool("pickup", false);
		if (this.effigyEffectGo)
		{
			this.effigyEffectGo.SetActive(false);
		}
		this.doingSetArtifact = false;
		base.StopAllCoroutines();
		if (!this.remote)
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.PlaceArtifactButton.SetActive(false);
			}
			this.playerAnimator.SetBool("firstTimePickup", false);
			Mood.StopConstantVibratation();
		}
	}

	private void Update()
	{
		if (!this.remote && Scene.HudGui.PlaceArtifactButton)
		{
			if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._artifactId) && LocalPlayer.AnimControl.currLayerState1.shortNameHash == this.artifactIdleHash && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && !this.doingSetArtifact)
			{
				float x = LocalPlayer.MainCamTr.eulerAngles.x;
				bool flag = x > (float)((!Scene.HudGui.PlaceArtifactButton.activeSelf) ? 20 : 19) && x < 90f;
				if (Scene.HudGui.PlaceArtifactButton.activeSelf != flag)
				{
					Scene.HudGui.PlaceArtifactButton.SetActive(flag);
				}
				bool flag2;
				if (!LocalPlayer.AnimControl.wallSetup.atWallCheck && LocalPlayer.FpCharacter.Grounded && !LocalPlayer.FpCharacter.standingOnRaft && !LocalPlayer.FpCharacter.StandingOnDynamicObject)
				{
					if (!this.iconSetup.placeIcon.activeSelf)
					{
						this.iconSetup.placeIcon.SetActive(true);
					}
					if (!this.iconSetup.placeSprite.activeSelf)
					{
						this.iconSetup.placeSprite.SetActive(true);
					}
					flag2 = false;
				}
				else
				{
					if (this.iconSetup.placeIcon.activeSelf)
					{
						this.iconSetup.placeIcon.SetActive(false);
					}
					if (this.iconSetup.placeSprite.activeSelf)
					{
						this.iconSetup.placeSprite.SetActive(false);
					}
					flag2 = true;
				}
				if (artifactBallController.GetAttractInput())
				{
					if (this._mode == artifactBallController.artifactMode.attract)
					{
						base.StartCoroutine(this.setArtifactStateRoutine(artifactBallController.artifactMode.idle));
					}
					else
					{
						base.StartCoroutine(this.setArtifactStateRoutine(artifactBallController.artifactMode.attract));
					}
				}
				else if (artifactBallController.GetRepelInput())
				{
					if (this._mode == artifactBallController.artifactMode.repel)
					{
						base.StartCoroutine(this.setArtifactStateRoutine(artifactBallController.artifactMode.idle));
					}
					else
					{
						base.StartCoroutine(this.setArtifactStateRoutine(artifactBallController.artifactMode.repel));
					}
				}
				else if (artifactBallController.GetPlaceInput() && LocalPlayer.FpCharacter.Grounded && Scene.HudGui.PlaceArtifactButton.activeSelf && !flag2)
				{
					LocalPlayer.SpecialActions.SendMessage("placeArtifactRoutine", base.transform);
				}
			}
			else if (Scene.HudGui.PlaceArtifactButton.activeSelf)
			{
				Scene.HudGui.PlaceArtifactButton.SetActive(false);
			}
		}
		if (this.remote)
		{
			if (this.currentStateIndex != this.playerAnimator.GetInteger("artifactStateMP"))
			{
				this.setArtifactState(this.playerAnimator.GetInteger("artifactStateMP"));
			}
			if (this.playerAnimator.GetCurrentAnimatorStateInfo(2).shortNameHash == this.artifactPutDownHash)
			{
				this.artifactAnimator.SetBool("pickup", true);
			}
			else
			{
				this.artifactAnimator.SetBool("pickup", false);
			}
		}
		this.baseColor = Color.Lerp(this.baseColor, this.targetColor, Time.deltaTime / 5f);
		this.emission = Mathf.Lerp(this.emission, this.targetEmission, Time.deltaTime / 5f);
		Color emissionColorProperty = this.baseColor * Mathf.LinearToGammaSpace(this.emission);
		this.SetEmissionColorProperty(emissionColorProperty);
		this.spin = Mathf.Lerp(this.spin, this.targetSpin, Time.deltaTime / 2f);
		this.artifactAnimator.SetFloat("spinSpeed", this.spin);
		float num = this.spin / 2f;
		if (num < 0f)
		{
			num *= -1f;
		}
		if (num < 1f)
		{
			num = 1f;
		}
		if (!this.placedArtifact)
		{
			this.playerAnimator.SetFloat("artifactWobble", num);
		}
		if (!this.remote && !this.placedArtifact)
		{
			float num2 = this.spin / 50f;
			if (num2 < 0f)
			{
				num2 *= -1f;
			}
			if ((double)num2 > 0.03)
			{
				Mood.BeginConstantVibratation(num2, num2);
			}
			else
			{
				Mood.StopConstantVibratation();
			}
		}
		if (this.effectSetup && Time.time > this.effectUpdateRate && this._mode == artifactBallController.artifactMode.repel)
		{
			this.effectSetup.sendEnemyState(false);
			this.effectUpdateRate = Time.time + 5f;
		}
		if (this.artifactLight)
		{
			this.baseColorLight = Color.Lerp(this.baseColorLight, this.targetColorLight, Time.deltaTime / 5f);
			this.artifactLight.color = this.baseColorLight;
			float intensity = this.emission / 5f;
			this.artifactLight.intensity = intensity;
		}
	}

	private static bool GetPlaceInput()
	{
		if (ForestVR.Enabled)
		{
			return FirstPersonCharacter.GetDropInput();
		}
		return TheForest.Utils.Input.GetButtonDown("Craft");
	}

	private static bool GetRepelInput()
	{
		return TheForest.Utils.Input.GetButtonDown("AltFire");
	}

	private static bool GetAttractInput()
	{
		return TheForest.Utils.Input.GetButtonDown("Fire1");
	}

	private IEnumerator setArtifactStateRoutine(artifactBallController.artifactMode set)
	{
		this.doingSetArtifact = true;
		this._mode = set;
		if (set == artifactBallController.artifactMode.attract)
		{
			this.playerAnimator.SetIntegerReflected("artifactState", 1);
		}
		else if (set == artifactBallController.artifactMode.repel)
		{
			this.playerAnimator.SetIntegerReflected("artifactState", 1);
		}
		else
		{
			this.playerAnimator.SetIntegerReflected("artifactState", 1);
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		this.playerAnimator.SetIntegerReflected("artifactState", 0);
		if (set == artifactBallController.artifactMode.attract)
		{
			this.setArtifactState(1);
		}
		else if (set == artifactBallController.artifactMode.repel)
		{
			this.setArtifactState(2);
		}
		else if (set == artifactBallController.artifactMode.idle)
		{
			this.setArtifactState(0);
		}
		this.doingSetArtifact = false;
		yield return null;
		yield break;
	}

	private void setArtifactState(int mode)
	{
		if (mode == 1)
		{
			if (this.effigyEffectGo)
			{
				this.effigyEffectGo.SetActive(false);
			}
			this.currentStateIndex = 1;
			if (!this.remote)
			{
				this.playerAnimator.SetIntegerReflected("artifactStateMP", 1);
			}
			this.targetSpin = -10f;
			this.targetEmission = 1.8f;
			this.targetColor = this.attractColor;
			this.targetColorLight = this.attractColorLight;
			this.StopPowerOffEvent();
			this.StopPowerOnEvent();
			this.StopPoweredUpEvent();
			this.AffirmPowerOnEvent();
		}
		else if (mode == 2)
		{
			if (this.effigyEffectGo)
			{
				this.effigyEffectGo.SetActive(true);
			}
			if (!this.remote)
			{
				this.playerAnimator.SetIntegerReflected("artifactStateMP", 2);
			}
			this.currentStateIndex = 2;
			this.targetSpin = 10f;
			this.targetEmission = 1.8f;
			this.targetColor = this.repelColor;
			this.targetColorLight = this.repelColorLight;
			this.StopPowerOffEvent();
			this.StopPowerOnEvent();
			this.StopPoweredUpEvent();
			this.AffirmPowerOnEvent();
		}
		else if (mode == 0)
		{
			if (this.effigyEffectGo)
			{
				this.effigyEffectGo.SetActive(false);
			}
			if (!this.remote)
			{
				this.playerAnimator.SetIntegerReflected("artifactStateMP", 0);
			}
			this.currentStateIndex = 0;
			this.targetSpin = 1f;
			this.targetEmission = 0f;
			this.targetColor = this.idleColor;
			this.StopPowerOnEvent();
			this.StopPoweredUpEvent();
			this.AffirmPowerOffEvent();
		}
	}

	public void forceArtifactState(int setState)
	{
		if (setState == 1)
		{
			if (!this.remote)
			{
				this.playerAnimator.SetIntegerReflected("artifactStateMP", 1);
			}
			this._mode = artifactBallController.artifactMode.attract;
			this.baseColor = this.attractColor;
			this.baseColorLight = this.attractColorLight;
			this.spin = 5f;
			this.emission = 1.8f;
		}
		else if (setState == 2)
		{
			if (!this.remote)
			{
				this.playerAnimator.SetIntegerReflected("artifactStateMP", 2);
			}
			this._mode = artifactBallController.artifactMode.repel;
			this.baseColor = this.repelColor;
			this.baseColorLight = this.repelColorLight;
			this.spin = -5f;
			this.emission = 1.8f;
		}
		else
		{
			if (!this.remote)
			{
				this.playerAnimator.SetIntegerReflected("artifactStateMP", 0);
			}
			this._mode = artifactBallController.artifactMode.idle;
			this.baseColor = this.idleColor;
			this.spin = 0f;
			this.emission = 0.1f;
		}
		this.setArtifactState(setState);
	}

	private void SetEmissionColorProperty(Color setColor)
	{
		this.ballRenderer.GetPropertyBlock(this._ballPropertyBlock);
		this._ballPropertyBlock.SetColor("_EmissionColor", setColor);
		this.ballRenderer.SetPropertyBlock(this._ballPropertyBlock);
	}

	private void resetActivate()
	{
		this.playerAnimator.SetIntegerReflected("artifactState", 0);
	}

	private void resetPickup()
	{
		this.artifactAnimator.SetBool("pickup", false);
		this.playerAnimator.SetBool("firstTimePickup", false);
	}

	private void enableArtifactOpen()
	{
		this.artifactAnimator.SetBool("open", true);
	}

	private void resetOpen()
	{
		this.artifactAnimator.CrossFade(this.idleHash, 0f, 0);
		this.artifactAnimator.SetBool("pickupSpawn", false);
		this.artifactAnimator.SetBool("open", false);
	}

	private void AffirmPoweredUpEvent()
	{
		if (this.poweredUpInstance == null && !CoopPeerStarter.DedicatedHost)
		{
			this.poweredUpInstance = FMODCommon.PlayOneshot("event:/combat/weapons/artifact/artifact_dropped_loop", base.transform);
		}
	}

	private void AffirmPowerOnEvent()
	{
		if (this.powerOnInstance == null && !CoopPeerStarter.DedicatedHost)
		{
			this.powerOnInstance = FMODCommon.PlayOneshot("event:/combat/weapons/artifact/artifact_loop", base.transform);
		}
	}

	private void AffirmPowerOffEvent()
	{
		if (this.powerOffInstance == null && !CoopPeerStarter.DedicatedHost)
		{
			this.powerOffInstance = FMODCommon.PlayOneshot("event:/combat/weapons/artifact/artifact_turn_off", base.transform);
		}
	}

	private void StopPoweredUpEvent()
	{
		if (this.poweredUpInstance != null)
		{
			UnityUtil.ERRCHECK(this.poweredUpInstance.stop(STOP_MODE.ALLOWFADEOUT));
			this.poweredUpInstance.release();
			this.poweredUpInstance = null;
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

	private void StopPowerOffEvent()
	{
		if (this.powerOffInstance != null)
		{
			UnityUtil.ERRCHECK(this.powerOffInstance.stop(STOP_MODE.IMMEDIATE));
			this.powerOffInstance.release();
			this.powerOffInstance = null;
		}
	}

	[ItemIdPicker]
	public int _artifactId;

	private artifactIconSetup iconSetup;

	public MeshRenderer ballRenderer;

	public Animator playerAnimator;

	public GameObject effigyEffectGo;

	public Light artifactLight;

	private artifactBallEffectSetup effectSetup;

	private Animator artifactAnimator;

	private bool doingSetArtifact;

	private bool doingPlaceArtifact;

	private float effectUpdateRate;

	private float targetSpin;

	private float targetEmission;

	public float emission;

	public float spin;

	public int currentStateIndex;

	private Material mat;

	public Color baseColor = Color.white;

	private Color targetColor;

	public Color attractColor;

	public Color repelColor;

	public Color idleColor;

	public Color attractColorLight;

	public Color repelColorLight;

	private Color baseColorLight;

	private Color targetColorLight;

	private MaterialPropertyBlock _ballPropertyBlock;

	private EventInstance poweredUpInstance;

	private EventInstance powerOnInstance;

	private EventInstance powerOffInstance;

	private int artifactIdleHash = Animator.StringToHash("artifactIdle");

	private int artifactPutDownHash = Animator.StringToHash("artifactPutDown");

	private int idleHash = Animator.StringToHash("idle");

	public bool remote;

	public bool placedArtifact;

	public artifactBallController.artifactMode _mode;

	public enum artifactMode
	{
		idle,
		attract,
		repel
	}
}
