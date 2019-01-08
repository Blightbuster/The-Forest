using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Items.World;
using TheForest.Utils;
using UnityEngine;

public class treeHitTrigger : MonoBehaviour
{
	private void Start()
	{
		this.setup = base.transform.root.GetComponentInChildren<playerScriptSetup>();
		this.animator = this.setup.playerBase.GetComponent<Animator>();
		this.rootTr = LocalPlayer.PlayerBase.transform;
		this.axeHeight = 1f;
		this.fsmLookAtTree = this.setup.pmControl.FsmVariables.GetFsmBool("lookAtTree");
		this.cutThreshold = 4.5f;
		this.fsmAxeSmashAngle = this.setup.pmControl.FsmVariables.GetFsmFloat("axeSmashAngle");
	}

	private void Update()
	{
		if (!PlayerPreferences.LowQualityPhysics)
		{
			this.HandleTreeAdjustements();
		}
	}

	private void HandleTreeAdjustements()
	{
		if (LocalPlayer.PlayerBase == null || Terrain.activeTerrain == null)
		{
			return;
		}
		Vector3 vector = LocalPlayer.PlayerBase.transform.position + LocalPlayer.Transform.forward * 1.5f;
		if (LocalPlayer.AnimControl.doShellRideMode)
		{
			vector = LocalPlayer.PlayerBase.transform.position;
		}
		this.heightFromTerrain = Terrain.activeTerrain.SampleHeight(LocalPlayer.PlayerBase.transform.position) + Terrain.activeTerrain.transform.position.y;
		this.heightFromTerrain = LocalPlayer.PlayerBase.transform.position.y - this.heightFromTerrain;
		this.tx = (vector.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
		this.tz = (vector.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
		this.tNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(this.tx, this.tz);
		float num = Vector3.Angle(this.tNormal, LocalPlayer.Transform.forward);
		num = (num - 90f) / 30f;
		num = Mathf.Clamp(num, -1f, 1f);
		this.TerrainAngleToPlayer = num;
		float num2 = 0.43f;
		if (num < 0f && this.heightFromTerrain < 0.5f && this.heightFromTerrain > -1f)
		{
			num2 += num * -0.2f;
		}
		this.fsmAxeSmashAngle.Value = num2;
		if (!this.atStump)
		{
			if (this.heightFromTerrain < 0.5f && this.heightFromTerrain > -1f)
			{
				this.animator.SetFloatReflected("axeBlend1", num);
			}
			else
			{
				this.animator.SetFloatReflected("axeBlend1", 0f, 0.5f, Time.deltaTime * 2f);
			}
		}
		if (this.atTree && !this.currTree)
		{
			this.forceExit();
		}
		if (this.currTreeCheck && !this.currTree)
		{
			this.forceExit();
			this.currTreeCheck = false;
		}
		if (this.animator.GetBool("deathBool"))
		{
			this.forceExit();
		}
		if (this.treeTriggers.Count == 0)
		{
			this.forceExit();
		}
		if (!this.inAnyTreeTrigger)
		{
			this.forceExit();
		}
	}

	private void FixedUpdate()
	{
		if (PlayerPreferences.LowQualityPhysics)
		{
			this.HandleTreeAdjustements();
		}
		this.inAnyTreeTrigger = false;
		if (!this.atStump)
		{
			if (this.animator.GetFloat("terrainAngle") < 0.2f)
			{
				this.animator.SetFloatReflected("axeBlend1", 0f, 0.5f, Time.deltaTime * 2f);
			}
			this.animator.SetFloatReflected("terrainAngle", 1f, 0.5f, Time.deltaTime * 2f);
		}
		if (this.currEnemyCheck)
		{
			this.atEnemy = true;
		}
		else
		{
			this.atEnemy = false;
		}
		this.atStump = false;
		this.currEnemyCheck = false;
	}

	private void forceExit()
	{
		this.animator.SetBoolReflected("bigTree", false);
		this.animator.SetFloatReflected("weaponHit", 0f);
		this.fsmLookAtTree.Value = false;
		this.currTree = null;
		this.currTreeCollider = null;
		this.treeTriggers.Clear();
		this.atTree = false;
		if (this.startedAxeAdjust)
		{
			base.StopCoroutine("setAxeHeight");
			this.startedAxeAdjust = false;
		}
	}

	private void resetWeaponHit()
	{
		this.animator.SetBoolReflected("bigTree", false);
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("FireTrigger"))
		{
			Fire2 component = other.GetComponent<Fire2>();
			if (component && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).shortNameHash == this.drawBowIdleHash)
			{
				if (component.CurrentLit)
				{
					float num = 1.8f;
					if (component.isBonfire)
					{
						num = 4.4f;
					}
					float num2;
					if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._bowRecurveId))
					{
						num2 = Vector3.Distance(other.transform.position, LocalPlayer.ScriptSetup.heldModernBowTip.transform.position);
					}
					else
					{
						num2 = Vector3.Distance(other.transform.position, LocalPlayer.ScriptSetup.heldBowTip.transform.position);
					}
					if (num2 < num)
					{
						if (Time.time > this.lightArrowTimer)
						{
							BowController component2;
							if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._bowRecurveId))
							{
								component2 = LocalPlayer.ScriptSetup.heldModernBow.GetComponent<BowController>();
							}
							else
							{
								component2 = LocalPlayer.ScriptSetup.heldBow.GetComponent<BowController>();
							}
							if (component2 && component2._activeAmmoBonus != WeaponStatUpgrade.Types.BurningAmmo && component2._activeFireArrowGO == null)
							{
								component2.gameObject.SendMessage("LightArrow");
							}
						}
					}
					else
					{
						this.lightArrowTimer = Time.time + 1.7f;
					}
				}
			}
			else
			{
				this.lightArrowTimer = Time.time + 1.7f;
			}
		}
		else if (other.gameObject.CompareTag("jumpObject") || other.gameObject.CompareTag("UnderfootWood"))
		{
			ExplodeTreeStump component3 = other.transform.GetComponent<ExplodeTreeStump>();
			if (component3)
			{
				this.atStump = true;
				this.animator.SetFloatReflected("terrainAngle", 0f, 0.5f, Time.deltaTime * 2f);
				this.animator.SetFloatReflected("axeBlend1", 1f, 0.5f, Time.deltaTime * 2f);
			}
		}
		else if (other.gameObject.CompareTag("animalCollide") || other.gameObject.CompareTag("Shell"))
		{
			animalId component4 = other.transform.GetComponent<animalId>();
			if (component4)
			{
				this.atStump = true;
				this.animator.SetFloatReflected("terrainAngle", 0f, 0.5f, Time.deltaTime * 2f);
				this.animator.SetFloatReflected("axeBlend1", -1f, 0.5f, Time.deltaTime * 2f);
			}
		}
		else if (other.gameObject.CompareTag("Float"))
		{
			this.atStump = true;
			this.animator.SetFloatReflected("terrainAngle", 0f, 0.5f, Time.deltaTime * 2f);
			this.animator.SetFloatReflected("axeBlend1", -0.5f, 0.5f, Time.deltaTime * 2f);
		}
		else if (other.gameObject.CompareTag("enemyCollide"))
		{
			this.currEnemyCheck = true;
			if (Vector3.Distance(LocalPlayer.Transform.position, other.transform.position) < 5f)
			{
				this.forceExit();
				return;
			}
		}
		else if (other.gameObject.CompareTag("treeTrigger") || other.gameObject.CompareTag("Tree"))
		{
			this.inAnyTreeTrigger = true;
			if (!this.treeTriggers.Contains(other))
			{
				this.inTreeCounter++;
				this.treeTriggers.Add(other);
				this.lastTreeCollider = other;
			}
			this.currTree = other.gameObject;
			this.currTreeCheck = true;
			getAxePos component5 = other.GetComponent<getAxePos>();
			if (component5)
			{
				if (component5.ants != null)
				{
					component5.ants.SetActive(true);
				}
				this.treeTriggerPos = component5.axePosTr.position;
			}
			else
			{
				this.treeTriggerPos = this.currTree.transform.position;
			}
			this.setup.pmControl.FsmVariables.GetFsmVector3("lookAtTreePos").Value = other.transform.position;
			this.atTree = true;
			this.treeTriggers.RemoveAll((Collider o) => o == null);
			if (this.treeTriggers.Count > 1)
			{
				this.treeTriggers.Sort((Collider c1, Collider c2) => (base.transform.position - new Vector3(c1.transform.position.x, base.transform.position.y, c1.transform.position.z)).sqrMagnitude.CompareTo((base.transform.position - new Vector3(c2.transform.position.x, base.transform.position.y, c2.transform.position.z)).sqrMagnitude));
			}
			this.treePos = new Vector3(this.treeTriggers[0].transform.position.x, base.transform.position.y, this.treeTriggers[0].transform.position.z);
			Vector3 position = LocalPlayer.PlayerBase.transform.position;
			position.y = base.transform.position.y;
			this.distance = Vector3.Distance(position, this.treePos);
			if (this.distance < this.cutThreshold)
			{
				if (!this.startedAxeAdjust)
				{
					base.StartCoroutine("setAxeHeight");
					this.startedAxeAdjust = true;
				}
				this.fsmLookAtTree.Value = true;
				this.animator.SetBoolReflected("bigTree", true);
				this.animator.SetFloatReflected("weaponHit", 1f);
			}
			else
			{
				this.fsmLookAtTree.Value = false;
				this.forceExit();
				this.animator.SetBoolReflected("bigTree", false);
				this.animator.SetFloatReflected("weaponHit", 0f);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("treeTrigger") || other.gameObject.CompareTag("Tree"))
		{
			if (this.treeTriggers.Contains(other))
			{
				this.inTreeCounter--;
				this.treeTriggers.Remove(other);
			}
			if (this.treeTriggers.Count == 0)
			{
				this.fsmLookAtTree.Value = false;
				this.forceExit();
			}
		}
	}

	private IEnumerator setAxeHeight()
	{
		for (;;)
		{
			this.relPos = this.rootTr.InverseTransformPoint(this.treeTriggerPos);
			float axeVal = (this.relPos.y - this.heightOffset) * this.multAxeHeight;
			this.axeHeightVal = Mathf.Lerp(this.axeHeightVal, axeVal, Time.deltaTime * 5f);
			if (this.animator)
			{
				this.animator.SetFloatReflected("aimAtTarget", this.axeHeightVal);
			}
			yield return YieldPresets.WaitForFixedUpdate;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	private float map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}

	private PlayMakerFSM pmControl;

	private Animator animator;

	private playerScriptSetup setup;

	public GameObject currTree;

	public Collider currTreeCollider;

	private Transform rootTr;

	private Vector3 treeTriggerPos;

	public bool atTree;

	public bool inAnyTreeTrigger;

	public bool atStump;

	public bool atEnemy;

	public Vector3 relPos;

	public float heightDiff;

	public float axeHeight;

	public float heightFactor;

	public float distance;

	private float multAxeHeight = 1f;

	private float heightOffset = 2.52f;

	private float cutThreshold = 5f;

	private Vector3 treePos;

	private bool currTreeCheck;

	private bool startedAxeAdjust;

	private bool currEnemyCheck;

	private float tx;

	private float tz;

	public float heightFromTerrain;

	public float terrainAngleNormalized;

	private Vector3 tNormal;

	private FsmBool fsmLookAtTree;

	public FsmFloat fsmAxeSmashAngle;

	public float TerrainAngleToPlayer;

	public List<Collider> treeTriggers = new List<Collider>();

	private Collider lastTreeCollider;

	private int inTreeCounter;

	public bool atFire;

	private float lightArrowTimer;

	private int drawBowIdleHash = Animator.StringToHash("drawBowIdle");

	private float axeHeightVal;
}
