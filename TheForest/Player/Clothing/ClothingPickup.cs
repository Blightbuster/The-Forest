using System;
using System.Collections.Generic;
using Bolt;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Player.Clothing
{
	
	public class ClothingPickup : EntityBehaviour<ISuitcaseState>
	{
		
		private void Awake()
		{
			base.enabled = false;
		}

		
		private void OnEnable()
		{
			this.Attached();
		}

		
		private void Update()
		{
			if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false))
			{
				this.RefreshBoltState();
				List<int> list = (from id in this._presetOutfitItemIds
				where id > 0
				select id).ToList<int>();
				List<int> ids = (list != null && list.Count != 0) ? list : ClothingItemDatabase.GetRandomOutfit(LocalPlayer.Stats.PlayerVariation == 0);
				if (LocalPlayer.Clothing.AddClothingOutfit(ids, true))
				{
					Scene.HudGui.ToggleGotClothingOutfitHud();
					LocalPlayer.Clothing.RefreshVisibleClothing();
					LocalPlayer.Stats.CheckArmsStart();
					if (BoltNetwork.isRunning && !base.entity.isOwner && !this._clientOnlyTake)
					{
						this.SendBoltTaken();
					}
					else
					{
						this.DoDestroy();
					}
				}
				else
				{
					Scene.HudGui.ToggleFullClothingOutfitCapacityHud();
				}
			}
		}

		
		private void DoDestroy()
		{
			if (this._disableInsteadOfDestroy)
			{
				this.GrabExit();
				((!this._destroyTarget) ? base.gameObject : this._destroyTarget).SetActive(false);
			}
			else
			{
				UnityEngine.Object.Destroy((!this._destroyTarget) ? base.gameObject : this._destroyTarget);
			}
		}

		
		private void GrabEnter()
		{
			this.ToggleIcons(true);
			base.enabled = true;
			this.SetBoltCanFreeze(false);
			this.RefreshBoltState();
		}

		
		private void SetBoltCanFreeze(bool canFreeze)
		{
			if (!BoltNetwork.isRunning || base.entity == null)
			{
				return;
			}
		}

		
		private void GrabExit()
		{
			this.ToggleIcons(false);
			base.enabled = false;
			this.SetBoltCanFreeze(true);
		}

		
		public void ToggleIcons(bool pickup)
		{
			if (this._sheenIcon && this._sheenIcon.activeSelf != !pickup)
			{
				this._sheenIcon.SetActive(!pickup);
			}
			if (this._pickupIcon && this._pickupIcon.activeSelf != pickup)
			{
				this._pickupIcon.SetActive(pickup);
			}
		}

		
		public override void Attached()
		{
			if (!BoltNetwork.isRunning)
			{
				return;
			}
			if (base.entity == null)
			{
				return;
			}
			if (!base.entity.isOwner)
			{
				this.RefreshBoltState();
				return;
			}
			for (int i = 0; i < base.state.PresetOutfitItemIds.Length; i++)
			{
				if (!this._presetOutfitItemIds.IndexValid(i))
				{
					base.state.PresetOutfitItemIds[i] = -1;
				}
				else
				{
					base.state.PresetOutfitItemIds[i] = this._presetOutfitItemIds[i];
				}
			}
		}

		
		private void RefreshBoltState()
		{
			if (!BoltNetwork.isRunning)
			{
				return;
			}
			if (base.entity.isOwner)
			{
				return;
			}
			if (this._presetOutfitItemIds == null)
			{
				this._presetOutfitItemIds = new List<int>();
			}
			else
			{
				this._presetOutfitItemIds.Clear();
			}
			for (int i = 0; i < base.state.PresetOutfitItemIds.Length; i++)
			{
				int num = base.state.PresetOutfitItemIds[i];
				if (num >= 0)
				{
					this._presetOutfitItemIds.Add(num);
				}
			}
		}

		
		public void SendBoltTaken()
		{
			if (!BoltNetwork.isRunning)
			{
				return;
			}
			if (base.entity.isOwner || base.entity.source == null)
			{
				return;
			}
			TakeClothingOutfit takeClothingOutfit = TakeClothingOutfit.Create(base.entity.source);
			takeClothingOutfit.target = base.entity;
			takeClothingOutfit.Send();
		}

		
		public void DoBoltTaken()
		{
			this.DoDestroy();
		}

		
		[ClothingItemIdPicker]
		public List<int> _presetOutfitItemIds;

		
		public GameObject _destroyTarget;

		
		public bool _disableInsteadOfDestroy;

		
		public GameObject _sheenIcon;

		
		public GameObject _pickupIcon;

		
		public bool _clientOnlyTake;
	}
}
