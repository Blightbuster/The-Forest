using System;
using Bolt;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UniLinq;
using UnityEngine;


public class EatCooked : EntityBehaviour
{
	
	protected virtual void Awake()
	{
		this.GrabExit();
	}

	
	protected virtual void Update()
	{
		if (this.ShowIconsPastPress && !this.MyPickUp.activeSelf && TheForest.Utils.Input.IsPastButtonPress("Take"))
		{
			this.ToggleIcons(false);
			if (this.DisablePastPress)
			{
				this.DisablePastPress.GrabExit();
			}
		}
		if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false))
		{
			LocalPlayer.Sfx.PlayWhoosh();
			if (!this.IsWater)
			{
				Cook componentInParent = base.GetComponentInParent<Cook>();
				int num = (int)(600f * this.Size);
				if (componentInParent)
				{
					Item item = ItemDatabase.ItemById(componentInParent._itemId);
					if (item != null)
					{
						if (item._usedStatEffect.Any((StatEffect use) => use._type == StatEffect.Types.EatenCalories))
						{
							num = (int)item._usedStatEffect.First((StatEffect use) => use._type == StatEffect.Types.EatenCalories)._amount;
						}
					}
				}
				if (!this.Burnt)
				{
					LocalPlayer.SpecialActions.SendMessage("eatMeatRoutine", false, SendMessageOptions.DontRequireReceiver);
					switch ((!componentInParent) ? DecayingInventoryItemView.DecayStates.Fresh : componentInParent._decayState)
					{
					case DecayingInventoryItemView.DecayStates.None:
					case DecayingInventoryItemView.DecayStates.Fresh:
					case DecayingInventoryItemView.DecayStates.DriedFresh:
						LocalPlayer.Stats.AteFreshMeat(this.IsLimb, this.Size, num);
						break;
					case DecayingInventoryItemView.DecayStates.Edible:
					case DecayingInventoryItemView.DecayStates.DriedEdible:
						LocalPlayer.Stats.AteEdibleMeat(this.IsLimb, this.Size, num);
						break;
					case DecayingInventoryItemView.DecayStates.Spoilt:
					case DecayingInventoryItemView.DecayStates.DriedSpoilt:
						LocalPlayer.Stats.AteSpoiltMeat(this.IsLimb, this.Size, num);
						break;
					}
					if (this.Dried)
					{
						LocalPlayer.Stats.Thirst += 0.05f * GameSettings.Survival.DriedMeatThirstRatio;
					}
				}
				else
				{
					LocalPlayer.SpecialActions.SendMessage("eatMeatRoutine", true, SendMessageOptions.DontRequireReceiver);
					LocalPlayer.Stats.AteBurnt(this.IsLimb, this.Size, num / 2);
					LocalPlayer.Stats.Thirst += 0.1f * GameSettings.Survival.DriedMeatThirstRatio;
				}
				if (this._remainsYield > 0)
				{
					LocalPlayer.Inventory.AddItem(this._remainsItemId, this._remainsYield, true, false, null);
				}
			}
			else
			{
				LocalPlayer.Sfx.PlayDrink();
				LocalPlayer.Stats.Thirst = 0f;
			}
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
			{
				if (base.entity.isOwner)
				{
					BoltNetwork.Destroy(base.entity);
				}
				else
				{
					RequestDestroy requestDestroy = RequestDestroy.Create(GlobalTargets.Everyone);
					requestDestroy.Entity = base.entity;
					requestDestroy.Send();
				}
			}
			else
			{
				UnityEngine.Object.Destroy(base.transform.parent.gameObject);
			}
		}
	}

	
	public void GrabEnter()
	{
		this.ToggleIcons(false);
		base.enabled = true;
	}

	
	public void GrabExit()
	{
		this.ToggleIcons(true);
		base.enabled = false;
	}

	
	protected virtual void ToggleIcons(bool sheen)
	{
		if (sheen)
		{
			if (this.MyPickUp.activeSelf)
			{
				this.MyPickUp.SetActive(false);
			}
			if (this.Sheen && !this.Sheen.activeSelf)
			{
				this.Sheen.SetActive(true);
			}
		}
		else
		{
			if (!this.MyPickUp.activeSelf && !this.ShowIconsPastPress)
			{
				this.MyPickUp.SetActive(true);
			}
			if (this.Sheen && this.Sheen.activeSelf)
			{
				this.Sheen.SetActive(false);
			}
		}
	}

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public bool IsWater;

	
	public bool IsLimb;

	
	public float Size = 1f;

	
	public bool Dried;

	
	public bool Burnt;

	
	public bool ShowIconsPastPress;

	
	public PickUp DisablePastPress;

	
	[ItemIdPicker]
	public int _remainsItemId;

	
	public int _remainsYield;
}
