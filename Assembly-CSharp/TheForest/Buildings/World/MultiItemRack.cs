using System;
using System.Collections;
using Bolt;
using TheForest.Audio;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class MultiItemRack : EntityEventListener<IMultiItemHolderState>
	{
		private void Awake()
		{
			base.enabled = false;
			this._storedItemIds = new int[this._slots.Length];
		}

		private void Update()
		{
			if (this.IsSlotOccupied)
			{
				Scene.HudGui.RackWidgets[(int)this._type].ShowSingle(this._currentTakeItem, this.CurrentTakeItemId, this._slots[this.CurrentSlot]._slotTr, SideIcons.Take);
				if (TheForest.Utils.Input.GetButtonDown("Take") && (LocalPlayer.Inventory.AddItem(this.CurrentTakeItemId, 1, false, false, null) || LocalPlayer.Inventory.FakeDrop(this.CurrentTakeItemId, null)))
				{
					LocalPlayer.Sfx.PlayItemCustomSfx(this.CurrentTakeItemId, true);
					this.CurrentTakeIcon.SetActive(false);
					if (BoltNetwork.isRunning)
					{
						ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(GlobalTargets.OnlyServer);
						itemHolderTakeItem.Target = base.entity;
						itemHolderTakeItem.Player = LocalPlayer.Entity;
						itemHolderTakeItem.ContentType = this.CurrentSlot;
						itemHolderTakeItem.ContentValue = this.CurrentTakeItemId;
						itemHolderTakeItem.Send();
						if (BoltNetwork.isClient)
						{
							this.CurrentTakeItemId = 0;
						}
					}
					else
					{
						this.CurrentTakeItemId = 0;
						this.UpdateRenderers();
					}
				}
			}
			int num = LocalPlayer.Inventory.OwnsWhich(this.CurrentAddItemId, this._allowFallback);
			if (!this.IsSlotOccupied && num > -1)
			{
				if (this._slots[this.CurrentSlot]._items.Length > 1)
				{
					Scene.HudGui.RackWidgets[(int)this._type].ShowList(this.CurrentAddItemId, num, this._slots[this.CurrentSlot]._slotTr, SideIcons.Craft);
				}
				else
				{
					Scene.HudGui.RackWidgets[(int)this._type].ShowSingle(this.CurrentAddItemId, num, this._slots[this.CurrentSlot]._slotTr, SideIcons.Craft);
				}
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					Sfx.Play(SfxInfo.SfxTypes.AddItem, this.CurrentAddIcon.transform, true);
					if (LocalPlayer.Inventory.RemoveItem(num, 1, false, true))
					{
						this.CurrentTakeItemId = num;
						this._currentTakeItem = this.CurrentAddItemId;
						if (BoltNetwork.isRunning)
						{
							ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(GlobalTargets.OnlyServer);
							itemHolderAddItem.Target = base.entity;
							itemHolderAddItem.ContentType = this.CurrentSlot;
							itemHolderAddItem.ContentInfo = num;
							itemHolderAddItem.Send();
						}
						else
						{
							this.UpdateRenderers();
						}
					}
				}
			}
			bool flag = this.CanToggleNextAddItem();
			if (flag && (TheForest.Utils.Input.GetButtonDown("Rotate") || num == -1))
			{
				LocalPlayer.Sfx.PlayWhoosh();
				this.ToggleNextAddItem();
			}
		}

		private void OnDestroy()
		{
			foreach (MultiItemRack.SlotInfo slotInfo in this._slots)
			{
				foreach (MultiItemRack.ItemInfo itemInfo in slotInfo._items)
				{
					itemInfo._renderers = null;
				}
				slotInfo._items = null;
				slotInfo._slotTr = null;
			}
			this._slots = null;
			if (Scene.HudGui)
			{
				Scene.HudGui.RackWidgets[(int)this._type].Shutdown();
			}
		}

		private void OnDeserialized()
		{
			if (!BoltNetwork.isClient)
			{
				this.UpdateRenderers();
			}
		}

		public void GrabEnter(Transform proxy)
		{
			base.enabled = (!BoltNetwork.isRunning || base.entity.isAttached);
			this._icons.transform.position = proxy.position + base.transform.TransformDirection(new Vector3(0f, -0.5f, 0.5f));
			if (base.enabled)
			{
				this._currentAddItem--;
				this.ToggleNextAddItem();
			}
			if (this.IsSlotOccupied)
			{
				foreach (MultiItemRack.ItemInfo itemInfo in this._slots[this.CurrentSlot]._items)
				{
					Item item = ItemDatabase.ItemById(itemInfo._itemId);
					if (item.CanFallbackTo(this.CurrentTakeItemId))
					{
						this._currentTakeItem = item._id;
						break;
					}
				}
			}
		}

		public void GrabExit()
		{
			base.enabled = false;
			Scene.HudGui.RackWidgets[(int)this._type].Shutdown();
		}

		public void ToggleNextAddItem()
		{
			int num = this._slots[this.CurrentSlot]._items.Length + 1;
			for (int i = 1; i < num; i++)
			{
				int num2 = (this._currentAddItem + i) % this._slots[this.CurrentSlot]._items.Length;
				if (LocalPlayer.Inventory.Owns(this._slots[this.CurrentSlot]._items[num2]._itemId, this._allowFallback))
				{
					this._currentAddItem = num2;
					return;
				}
			}
			this._currentAddItem = Mathf.Clamp(this._currentAddItem, 0, this._slots[this.CurrentSlot]._items.Length - 1);
		}

		public bool CanToggleNextAddItem()
		{
			if (!this.IsSlotOccupied)
			{
				for (int i = 1; i < this._slots[this.CurrentSlot]._items.Length; i++)
				{
					int num = (this._currentAddItem + i) % this._slots[this.CurrentSlot]._items.Length;
					if (LocalPlayer.Inventory.Owns(this._slots[this.CurrentSlot]._items[num]._itemId, this._allowFallback))
					{
						return true;
					}
				}
			}
			return false;
		}

		private void UpdateRenderers()
		{
			for (int i = 0; i < this._slots.Length; i++)
			{
				int num = this._storedItemIds[i];
				MultiItemRack.SlotInfo slotInfo = this._slots[i];
				if (num > 0)
				{
					MultiItemRack.ItemInfo itemInfo = null;
					for (int j = 0; j < slotInfo._items.Length; j++)
					{
						if (slotInfo._items[j]._itemId == num)
						{
							itemInfo = slotInfo._items[j];
							break;
						}
					}
					if (itemInfo != null && itemInfo._renderers.Length > 0)
					{
						for (int k = 0; k < itemInfo._renderers.Length; k++)
						{
							MultiItemRack.RendererInfo rendererInfo = itemInfo._renderers[k];
							if (rendererInfo._mat && rendererInfo._mat != rendererInfo._renderer.sharedMaterial)
							{
								rendererInfo._renderer.sharedMaterial = rendererInfo._mat;
							}
							if (!rendererInfo._renderer.gameObject.activeSelf)
							{
								rendererInfo._renderer.gameObject.SetActive(true);
							}
						}
					}
					else if (slotInfo._slotTr.childCount == 0)
					{
						this.SpawnItemView(num, slotInfo._slotTr);
					}
				}
				else
				{
					for (int l = 0; l < slotInfo._items.Length; l++)
					{
						if (slotInfo._items[l]._renderers.Length > 0)
						{
							for (int m = 0; m < slotInfo._items[l]._renderers.Length; m++)
							{
								GameObject gameObject = slotInfo._items[l]._renderers[m]._renderer.gameObject;
								if (gameObject.activeSelf)
								{
									gameObject.SetActive(false);
								}
							}
						}
						else if (slotInfo._slotTr.childCount > 0)
						{
							UnityEngine.Object.Destroy(slotInfo._slotTr.GetChild(0).gameObject);
						}
					}
				}
			}
		}

		private MultiItemRack.ItemInfo GetSlotInfo(int itemId)
		{
			if (itemId > 0)
			{
				for (int i = 0; i < this._slots[this.CurrentSlot]._items.Length; i++)
				{
					Item item = ItemDatabase.ItemById(this._slots[this.CurrentSlot]._items[i]._itemId);
					if (item.CanFallbackTo(itemId))
					{
						return this._slots[this.CurrentSlot]._items[i];
					}
				}
			}
			return null;
		}

		private void SpawnItemView(int itemId, Transform parent)
		{
			if (LocalPlayer.Inventory)
			{
				if (ItemDatabase.ItemById(itemId).MatchType(Item.Types.Equipment))
				{
					this.SpawnEquipmentItemView(itemId, parent);
				}
				else
				{
					this.SpawnNonEquipmentItemView(itemId, parent);
				}
			}
		}

		private void SpawnEquipmentItemView(int itemId, Transform parent)
		{
			Reparent.Locked = true;
			InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inventoryItemView._held);
			FakeParent component = inventoryItemView._held.GetComponent<FakeParent>();
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = ((!component) ? inventoryItemView._held.transform.localPosition : component.RealLocalPosition);
			gameObject.transform.localRotation = ((!component) ? inventoryItemView._held.transform.localRotation : component.RealLocalRotation);
			gameObject.gameObject.layer = base.transform.parent.gameObject.layer;
			gameObject.SetActive(true);
			IEnumerator enumerator = gameObject.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					if (transform.name == "collide" || transform.GetComponent<BurnableCloth>())
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
			Reparent.Locked = false;
		}

		private void SpawnNonEquipmentItemView(int itemId, Transform parent)
		{
			InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0];
			InventoryItemView inventoryItemView2 = UnityEngine.Object.Instantiate<InventoryItemView>(inventoryItemView);
			inventoryItemView2.transform.localScale = inventoryItemView.transform.lossyScale;
			inventoryItemView2.transform.parent = parent;
			if (inventoryItemView._modelOffsetTr)
			{
				inventoryItemView2.transform.localPosition = inventoryItemView._modelOffsetTr.localPosition;
			}
			else
			{
				Vector3 position = parent.position;
				position.y += LocalPlayer.Inventory._inventoryGO.transform.InverseTransformPoint(inventoryItemView.transform.position).y;
				inventoryItemView2.transform.position = position;
			}
			inventoryItemView2.transform.rotation = base.transform.parent.rotation * Quaternion.Inverse(inventoryItemView.transform.rotation);
			inventoryItemView2.gameObject.layer = base.transform.parent.gameObject.layer;
			inventoryItemView2.gameObject.SetActive(true);
			UnityEngine.Object.Destroy(inventoryItemView2.GetComponent<Collider>());
			VirtualCursorSnapNode component = inventoryItemView2.gameObject.GetComponent<VirtualCursorSnapNode>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
			StoreInformation component2 = inventoryItemView2.gameObject.GetComponent<StoreInformation>();
			if (component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			UnityEngine.Object.Destroy(inventoryItemView2);
		}

		public override void Attached()
		{
			base.state.AddCallback("ItemCount[]", new PropertyCallbackSimple(this.SlotItemChangedMP));
			if (BoltNetwork.isServer)
			{
				for (int i = 0; i < this._slots.Length; i++)
				{
					base.state.ItemCount[i] = this._storedItemIds[i];
				}
			}
		}

		public void TakeItemMP(BoltConnection source, int contentType, int contentValue)
		{
			if (this._storedItemIds[contentType] == contentValue)
			{
				base.entity.Freeze(false);
				base.state.ItemCount[contentType] = 0;
			}
			else
			{
				ItemRemoveFromPlayer itemRemoveFromPlayer;
				if (source == null)
				{
					itemRemoveFromPlayer = ItemRemoveFromPlayer.Create(GlobalTargets.OnlySelf);
				}
				else
				{
					itemRemoveFromPlayer = ItemRemoveFromPlayer.Create(source);
				}
				itemRemoveFromPlayer.ItemId = contentValue;
				itemRemoveFromPlayer.Send();
			}
		}

		public void AddItemMP(int contentType, int contentInfo)
		{
			for (int i = 0; i < this._slots[contentType]._items.Length; i++)
			{
				Item item = ItemDatabase.ItemById(this._slots[contentType]._items[i]._itemId);
				if (item.CanFallbackTo(contentInfo))
				{
					base.state.ItemCount[contentType] = contentInfo;
					base.entity.Freeze(false);
				}
			}
		}

		private void SlotItemChangedMP()
		{
			for (int i = 0; i < this._slots.Length; i++)
			{
				if (!CoopPeerStarter.DedicatedHost && i == this.CurrentSlot && base.state.ItemCount[i] == 0)
				{
					this._currentAddItem--;
					this.ToggleNextAddItem();
				}
				this._storedItemIds[i] = base.state.ItemCount[i];
			}
			this.UpdateRenderers();
		}

		public int CurrentSlot { get; set; }

		private int CurrentAddItemId
		{
			get
			{
				return this._slots[this.CurrentSlot]._items[this._currentAddItem]._itemId;
			}
		}

		private int CurrentTakeItemId
		{
			get
			{
				return this._storedItemIds[this.CurrentSlot];
			}
			set
			{
				this._storedItemIds[this.CurrentSlot] = value;
			}
		}

		private bool IsSlotOccupied
		{
			get
			{
				return this._storedItemIds[this.CurrentSlot] > 0;
			}
		}

		private GameObject CurrentAddIcon
		{
			get
			{
				return this._addIcon.gameObject;
			}
		}

		private GameObject CurrentTakeIcon
		{
			get
			{
				return this._takeIcon.gameObject;
			}
		}

		public MultiItemRack.SlotInfo[] _slots;

		public bool _allowFallback;

		public Renderer _takeIcon;

		public Renderer _addIcon;

		public GameObject _icons;

		public GameObject _toggleIcon;

		public RackTypes _type;

		[SerializeThis]
		private int[] _storedItemIds;

		private int _currentAddItem;

		private int _currentTakeItem;

		[Serializable]
		public class SlotInfo
		{
			public MultiItemRack.ItemInfo[] _items;

			public Transform _slotTr;
		}

		[Serializable]
		public class ItemInfo
		{
			[ItemIdPicker]
			public int _itemId;

			public MultiItemRack.RendererInfo[] _renderers;
		}

		[Serializable]
		public class RendererInfo
		{
			public Renderer _renderer;

			public Material _mat;
		}
	}
}
