using System;
using Bolt;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;


public class visRangeSetup : EntityBehaviour<IPlayerState>
{
	
	public override void Attached()
	{
		if (!base.entity.IsOwner())
		{
			base.state.AddCallback("TreeDensity", new PropertyCallbackSimple(this.TreeDensityChanged));
			base.state.AddCallback("isTargetting", new PropertyCallbackSimple(this.isTargettingChanged));
		}
	}

	
	private void TreeDensityChanged()
	{
		this.treeDensity = base.state.TreeDensity;
	}

	
	private void isTargettingChanged()
	{
		this.currentlyTargetted = base.state.isTargetting;
	}

	
	private void Awake()
	{
	}

	
	private void Start()
	{
		if (this.host)
		{
			this.tf = LocalPlayer.ScriptSetup.targetFunctions;
			if (!base.IsInvoking("updateCloseTrees"))
			{
				base.InvokeRepeating("updateCloseTrees", 2f, 2f);
			}
		}
		if (!base.IsInvoking("updateVisParams"))
		{
			base.InvokeRepeating("updateVisParams", 2f, 0.65f);
		}
		if (!BoltNetwork.isClient && !base.IsInvoking("updateTarget"))
		{
			base.InvokeRepeating("updateTarget", 2f, 1.5f);
		}
	}

	
	private void updateCloseTrees()
	{
		this.treeCount = 0f;
		if (Scene.SceneTracker == null)
		{
			return;
		}
		if (Scene.SceneTracker.allPlayersInCave.Contains(base.gameObject))
		{
			this.treeCount = 9f;
		}
		else
		{
			if (Scene.SceneTracker.closeTrees.Count > 0)
			{
				Scene.SceneTracker.closeTrees.RemoveAll((GameObject o) => o == null);
			}
			float num = float.PositiveInfinity;
			for (int i = 0; i < Scene.SceneTracker.closeTrees.Count; i++)
			{
				float sqrMagnitude = (Scene.SceneTracker.closeTrees[i].transform.position - base.transform.position).sqrMagnitude;
				if (sqrMagnitude < this.testDist * this.testDist)
				{
					this.treeCount += 1f;
				}
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					this.closestTree = Scene.SceneTracker.closeTrees[i];
				}
			}
		}
	}

	
	private void updateTarget()
	{
		this.isTarget = false;
		base.Invoke("checkCurrentlyTargetted", 1f);
	}

	
	private void checkCurrentlyTargetted()
	{
		if (!this.isTarget)
		{
			this.currentlyTargetted = false;
		}
		else
		{
			this.currentlyTargetted = true;
		}
	}

	
	private void updateVisParams()
	{
		if (Scene.Atmosphere == null)
		{
			return;
		}
		if (this.host)
		{
			if (Scene.Atmosphere.TimeOfDay > 50f && Scene.Atmosphere.TimeOfDay < 270f)
			{
				this.amountOfLight = 1f - (Scene.Atmosphere.TimeOfDay - 50f) / 40f / 2f;
			}
			else if (Scene.Atmosphere.TimeOfDay > 270f && Scene.Atmosphere.TimeOfDay < 310f)
			{
				this.amountOfLight = (Scene.Atmosphere.TimeOfDay - 270f) / 40f / 2f;
			}
			else
			{
				this.amountOfLight = 1f;
			}
			if (LocalPlayer.IsInCaves)
			{
				this.amountOfLight = 0.65f;
			}
			this.amountOfLight = Mathf.Clamp(this.amountOfLight, 0.5f, 1f);
			if (LocalPlayer.FpCharacter.crouching)
			{
				this.crouchOffset = 40f * (1.5f - this.amountOfLight);
			}
			else
			{
				this.crouchOffset = 0f;
			}
			if (LocalPlayer.Inventory.IsWeaponBurning)
			{
				this.litWeaponOffset = 70f * this.amountOfLight;
			}
			else
			{
				this.litWeaponOffset = 0f;
			}
			this.stealthFactor = 1f - LocalPlayer.Stats.Stealth * GameSettings.Survival.StealthRatio / 75f;
			if ((double)LocalPlayer.AnimControl.overallSpeed > 0.5)
			{
				this.movementPenalty = 35f;
			}
			else
			{
				this.movementPenalty = 0f;
			}
			float num = 100f - this.crouchOffset - this.bushOffset + this.movementPenalty;
			float num2 = Mathf.Clamp(this.treeCount, 0f, 12f);
			num2 /= 12f;
			num2 = 1f - num2;
			num2 = Mathf.Clamp(num2, 0.4f, 0.8f);
			this.modVisRange = num * num2 * this.offsetFactor * this.stealthFactor * this.amountOfLight + this.litWeaponOffset + this.tf.lighterRange;
			this.modVisRange = Mathf.Clamp(this.modVisRange, 4f, 100f);
			this.unscaledVisRange = this.modVisRange;
		}
		else
		{
			this.modVisRange = this.treeDensity;
		}
		if (BoltNetwork.isRunning && !this.host)
		{
			this.unscaledVisRange = this.treeDensity;
		}
		if (BoltNetwork.isRunning)
		{
			if (base.entity != null && base.entity.isAttached)
			{
				if (base.entity.IsOwner())
				{
					this.treeDensity = this.modVisRange;
					if (this.treeDensity != base.state.TreeDensity)
					{
						base.state.TreeDensity = this.modVisRange;
					}
				}
				else if (this.isTarget != base.state.isTargetting)
				{
					base.state.isTargetting = this.currentlyTargetted;
				}
			}
		}
		else
		{
			this.treeDensity = this.modVisRange;
		}
	}

	
	private void LateUpdate()
	{
		this.bushOffset = 0f;
	}

	
	private void OnTriggerStay(Collider other)
	{
		if (!other || !this.host || !LocalPlayer.GameObject)
		{
			return;
		}
		if (other.gameObject.CompareTag("SmallTree"))
		{
			if (LocalPlayer.FpCharacter.crouching)
			{
				this.bushOffset = 50f;
			}
			else
			{
				this.bushOffset = 20f;
			}
		}
	}

	
	public float treeDensity = 1f;

	
	private float bushOffset;

	
	public float unscaledVisRange;

	
	public float modVisRange;

	
	public float justTreeDensity;

	
	private float stealthFactor;

	
	private float darkFactor = 0.75f;

	
	private float litWeaponOffset;

	
	private float movementPenalty;

	
	private float crouchOffset;

	
	public bool host;

	
	public float treeCount;

	
	public float testDist = 30f;

	
	public float offsetFactor = 1f;

	
	public bool isTarget;

	
	public bool currentlyTargetted;

	
	public float amountOfLight;

	
	private playerTargetFunctions tf;

	
	private int frameOffset;

	
	public int frameInterval = 10;

	
	public GameObject closestTree;
}
