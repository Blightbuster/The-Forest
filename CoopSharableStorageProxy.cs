using System;
using Bolt;
using TheForest.Items.Core;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using UnityEngine;


public class CoopSharableStorageProxy : EntityBehaviour<IPlayerState>
{
	
	private void Start()
	{
		if (!this._initialized && base.entity.isAttached)
		{
			this.Attached();
		}
	}

	
	public override void Attached()
	{
		if (!this._initialized)
		{
			this._initialized = true;
			base.state.AddCallback("SharableStorage[]", new PropertyCallbackSimple(this.RefreshStorage));
			this.RefreshStorage();
		}
	}

	
	private void RefreshStorage()
	{
		bool flag = false;
		int num = 0;
		for (int i = 0; i < base.state.SharableStorage.Length; i++)
		{
			if (i < this._storage.UsedSlots.Count)
			{
				if (base.state.SharableStorage[i].Id != this._storage.UsedSlots[i + num]._itemId || base.state.SharableStorage[i].Bonus != (int)this._storage.UsedSlots[i + num]._properties.ActiveBonus || !Mathf.Approximately(base.state.SharableStorage[i].BonusValue, this._storage.UsedSlots[i + num]._properties.ActiveBonusValue))
				{
					if (base.state.SharableStorage[i].Id > 0)
					{
						this._storage.UsedSlots[i + num]._itemId = base.state.SharableStorage[i].Id;
						this._storage.UsedSlots[i + num]._properties.Copy(base.state.SharableStorage[i]);
						flag = true;
					}
					else
					{
						this._storage.UsedSlots.RemoveAt(i);
						num--;
						flag = true;
					}
				}
			}
			else if (base.state.SharableStorage[i].Id > 0)
			{
				this._storage.UsedSlots.Add(new ItemStorage.InventoryItemX
				{
					_itemId = base.state.SharableStorage[i].Id,
					_properties = new ItemProperties
					{
						ActiveBonus = (WeaponStatUpgrade.Types)base.state.SharableStorage[i].Bonus,
						ActiveBonusValue = base.state.SharableStorage[i].BonusValue
					},
					_amount = 1,
					_maxAmount = 1
				});
				flag = true;
			}
		}
		if (flag)
		{
			this._storage.UpdateContentVersion();
		}
	}

	
	public ItemStorage _storage;

	
	private bool _initialized;
}
