using System;
using FMOD.Studio;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class chainSawAttackSetup : MonoBehaviour
{
	
	private void Start()
	{
		this.sawCollider = base.transform.GetComponent<Collider>();
		if (!this.netPrefab)
		{
			this.sawCollider.enabled = false;
			this.mainCollider.enabled = false;
		}
		this.attackTimer = Time.time + 0.05f;
		this.chainSpeed = 0f;
		this.active = (this.netPrefab || LocalPlayer.Stats.Fuel.CurrentFuel > 0f);
	}

	
	private void OnEnable()
	{
		if (!this.netPrefab)
		{
			LocalPlayer.Inventory.StashLeftHand();
		}
		if (!this.sawCollider)
		{
			this.sawCollider = base.transform.GetComponent<Collider>();
		}
	}

	
	private void AffirmEventStarted()
	{
		if (this.eventInstance == null && !CoopPeerStarter.DedicatedHost)
		{
			this.eventInstance = FMODCommon.PlayOneshot("event:/combat/chainsaw/chainsaw", base.transform);
			if (this.eventInstance != null)
			{
				this.eventInstance.getParameter("cutting", out this.cuttingParameter);
				this.eventInstance.getParameter("gore", out this.cuttingFleshParameter);
				this.eventInstance.getParameter("wood", out this.cuttingTreeParameter);
				this.SetCuttingParameter(0f);
				this.setCuttingFleshParameter(0f);
				this.setCuttingTreeParameter(0f);
			}
		}
	}

	
	private void StopEvent()
	{
		if (this.eventInstance != null)
		{
			this.SetCuttingParameter(0f);
			this.setCuttingFleshParameter(0f);
			this.setCuttingTreeParameter(0f);
			UnityUtil.ERRCHECK(this.eventInstance.stop(STOP_MODE.ALLOWFADEOUT));
			this.eventInstance = null;
			this.cuttingParameter = null;
			this.cuttingFleshParameter = null;
			this.cuttingTreeParameter = null;
		}
	}

	
	private void SetCuttingParameter(float value)
	{
		if (this.cuttingParameter != null)
		{
			UnityUtil.ERRCHECK(this.cuttingParameter.setValue(value));
		}
	}

	
	private void setCuttingFleshParameter(float value)
	{
		if (this.cuttingFleshParameter != null)
		{
			UnityUtil.ERRCHECK(this.cuttingFleshParameter.setValue(value));
		}
	}

	
	private void setCuttingTreeParameter(float value)
	{
		if (this.cuttingTreeParameter != null)
		{
			UnityUtil.ERRCHECK(this.cuttingTreeParameter.setValue(value));
		}
	}

	
	private void Update()
	{
		float to = 0.2f;
		AnimatorStateInfo currentAnimatorStateInfo = this.playerAnimator.GetCurrentAnimatorStateInfo(1);
		AnimatorStateInfo currentAnimatorStateInfo2 = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
		if (currentAnimatorStateInfo.shortNameHash == this.toIdleHash)
		{
			if (!this.netPrefab)
			{
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			}
			if (currentAnimatorStateInfo.normalizedTime > 0.5f)
			{
				to = 7f;
			}
			this.chainSawAnimator.CrossFade("Base Layer.toIdle", 0f, 0, currentAnimatorStateInfo.normalizedTime);
			if (!this.netPrefab)
			{
				this.playerAnimator.SetBool("cancelCheckArms", true);
				this.playerAnimator.SetBool("checkArms", false);
			}
			this.syncIdle = true;
		}
		else
		{
			this.syncIdle = false;
			if (!this.netPrefab)
			{
				this.playerAnimator.SetBool("cancelCheckArms", false);
			}
		}
		if (this.active)
		{
			if (currentAnimatorStateInfo2.shortNameHash == this.sawTreeHash || currentAnimatorStateInfo.shortNameHash == this.sawAttackHash)
			{
				if (Time.time > this.attackTimer && !this.netPrefab)
				{
					if (this.sawCollider.enabled)
					{
						this.sawCollider.enabled = false;
						if (this.mainCollider)
						{
							this.mainCollider.enabled = true;
						}
					}
					else
					{
						this.sawCollider.enabled = true;
						if (this.mainCollider)
						{
							this.mainCollider.enabled = false;
						}
					}
					if (currentAnimatorStateInfo.shortNameHash == this.sawAttackHash)
					{
						this.attackTimer = Time.time + 0.25f;
					}
					else
					{
						this.attackTimer = Time.time + 0.05f;
					}
				}
				to = 7f;
			}
			else
			{
				if (!this.netPrefab)
				{
					this.sawCollider.enabled = false;
				}
				if (this.mainCollider)
				{
					this.mainCollider.enabled = false;
				}
			}
			if (currentAnimatorStateInfo.shortNameHash == this.idleHash || currentAnimatorStateInfo.shortNameHash == this.toIdleHash)
			{
				if (this.netPrefab && this.netActive)
				{
					this.AffirmEventStarted();
				}
				else if (!this.netPrefab)
				{
					this.AffirmEventStarted();
				}
			}
			if (this.eventInstance != null)
			{
				if (currentAnimatorStateInfo2.shortNameHash == this.sawTreeHash || currentAnimatorStateInfo2.shortNameHash == this.toSawTreeHash || currentAnimatorStateInfo.shortNameHash == this.sawAttackHash || currentAnimatorStateInfo.shortNameHash == this.toSawAttackHash)
				{
					if (!this.cuttingObject)
					{
						this.SetCuttingParameter(0.5f);
					}
					if (this.cuttingObject)
					{
						this.SetCuttingParameter(0.8f);
						this.setCuttingTreeParameter(0.5f);
						if (this.cuttingFlesh)
						{
							this.setCuttingTreeParameter(0f);
							this.setCuttingFleshParameter(0.5f);
						}
					}
				}
				else
				{
					this.cuttingObject = false;
					this.cuttingFlesh = false;
					this.cuttingTree = false;
					this.SetCuttingParameter(0f);
					this.setCuttingFleshParameter(0f);
					this.setCuttingTreeParameter(0f);
				}
				UnityUtil.ERRCHECK(this.eventInstance.set3DAttributes(base.transform.to3DAttributes()));
			}
		}
		if (!this.netPrefab)
		{
			bool flag = LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World;
			if (LocalPlayer.Stats.Fuel.CurrentFuel <= 0f)
			{
				to = 0f;
				if (this.active)
				{
					this.active = false;
					this.StopEvent();
					this.sawCollider.enabled = false;
					this.playerAnimator.SetFloat("fuel", 0f);
					this.playerAnimator.SetBool("attack", false);
					this.playerAnimator.SetBool("sawAttack", false);
					if (this.smokeGo)
					{
						this.smokeGo.SetActive(false);
					}
				}
				LocalPlayer.Inventory.SetReloadDelay(1f);
				if (Scene.HudGui.FuelReserveEmpty.activeSelf != flag)
				{
					Scene.HudGui.FuelReserveEmpty.SetActive(flag);
				}
				Scene.HudGui.FuelReserve.fillAmount = 0f;
				Mood.StopConstantVibratation();
			}
			else
			{
				if (!this.active)
				{
					this.active = true;
				}
				LocalPlayer.Inventory.CancelReloadDelay();
				if (!this.netPrefab)
				{
					this.playerAnimator.SetFloat("fuel", 1f);
				}
				if (this.smokeGo)
				{
					this.smokeGo.SetActive(true);
				}
				if (this.mainCollider.enabled)
				{
					LocalPlayer.Stats.Fuel.CurrentFuel -= Time.deltaTime * 3f;
					Mood.BeginConstantVibratation(this.VibrateForceAttack, this.VibrateForceAttack);
				}
				else
				{
					Mood.BeginConstantVibratation(this.VibrateForceIdle, this.VibrateForceIdle);
				}
				Scene.HudGui.FuelReserve.fillAmount = LocalPlayer.Stats.Fuel.CurrentFuel / LocalPlayer.Stats.Fuel.MaxFuelCapacity;
				if (Scene.HudGui.FuelReserveEmpty.activeSelf)
				{
					Scene.HudGui.FuelReserveEmpty.SetActive(false);
				}
			}
			if (Scene.HudGui.FuelReserve.gameObject.activeSelf != flag)
			{
				Scene.HudGui.FuelReserve.gameObject.SetActive(flag);
			}
		}
		if (this.netPrefab)
		{
			if (this.playerAnimator.GetFloat("fuel") <= 0.1f)
			{
				to = 0f;
				if (this.netActive)
				{
					this.StopEvent();
					if (this.smokeGo)
					{
						this.smokeGo.SetActive(false);
					}
					this.netActive = false;
				}
			}
			else if (!this.netActive)
			{
				if (this.smokeGo)
				{
					this.smokeGo.SetActive(true);
				}
				this.netActive = true;
			}
		}
		this.chainSpeed = Mathf.Lerp(this.chainSpeed, to, Time.deltaTime * 2f);
		this.chainSawAnimator.SetFloat("chainSpeed", this.chainSpeed);
	}

	
	private void OnTriggerEnter(Collider other)
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.playerAnimator.GetCurrentAnimatorStateInfo(1);
		AnimatorStateInfo currentAnimatorStateInfo2 = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
		if ((currentAnimatorStateInfo2.shortNameHash == this.toSawTreeHash && other.CompareTag("Tree")) || (currentAnimatorStateInfo2.shortNameHash == this.sawTreeHash && (other.CompareTag("Tree") || other.CompareTag("MidTree") || other.GetComponent<TreeWindSfx>())))
		{
			this.cuttingObject = true;
		}
		if ((currentAnimatorStateInfo.shortNameHash == this.sawAttackHash && other.CompareTag("enemyCollide")) || (currentAnimatorStateInfo.shortNameHash == this.toSawAttackHash && other.CompareTag("enemyCollide")))
		{
			this.cuttingObject = true;
			this.cuttingFlesh = true;
		}
	}

	
	private void OnDisable()
	{
		this.StopEvent();
		if (!this.netPrefab)
		{
			if (this.mainCollider)
			{
				this.mainCollider.enabled = false;
			}
			if (this.sawCollider)
			{
				this.sawCollider.enabled = false;
			}
			LocalPlayer.Inventory.CancelReloadDelay();
			if (Scene.HudGui)
			{
				if (Scene.HudGui.FuelReserve.gameObject.activeSelf)
				{
					Scene.HudGui.FuelReserve.gameObject.SetActive(false);
				}
				if (Scene.HudGui.FuelReserveEmpty.activeSelf)
				{
					Scene.HudGui.FuelReserveEmpty.SetActive(false);
				}
			}
			Mood.StopConstantVibratation();
		}
	}

	
	public Animator playerAnimator;

	
	public Animator chainSawAnimator;

	
	private Collider sawCollider;

	
	public Collider mainCollider;

	
	public GameObject smokeGo;

	
	public float VibrateForceIdle = 0.15f;

	
	public float VibrateForceAttack = 1f;

	
	private float chainSpeed;

	
	private int idleHash = Animator.StringToHash("chainSawIdle");

	
	private int sawTreeHash = Animator.StringToHash("treeSaw");

	
	private int sawAttackHash = Animator.StringToHash("chainSawAttack");

	
	private int toIdleHash = Animator.StringToHash("toChainsawIdle");

	
	private int toSawTreeHash = Animator.StringToHash("toTreeSaw");

	
	private int toSawAttackHash = Animator.StringToHash("toChainSawAttack");

	
	private float attackTimer;

	
	private bool syncIdle;

	
	private bool cuttingObject;

	
	private bool cuttingFlesh;

	
	private bool cuttingTree;

	
	private bool active;

	
	private bool netActive;

	
	public bool netPrefab;

	
	private EventInstance eventInstance;

	
	private ParameterInstance cuttingParameter;

	
	private ParameterInstance cuttingFleshParameter;

	
	private ParameterInstance cuttingTreeParameter;
}
