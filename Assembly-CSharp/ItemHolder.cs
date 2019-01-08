using System;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

[DoNotSerializePublic]
public class ItemHolder : EntityEventListener<IItemHolderState>
{
	private void Awake()
	{
		base.enabled = false;
		if (this._bonusManager)
		{
			this._bonusManager.Init(this.ItemsRender.Length);
		}
	}

	private void OnEnable()
	{
		FMODCommon.PreloadEvents(new string[]
		{
			this.addItemEvent
		});
		this.hasPreloaded = true;
	}

	private void OnDisable()
	{
		if (this.hasPreloaded)
		{
			FMODCommon.UnloadEvents(new string[]
			{
				this.addItemEvent
			});
			this.hasPreloaded = false;
		}
		if (Scene.HudGui)
		{
			Scene.HudGui.HolderWidgets[(int)this._type].ShutDown();
		}
	}

	private void Update()
	{
		if (BoltNetwork.isServer)
		{
			base.state.ItemCount = this.Items;
		}
		else if (BoltNetwork.isClient)
		{
			this.Items = base.state.ItemCount;
		}
		bool takeIcon = this.Items > 0;
		if (this.Items > 0 && TheForest.Utils.Input.GetButtonDown("Take") && LocalPlayer.Inventory.AddItem(this._itemid, 1, false, false, (!this._bonusManager) ? ItemProperties.Any : this._bonusManager.GetItemProperties(this.Items - 1)))
		{
			LocalPlayer.Sfx.PlayItemCustomSfx(this._itemid, true);
			if (BoltNetwork.isRunning)
			{
				ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(GlobalTargets.OnlyServer);
				itemHolderTakeItem.Target = base.entity;
				itemHolderTakeItem.Player = LocalPlayer.Entity;
				itemHolderTakeItem.Send();
			}
			else
			{
				this.Items--;
				this.ItemsRender[this.Items].SetActive(false);
				if (this._bonusManager)
				{
					this._bonusManager.UnsetItemProperties(this.Items);
				}
			}
		}
		bool flag = this.Items < this.ItemsRender.Length && LocalPlayer.Inventory.Owns(this._itemid, true);
		if (flag && TheForest.Utils.Input.GetButtonDown("Craft"))
		{
			if (this.addItemEvent.Length > 0)
			{
				FMODCommon.PlayOneshot(this.addItemEvent, base.transform);
			}
			else
			{
				LocalPlayer.Sfx.PlayPutDown(base.gameObject);
			}
			if (LocalPlayer.Inventory.RemoveItem(this._itemid, 1, false, true))
			{
				if (BoltNetwork.isRunning)
				{
					ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(GlobalTargets.OnlyServer);
					itemHolderAddItem.Target = base.entity;
					itemHolderAddItem.Send();
				}
				else
				{
					this.Items++;
					this.ItemsRender[this.Items - 1].SetActive(true);
					if (this._bonusManager)
					{
						int index = LocalPlayer.Inventory.AmountOf(this._itemid, true);
						this._bonusManager.SetItemProperties(this.Items - 1, this._itemid, LocalPlayer.Inventory.InventoryItemViewsCache[this._itemid][index].Properties);
					}
				}
			}
		}
		Scene.HudGui.HolderWidgets[(int)this._type].Show(takeIcon, flag, this.TakeIcon.transform);
	}

	private void OnDeserialized()
	{
		if (!BoltNetwork.isClient)
		{
			for (int i = 0; i < this.ItemsRender.Length; i++)
			{
				GameObject gameObject = this.ItemsRender[i];
				bool flag = i < this.Items;
				if (gameObject.activeSelf != flag)
				{
					gameObject.SetActive(flag);
				}
			}
		}
	}

	private void GrabEnter()
	{
		LocalPlayer.Inventory.DontShowDrop = true;
		base.enabled = (!BoltNetwork.isRunning || base.entity.isAttached);
	}

	private void GrabExit()
	{
		LocalPlayer.Inventory.DontShowDrop = false;
		base.enabled = false;
		Scene.HudGui.HolderWidgets[(int)this._type].ShutDown();
	}

	public override void Attached()
	{
		base.state.AddCallback("ItemCount", new PropertyCallbackSimple(this.ItemCountChangedMP));
		if (BoltNetwork.isServer)
		{
			base.state.ItemCount = this.Items;
		}
	}

	public void TakeItemMP(BoltEntity targetPlayer)
	{
		if (this.Items > 0)
		{
			base.entity.Freeze(false);
			base.state.ItemCount = (this.Items = Mathf.Max(0, this.Items - 1));
		}
		else
		{
			ItemRemoveFromPlayer itemRemoveFromPlayer;
			if (targetPlayer.isOwner)
			{
				itemRemoveFromPlayer = ItemRemoveFromPlayer.Create(GlobalTargets.OnlySelf);
			}
			else
			{
				itemRemoveFromPlayer = ItemRemoveFromPlayer.Create(targetPlayer.source);
			}
			itemRemoveFromPlayer.ItemId = this._itemid;
			itemRemoveFromPlayer.Send();
		}
	}

	public void AddItemMP(BoltConnection source)
	{
		if (this.Items < this.ItemsRender.Length)
		{
			base.state.ItemCount = (this.Items = Mathf.Min(this.Items + 1, this.ItemsRender.Length));
			base.entity.Freeze(false);
		}
		else
		{
			PlayerAddItem playerAddItem = PlayerAddItem.Create(source);
			playerAddItem.ItemId = this._itemid;
			playerAddItem.Send();
		}
	}

	private void ItemCountChangedMP()
	{
		if (BoltNetwork.isClient)
		{
			this.Items = base.state.ItemCount;
		}
		for (int i = 0; i < this.ItemsRender.Length; i++)
		{
			this.ItemsRender[i].SetActive(false);
		}
		for (int j = 0; j < this.Items; j++)
		{
			this.ItemsRender[j].SetActive(true);
		}
	}

	public void forceRemoveItem()
	{
		this.ItemsRender[this.Items - 1].SetActive(false);
		this.Items--;
		if (this.Items > 0)
		{
			LocalPlayer.Sfx.PlayItemCustomSfx(this._itemid, true);
		}
	}

	[ItemIdPicker]
	public int _itemid;

	public GameObject[] ItemsRender;

	public GameObject TakeIcon;

	public GameObject AddIcon;

	public StorageItemBonusManager _bonusManager;

	public HolderTypes _type;

	[Header("FMOD")]
	public string addItemEvent;

	[SerializeThis]
	public int Items;

	private bool hasPreloaded;
}
