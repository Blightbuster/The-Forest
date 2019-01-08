using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Player;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class MultiThrowerItemHolder : EntityBehaviour<IMultiThrowerState>
	{
		private void Awake()
		{
			base.enabled = false;
			this._items = new int[this._renderSlots.Length];
		}

		private void OnEnable()
		{
			FMODCommon.PreloadEvents(new string[]
			{
				this._addItemEvent
			});
			this._hasPreloaded = true;
		}

		private void OnDisable()
		{
			if (this._hasPreloaded)
			{
				FMODCommon.UnloadEvents(new string[]
				{
					this._addItemEvent
				});
				this._hasPreloaded = false;
			}
		}

		private void Update()
		{
			if (BoltNetwork.isRunning)
			{
				this.UpdateMP();
			}
			if (this._nextItemIndex > 0)
			{
				Scene.HudGui.MultiThrowerTakeWidget.ShowSingle(this._items[this._nextItemIndex - 1], this._takeIcon.transform, SideIcons.Take);
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					this.TakeItem();
				}
			}
			else
			{
				Scene.HudGui.MultiThrowerTakeWidget.Shutdown();
			}
			if (this._nextItemIndex < this._renderSlots.Length)
			{
				bool flag = this.CanToggleNextAddItem();
				if (flag && TheForest.Utils.Input.GetButtonDown("Rotate"))
				{
					LocalPlayer.Sfx.PlayWhoosh();
					this.ToggleNextAddItem();
				}
				if (this._currentAddItem >= 0 && this._currentAddItem < ItemDatabase.Items.Length)
				{
					int id = ItemDatabase.Items[this._currentAddItem]._id;
					bool flag2 = !LocalPlayer.Inventory.IsRightHandEmpty() && LocalPlayer.Inventory.RightHand._itemId == id;
					if (LocalPlayer.Inventory.Owns(id, true))
					{
						Scene.HudGui.MultiThrowerAddWidget.ShowList(ItemDatabase.Items[this._currentAddItem]._id, this._addIcon.transform, SideIcons.Craft);
						if (TheForest.Utils.Input.GetButtonDown("Craft") && ((flag2 && LocalPlayer.Inventory.ShuffleRemoveRightHandItem()) || (!flag2 && LocalPlayer.Inventory.RemoveItem(id, 1, false, true))))
						{
							if (this._addItemEvent.Length > 0)
							{
								FMODCommon.PlayOneshot(this._addItemEvent, base.transform);
							}
							else
							{
								LocalPlayer.Sfx.PlayPutDown(base.gameObject);
							}
							if (BoltNetwork.isRunning)
							{
								ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(GlobalTargets.OnlyServer);
								itemHolderAddItem.ContentType = id;
								itemHolderAddItem.Target = base.entity;
								itemHolderAddItem.Send();
							}
							else
							{
								this.SpawnEquipmentItemView(this._renderSlots[this._nextItemIndex], id);
								this._items[this._nextItemIndex] = id;
								this._nextItemIndex++;
							}
						}
						return;
					}
				}
				if (flag)
				{
					this.ToggleNextAddItem();
				}
				Scene.HudGui.MultiThrowerAddWidget.Shutdown();
			}
			else
			{
				Scene.HudGui.MultiThrowerAddWidget.Shutdown();
			}
		}

		private void TakeItem()
		{
			int num = this._items[this._nextItemIndex - 1];
			bool flag = LocalPlayer.Inventory.AddItem(num, 1, false, false, null);
			if (!flag)
			{
				flag = LocalPlayer.Inventory.FakeDrop(num, null);
			}
			if (!flag)
			{
				return;
			}
			if (BoltNetwork.isRunning)
			{
				ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(GlobalTargets.OnlyServer);
				itemHolderTakeItem.Target = base.entity;
				itemHolderTakeItem.ContentType = num;
				itemHolderTakeItem.Player = LocalPlayer.Entity;
				itemHolderTakeItem.Send();
			}
			else
			{
				this._nextItemIndex--;
				UnityEngine.Object.Destroy(this._renderSlots[this._nextItemIndex].GetChild(0).gameObject);
				this._renderSlots[this._nextItemIndex].gameObject.SetActive(false);
				this._items[this._nextItemIndex] = -1;
			}
			if (DecayingInventoryItemView.LastUsed)
			{
				DecayingInventoryItemView.LastUsed.SetDecayState(DecayingInventoryItemView.DecayStates.Spoilt);
			}
		}

		private void OnDestroy()
		{
			this.GrabExit();
		}

		private void OnDeserialized()
		{
			if (!BoltNetwork.isClient)
			{
				for (int i = 0; i < this._items.Length; i++)
				{
					Transform parent = this._renderSlots[i];
					int num = this._items[i];
					if (num <= 0)
					{
						break;
					}
					this.SpawnEquipmentItemView(parent, num);
					this._nextItemIndex++;
				}
				if (this._ammoLoaded == null || this._ammoLoaded.Length != 3)
				{
					this._ammoLoaded = new int[3];
				}
				for (int j = 0; j < this._ammoLoaded.Length; j++)
				{
					if (this._ammoLoaded[j] > 0)
					{
						this._anim.rockAmmo[j].SetActive(true);
						this.SpawnEquipmentItemView(this._anim.rockAmmo[j].transform, this._ammoLoaded[j]);
					}
				}
			}
		}

		private void GrabEnter()
		{
			if (!BoltNetwork.isRunning || base.entity.isAttached)
			{
				if (!LocalPlayer.Inventory.IsRightHandEmpty() && this.IsValidItem(LocalPlayer.Inventory.RightHandOrNext.ItemCache))
				{
					this._currentAddItem = ItemDatabase.ItemIndexById(LocalPlayer.Inventory.RightHandOrNext._itemId);
					this.UpdateIcons();
				}
				else
				{
					this._currentAddItem--;
					this.ToggleNextAddItem();
				}
				base.enabled = (this._nextItemIndex > 0 || (this._currentAddItem >= 0 && this._currentAddItem < ItemDatabase.Items.Length && LocalPlayer.Inventory.Owns(ItemDatabase.Items[this._currentAddItem]._id, true)));
			}
		}

		private void GrabExit()
		{
			base.enabled = false;
			if (Scene.HudGui)
			{
				Scene.HudGui.MultiThrowerAddWidget.Shutdown();
				Scene.HudGui.MultiThrowerTakeWidget.Shutdown();
			}
		}

		private void UpdateMP()
		{
		}

		private bool IsWhiteListed(int itemId)
		{
			for (int i = 0; i < this._whiteListedItemIds.Length; i++)
			{
				if (this._whiteListedItemIds[i] == itemId)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsValidItem(Item item)
		{
			return !item.MatchType(Item.Types.Story | Item.Types.Weapon) && item.MatchType(Item.Types.Equipment) && item._equipmentSlot == Item.EquipmentSlot.RightHand && this.IsWhiteListed(item._id);
		}

		private void UpdateIcons()
		{
		}

		public void ToggleNextAddItem()
		{
			int nextItemIndex = this.GetNextItemIndex();
			if (nextItemIndex != this._currentAddItem)
			{
				this._currentAddItem = nextItemIndex;
				this.UpdateIcons();
				return;
			}
			if (this._currentAddItem > -1)
			{
				Item item = ItemDatabase.Items[this._currentAddItem];
				if (!LocalPlayer.Inventory.Owns(item._id, true) || !this.IsValidItem(item))
				{
					this._currentAddItem = -1;
				}
			}
		}

		public bool CanToggleNextAddItem()
		{
			return this.GetNextItemIndex() != this._currentAddItem;
		}

		private int GetNextItemIndex()
		{
			if (this._currentAddItem < 0)
			{
				this._currentAddItem = -1;
			}
			for (int i = 1; i < ItemDatabase.Items.Length; i++)
			{
				int num = (this._currentAddItem + i) % ItemDatabase.Items.Length;
				Item item = ItemDatabase.Items[num];
				if (this.IsValidItem(item) && LocalPlayer.Inventory.Owns(item._id, false))
				{
					return num;
				}
			}
			return this._currentAddItem;
		}

		private void SpawnEquipmentItemView(Transform parent, int itemId)
		{
			if (LocalPlayer.Inventory)
			{
				Reparent.Locked = true;
				InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0];
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((!inventoryItemView.ItemCache._throwerProjectilePrefab) ? inventoryItemView._held : inventoryItemView.ItemCache._throwerProjectilePrefab.gameObject);
				FakeParent component = inventoryItemView._held.GetComponent<FakeParent>();
				parent.gameObject.SetActive(true);
				gameObject.transform.parent = parent;
				gameObject.transform.localPosition = ((!component) ? inventoryItemView._held.transform.localPosition : component.RealLocalPosition);
				gameObject.transform.localRotation = ((!component) ? inventoryItemView._held.transform.localRotation : component.RealLocalRotation);
				gameObject.layer = base.gameObject.layer;
				gameObject.SetActive(true);
				if (!inventoryItemView.ItemCache._throwerProjectilePrefab)
				{
					IEnumerator enumerator = gameObject.transform.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							object obj = enumerator.Current;
							Transform transform = (Transform)obj;
							if (transform.name.Contains("collide") || transform.GetComponent<weaponInfo>())
							{
								UnityEngine.Object.Destroy(transform.gameObject);
							}
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
					foreach (MonoBehaviour obj2 in gameObject.GetComponents<MonoBehaviour>())
					{
						UnityEngine.Object.Destroy(obj2);
					}
				}
				Reparent.Locked = false;
			}
		}

		private void TransferItemView(Transform itemView, Transform parent, int itemId)
		{
			itemView.parent = parent;
			if (LocalPlayer.Inventory)
			{
				InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0];
				FakeParent component = inventoryItemView._held.GetComponent<FakeParent>();
				itemView.transform.localPosition = ((!component) ? inventoryItemView._held.transform.localPosition : component.RealLocalPosition);
				itemView.transform.localRotation = ((!component) ? inventoryItemView._held.transform.localRotation : component.RealLocalRotation);
			}
			else
			{
				itemView.localPosition = Vector3.zero;
				itemView.localRotation = Quaternion.identity;
			}
		}

		public override void Attached()
		{
			base.state.AddCallback("Items[]", new PropertyCallbackSimple(this.ItemsChangedMP));
			if (BoltNetwork.isClient)
			{
				base.state.AddCallback("Ammo[]", new PropertyCallbackSimple(this.AmmoChangedMP));
			}
			if (BoltNetwork.isServer)
			{
				for (int i = 0; i < this._items.Length; i++)
				{
					base.state.Items[i] = this._items[i];
				}
				for (int j = 0; j < this._ammoLoaded.Length; j++)
				{
					base.state.Ammo[j] = this._ammoLoaded[j];
				}
			}
		}

		public void loadItemIntoBasket(int type)
		{
			if (this._nextItemIndex > 0)
			{
				this._nextItemIndex--;
				int num = this._items[this._nextItemIndex];
				this._items[this._nextItemIndex] = -1;
				this._ammoLoaded[this._anim.ammoCount] = num;
				this._anim.rockAmmo[this._anim.ammoCount].SetActive(true);
				if (this._renderSlots[this._nextItemIndex].childCount > 0)
				{
					Transform child = this._renderSlots[this._nextItemIndex].GetChild(0);
					this.TransferItemView(child, this._anim.rockAmmo[this._anim.ammoCount].transform, num);
				}
				if (LocalPlayer.Sfx)
				{
					FMODCommon.PlayOneshotNetworked(LocalPlayer.Sfx.WhooshEvent, base.transform, FMODCommon.NetworkRole.Server);
				}
				if (BoltNetwork.isRunning)
				{
					base.entity.Freeze(false);
					base.state.Items[this._nextItemIndex] = -1;
					base.state.Ammo[this._anim.ammoCount] = num;
				}
				this._anim.ammoCount++;
			}
		}

		public void resetBasketAmmo()
		{
			for (int i = 0; i < this._ammoLoaded.Length; i++)
			{
				this._ammoLoaded[i] = 0;
				if (BoltNetwork.isServer)
				{
					base.state.Ammo[i] = 0;
				}
				if (!BoltNetwork.isClient && this._anim.rockAmmo[i].transform.childCount > 0)
				{
					UnityEngine.Object.Destroy(this._anim.rockAmmo[i].transform.GetChild(0).gameObject);
				}
				this._anim.rockAmmo[i].SetActive(false);
			}
			this._anim.ammoCount = 0;
		}

		public void TakeItemMP(BoltEntity targetPlayer, int itemId)
		{
			if (this._nextItemIndex > 0)
			{
				base.entity.Freeze(false);
				base.state.Items[--this._nextItemIndex] = (this._items[this._nextItemIndex] = -1);
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
				itemRemoveFromPlayer.ItemId = itemId;
				itemRemoveFromPlayer.Send();
			}
		}

		public void AddItemMP(int itemId)
		{
			if (this._nextItemIndex < this._items.Length)
			{
				NetworkArray_Values<int> items = base.state.Items;
				int nextItemIndex = this._nextItemIndex;
				this._items[this._nextItemIndex] = itemId;
				items[nextItemIndex] = itemId;
				base.entity.Freeze(false);
				this._nextItemIndex++;
			}
		}

		private void ItemsChangedMP()
		{
			if (BoltNetwork.isClient)
			{
				this._nextItemIndex = 0;
				for (int i = 0; i < this._items.Length; i++)
				{
					int num = base.state.Items[i];
					this._items[i] = num;
					if (num > 0)
					{
						this._nextItemIndex++;
					}
				}
			}
			for (int j = 0; j < this._items.Length; j++)
			{
				int num2 = this._items[j];
				Transform transform = this._renderSlots[j];
				bool flag = transform.childCount > 0;
				if (num2 > 0)
				{
					if (!flag)
					{
						this.SpawnEquipmentItemView(transform, num2);
					}
				}
				else if (flag)
				{
					UnityEngine.Object.Destroy(transform.GetChild(0).gameObject);
				}
			}
		}

		private void AmmoChangedMP()
		{
			if (BoltNetwork.isClient)
			{
				this._anim.ammoCount = 0;
				for (int i = 0; i < this._ammoLoaded.Length; i++)
				{
					if (this._ammoLoaded[i] != base.state.Ammo[i])
					{
						if (this._anim.rockAmmo[i].transform.childCount > 0)
						{
							this._anim.rockAmmo[i].SetActive(false);
							UnityEngine.Object.Destroy(this._anim.rockAmmo[i].transform.GetChild(0).gameObject);
						}
						this._ammoLoaded[i] = base.state.Ammo[i];
						if (this._ammoLoaded[i] > 0)
						{
							this._anim.rockAmmo[i].SetActive(true);
							this.SpawnEquipmentItemView(this._anim.rockAmmo[i].transform, this._ammoLoaded[i]);
						}
					}
					if (this._ammoLoaded[i] > 0)
					{
						this._anim.ammoCount++;
					}
				}
			}
		}

		public void disableTriggerMP()
		{
			if (BoltNetwork.isRunning)
			{
				RockThrowerActivated rockThrowerActivated = RockThrowerActivated.Create(GlobalTargets.Everyone);
				rockThrowerActivated.Target = base.entity;
				rockThrowerActivated.Send();
			}
		}

		public void enableTriggerMP()
		{
			if (BoltNetwork.isRunning)
			{
				RockThrowerDeActivated rockThrowerDeActivated = RockThrowerDeActivated.Create(GlobalTargets.Everyone);
				rockThrowerDeActivated.Target = base.entity;
				rockThrowerDeActivated.Send();
			}
		}

		public void forceRemoveItem()
		{
			if (this._nextItemIndex > 0)
			{
				if (BoltNetwork.isRunning)
				{
					RockThrowerRemoveItem rockThrowerRemoveItem = RockThrowerRemoveItem.Create(GlobalTargets.OnlyServer);
					rockThrowerRemoveItem.ContentType = this._items[this._nextItemIndex - 1];
					rockThrowerRemoveItem.Target = base.entity;
					rockThrowerRemoveItem.Player = LocalPlayer.Entity;
					rockThrowerRemoveItem.Send();
				}
				else
				{
					this.loadItemIntoBasket(this._items[this._nextItemIndex - 1]);
				}
			}
		}

		public void sendResetAmmoMP()
		{
			if (BoltNetwork.isRunning)
			{
				RockThrowerResetAmmo rockThrowerResetAmmo = RockThrowerResetAmmo.Create(GlobalTargets.Everyone);
				rockThrowerResetAmmo.Target = base.entity;
				rockThrowerResetAmmo.Send();
			}
			else
			{
				this.resetBasketAmmo();
			}
		}

		public void sendAnimVars(int var, bool onoff)
		{
			RockThrowerAnimate rockThrowerAnimate = RockThrowerAnimate.Create(GlobalTargets.Everyone);
			rockThrowerAnimate.animVar = var;
			rockThrowerAnimate.onoff = onoff;
			rockThrowerAnimate.Target = base.entity;
			rockThrowerAnimate.Send();
		}

		public void sendLandTarget()
		{
			RockThrowerLandTarget rockThrowerLandTarget = RockThrowerLandTarget.Create(GlobalTargets.OnlyServer);
			rockThrowerLandTarget.landPos = this._anim.throwPos.GetComponent<rockThrowerAimingReticle>()._currentLandTarget;
			rockThrowerLandTarget.Target = base.entity;
			rockThrowerLandTarget.Send();
		}

		public int[] AmmoLoaded
		{
			get
			{
				return this._ammoLoaded;
			}
		}

		public Transform[] _renderSlots;

		public GameObject _takeIcon;

		public GameObject _addIcon;

		public Transform _lever;

		public rockThrowerAnimEvents _anim;

		[ItemIdPicker(Item.Types.Equipment)]
		public int[] _whiteListedItemIds;

		[Header("FMOD")]
		public string _addItemEvent;

		[SerializeThis]
		private int[] _items;

		private int _currentAddItem;

		private int _nextItemIndex;

		private bool _hasPreloaded;

		[SerializeThis]
		private int[] _ammoLoaded = new int[3];
	}
}
