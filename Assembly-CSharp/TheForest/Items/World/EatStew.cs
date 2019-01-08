using System;
using Bolt;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	public class EatStew : EatCooked
	{
		protected override void Awake()
		{
			base.Awake();
			if (this._fullness < 0.01f && this._hydration < 0.01f && (!BoltNetwork.isRunning || !base.entity.isAttached || base.entity.isOwner))
			{
				UnityEngine.Object.Destroy(base.transform.parent.gameObject);
			}
		}

		protected override void Update()
		{
			this.ToggleIcons(false);
			if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false))
			{
				if (this.CanGather)
				{
					LocalPlayer.Inventory.GatherWater(true);
					LocalPlayer.Sfx.PlayTwinkle();
				}
				else if ((double)this._fullness > 0.01)
				{
					LocalPlayer.Sfx.PlayWhoosh();
					if (this._meats > 0)
					{
					}
					int calories = this._meats * 600 + this._mushrooms * 10 + this._herbs * 5;
					StewCombo.SetIngredients(this._meats, this._mushrooms, this._herbs, this._hydration > 0.01f);
					LocalPlayer.Stats.AteCustom(this._fullness * StewCombo.FullnessRatio, this._health * StewCombo.HealthRatio, this._energy * StewCombo.EnergyRatio, this._isFresh > 0f, this._meats > 0, this.IsLimb, calories);
					if (this._remainsYield > 0)
					{
						LocalPlayer.Inventory.AddItem(this._remainsItemId, this._remainsYield, true, false, null);
					}
					if (this._hydration > 0.01f)
					{
						LocalPlayer.Sfx.PlayDrink();
						LocalPlayer.Stats.Thirst = Mathf.Clamp01(LocalPlayer.Stats.Thirst - this._hydration);
					}
				}
				else if (this._hydration > 0.01f)
				{
					LocalPlayer.Sfx.PlayDrink();
					LocalPlayer.Stats.Thirst = Mathf.Clamp01(LocalPlayer.Stats.Thirst - this._hydration);
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

		protected override void ToggleIcons(bool sheen)
		{
			bool canGather = this.CanGather;
			bool flag = this._fullness > 0.01f;
			bool flag2 = !this.CanGather && !flag;
			if (sheen)
			{
				if (this.MyPickUp.activeSelf)
				{
					this.MyPickUp.SetActive(false);
				}
				if (this.Sheen.activeSelf != flag2)
				{
					this.Sheen.SetActive(flag2);
				}
				if (this._billboardEat)
				{
					if (this._billboardEat.activeSelf)
					{
						this._billboardEat.SetActive(false);
					}
					if (this._billboardEatSheen.activeSelf != flag)
					{
						this._billboardEatSheen.SetActive(flag);
					}
				}
				if (this._billboardGather)
				{
					if (this._billboardGather.activeSelf)
					{
						this._billboardGather.SetActive(false);
					}
					if (this._billboardGatherSheen.activeSelf != canGather)
					{
						this._billboardGatherSheen.SetActive(canGather);
					}
				}
			}
			else
			{
				if (this.MyPickUp.activeSelf != flag2)
				{
					this.MyPickUp.SetActive(flag2);
				}
				if (this.Sheen.activeSelf)
				{
					this.Sheen.SetActive(false);
				}
				if (this._billboardEat)
				{
					if (this._billboardEat.activeSelf != flag)
					{
						this._billboardEat.SetActive(flag);
					}
					if (this._billboardEatSheen.activeSelf)
					{
						this._billboardEatSheen.SetActive(false);
					}
				}
				if (this._billboardGather)
				{
					if (this._billboardGather.activeSelf != canGather)
					{
						this._billboardGather.SetActive(canGather);
					}
					if (this._billboardGatherSheen.activeSelf)
					{
						this._billboardGatherSheen.SetActive(false);
					}
				}
			}
		}

		private bool CanGather
		{
			get
			{
				return this._fullness < 0.01f && this._hydration > 0.01f && LocalPlayer.Inventory && (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._potItemId) || LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._waterSkinItemId));
			}
		}

		public GameObject _billboardEat;

		public GameObject _billboardEatSheen;

		public GameObject _billboardGather;

		public GameObject _billboardGatherSheen;

		[ItemIdPicker]
		public int _potItemId;

		[ItemIdPicker]
		public int _waterSkinItemId;

		public float _hydration = 1f;

		public float _fullness;

		public float _energy;

		public float _health;

		public float _isFresh;

		public int _meats;

		public int _mushrooms;

		public int _herbs;
	}
}
