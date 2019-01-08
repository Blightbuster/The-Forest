using System;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	[AddComponentMenu("Items/World/Decaying Item")]
	public class DecayingItem : EntityBehaviour<IDecayingPickup>
	{
		private void OnStateRefresh()
		{
			if (LocalPlayer.Inventory)
			{
				DecayingInventoryItemView decayingInventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[this._itemId][0] as DecayingInventoryItemView;
				if (decayingInventoryItemView)
				{
					this._renderer.sharedMaterial = decayingInventoryItemView.GetMaterialForState(this.DecayState);
				}
			}
			if (this._pickup)
			{
				this._pickup._state = this.DecayState;
			}
		}

		public override void Attached()
		{
			if (base.entity.isOwner)
			{
				base.state.State = (int)this.DecayState;
			}
			else
			{
				base.state.AddCallback("State", new PropertyCallbackSimple(this.OnStateRefreshMp));
			}
		}

		private void OnStateRefreshMp()
		{
			this.DecayState = (DecayingInventoryItemView.DecayStates)base.state.State;
		}

		public DecayingInventoryItemView.DecayStates DecayState
		{
			get
			{
				return this._decayState;
			}
			set
			{
				this._decayState = value;
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
				{
					base.state.State = (int)value;
				}
				this.OnStateRefresh();
			}
		}

		public Renderer _renderer;

		[ItemIdPicker]
		public int _itemId;

		public DecayingPickUp _pickup;

		private DecayingInventoryItemView.DecayStates _decayState;
	}
}
