using System;
using System.Collections;
using Bolt;
using TheForest.Audio;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Items.Utils;
using TheForest.Items.World;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UniLinq;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/World/Weapon Rack Slot")]
	public class WeaponRackSlot : MonoBehaviour
	{
		
		private void Awake()
		{
			if (!this._upsideDownBaseOffset)
			{
				this._upsideDownBaseOffset = base.transform;
			}
		}

		
		private void Start()
		{
			base.enabled = false;
		}

		
		private void Update()
		{
			if (!this._storedItemGO)
			{
				if (this._billboardRotate)
				{
					bool flag = this.CanToggleNextAddItem();
					if (flag && TheForest.Utils.Input.GetButtonDown("Rotate"))
					{
						LocalPlayer.Sfx.PlayWhoosh();
						this.ToggleNextAddItem();
					}
				}
				if (this._nextActionTime < Time.time && TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					this._nextActionTime = Time.time + 0.35f;
					if (this._fillMode == WeaponRackSlot.FillMode.EquipedInRightHand)
					{
						this.PlaceEquipment();
					}
					else
					{
						this.PlaceNonEquipment();
					}
				}
				else if (this._billboardRotate)
				{
					Scene.HudGui.RackWidgets[(int)this._type].ShowList(ItemDatabase.Items[this._currentAddItem]._id, this._addIcon.transform, SideIcons.Craft);
				}
				else
				{
					Scene.HudGui.RackWidgets[(int)this._type].ShowSingle(LocalPlayer.Inventory.RightHandOrNext._itemId, this._addIcon.transform, SideIcons.Craft);
				}
			}
			else if (this._nextActionTime < Time.time && TheForest.Utils.Input.GetButtonDown("Take"))
			{
				this._nextActionTime = Time.time + 0.35f;
				LocalPlayer.Sfx.PlayWhoosh();
				this.TakeCurrentItem();
			}
			else
			{
				Scene.HudGui.RackWidgets[(int)this._type].ShowSingle(this._storedItemId, this._takeIcon.transform, SideIcons.Take);
			}
		}

		
		private void OnEnable()
		{
			if (LocalPlayer.Inventory)
			{
				EventRegistry.Player.Subscribe(TfEvent.EquippedItem, new EventRegistry.SubscriberCallback(this.OnEquippedItem));
			}
		}

		
		private void OnDisable()
		{
			if (LocalPlayer.Inventory)
			{
				EventRegistry.Player.Unsubscribe(TfEvent.EquippedItem, new EventRegistry.SubscriberCallback(this.OnEquippedItem));
			}
			if (Scene.HudGui)
			{
				Scene.HudGui.RackWidgets[(int)this._type].Shutdown();
			}
		}

		
		private IEnumerator OnDeserialized()
		{
			while (!GlobalDataSaver.Ready)
			{
				yield return null;
			}
			yield return null;
			if (this._storedItemId > 0)
			{
				while (!LocalPlayer.Inventory)
				{
					yield return null;
				}
				yield return null;
				if (BoltNetwork.isRunning)
				{
					if (BoltNetwork.isServer)
					{
						BoltEntity be = base.GetComponentInParent<BoltEntity>();
						while (!be.isAttached)
						{
							yield return null;
						}
						RackAdd add = RackAdd.Create(GlobalTargets.OnlyServer);
						add.Slot = base.GetComponentInParent<CoopRack>().GetSlotIndex(this);
						add.Rack = be;
						add.ItemId = this._storedItemId;
						this._storedItemId = 0;
						add.Send();
					}
				}
				else
				{
					this.SpawnItemView();
				}
			}
			yield break;
		}

		
		private void OnEquippedItem(object o)
		{
			this.OnEquippedItem((int)o);
		}

		
		private void OnEquippedItem(int itemId)
		{
			this.GrabEnter();
		}

		
		private void GrabEnter()
		{
			if (this._storedItemGO)
			{
				this._takeIcon.transform.parent.position = base.transform.position;
				base.enabled = true;
			}
			else if (this._billboardRotate)
			{
				if (this._fillMode == WeaponRackSlot.FillMode.EquipedInRightHand && !LocalPlayer.Inventory.IsRightHandEmpty() && this.IsValidItem(LocalPlayer.Inventory.RightHandOrNext.ItemCache))
				{
					this._currentAddItem = ItemDatabase.ItemIndexById(LocalPlayer.Inventory.RightHandOrNext._itemId);
					this.UpdateIcons();
				}
				else
				{
					this._currentAddItem--;
					this.ToggleNextAddItem();
				}
				base.enabled = (this._currentAddItem >= 0 && this._currentAddItem < ItemDatabase.Items.Length && LocalPlayer.Inventory.Owns(ItemDatabase.Items[this._currentAddItem]._id, true));
				if (!this._offsetIcons)
				{
					this._addIcon.transform.parent.position = base.transform.position;
				}
				else
				{
					this._addIcon.transform.parent.position = base.transform.position + this.IconsOffset;
				}
			}
			else
			{
				if (this._fillMode != WeaponRackSlot.FillMode.EquipedInRightHand || LocalPlayer.Inventory.IsRightHandEmpty() || !this.IsValidItem(LocalPlayer.Inventory.RightHand.ItemCache))
				{
					if (this._fillMode != WeaponRackSlot.FillMode.AutoFromInventory)
					{
						return;
					}
					if (!this._itemWhiteList.Any((int wli) => LocalPlayer.Inventory._possessedItems.Any((InventoryItem pi) => pi._itemId == wli)))
					{
						return;
					}
				}
				if (!this._offsetIcons)
				{
					this._addIcon.transform.parent.position = base.transform.position;
				}
				else
				{
					this._addIcon.transform.parent.position = base.transform.position + this.IconsOffset;
				}
				base.enabled = true;
			}
		}

		
		private void GrabExit()
		{
			base.enabled = false;
			Scene.HudGui.RackWidgets[(int)this._type].Shutdown();
		}

		
		private void UpdateIcons()
		{
			Item item = ItemDatabase.Items[this._currentAddItem];
			this._addIcon.sharedMaterial = item._addMat;
			this._takeIcon.sharedMaterial = item._takeMat;
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

		
		private void PlaceEquipment()
		{
			Item item = null;
			if (!this._billboardRotate)
			{
				InventoryItemView inventoryItemView = LocalPlayer.Inventory.IsRightHandEmpty() ? null : LocalPlayer.Inventory.RightHand;
				if (inventoryItemView != null)
				{
					item = inventoryItemView.ItemCache;
				}
			}
			else
			{
				item = ItemDatabase.Items[this._currentAddItem];
			}
			if (item != null && this.IsValidItem(item))
			{
				if (LocalPlayer.Inventory.IsRightHandEmpty() || LocalPlayer.Inventory.RightHand._itemId != item._id || LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.RightHand))
				{
					if (!LocalPlayer.Inventory.RemoveItem(item._id, 1, false, true))
					{
						return;
					}
				}
				else if (!LocalPlayer.Inventory.ShuffleRemoveRightHandItem())
				{
					return;
				}
				if (BoltNetwork.isRunning && !this.hellDoorSlot)
				{
					RackAdd rackAdd = RackAdd.Create(GlobalTargets.OnlyServer);
					rackAdd.Slot = base.GetComponentInParent<CoopRack>().GetSlotIndex(this);
					rackAdd.Rack = base.GetComponentInParent<BoltEntity>();
					rackAdd.ItemId = item._id;
					rackAdd.Send();
				}
				else
				{
					if (this.hellDoorSlot)
					{
						this.Removed = false;
						this.Added = true;
					}
					this._storedItemId = item._id;
					this.SpawnItemView();
				}
				Sfx.Play(SfxInfo.SfxTypes.AddItem, base.transform, true);
				this._addIcon.gameObject.SetActive(false);
				if (!this._offsetIcons)
				{
					this._addIcon.transform.parent.position = base.transform.position;
				}
				else
				{
					this._takeIcon.transform.parent.position = base.transform.position + this.IconsOffset;
				}
			}
		}

		
		private void SpawnItemView()
		{
			if (this._positionningSource == WeaponRackSlot.PositionningSource.Held)
			{
				this.SpawnEquipmentItemView();
			}
			else
			{
				this.SpawnNonEquipmentItemView();
			}
		}

		
		private void SpawnEquipmentItemView()
		{
			Reparent.Locked = true;
			InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[this._storedItemId][0];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inventoryItemView._held);
			FakeParent component = inventoryItemView._held.GetComponent<FakeParent>();
			if (this._applyUpsideDownOffset && inventoryItemView.ItemCache._allowUpsideDownPlacement)
			{
				gameObject.transform.parent = this._upsideDownBaseOffset;
				gameObject.transform.localPosition = ((!component) ? inventoryItemView._held.transform.localPosition : component.RealLocalPosition) + inventoryItemView.ItemCache._upsideDownOffset;
			}
			else
			{
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = ((!component) ? inventoryItemView._held.transform.localPosition : component.RealLocalPosition);
			}
			gameObject.transform.localRotation = ((!component) ? inventoryItemView._held.transform.localRotation : component.RealLocalRotation);
			gameObject.transform.parent = base.transform;
			ItemUtils.FixItemPosition(gameObject, this._storedItemId);
			gameObject.layer = base.gameObject.layer;
			Animator animator = gameObject.GetComponent<Animator>() ?? gameObject.transform.GetChild(0).GetComponent<Animator>();
			if (animator)
			{
				UnityEngine.Object.Destroy(animator);
			}
			this.CleanUpComponents(gameObject.transform);
			gameObject.SetActive(true);
			foreach (MonoBehaviour obj in gameObject.GetComponentsInChildren<MonoBehaviour>())
			{
				UnityEngine.Object.Destroy(obj);
			}
			this._storedItemGO = gameObject;
			Reparent.Locked = false;
			this.OnItemAdded.Invoke(this._storedItemId);
		}

		
		private void OnBeginCollapse()
		{
			if (!BoltNetwork.isRunning || BoltNetwork.isServer)
			{
				foreach (Renderer renderer in this._storedItemGO.GetComponentsInChildren<Renderer>())
				{
					renderer.enabled = false;
				}
				Vector3 position = base.transform.position;
				ItemUtils.SpawnItem(this._storedItemId, position, Quaternion.identity, true);
			}
		}

		
		private void CleanUpComponents(Transform tr)
		{
			QuickSelectViewClearOut component = tr.GetComponent<QuickSelectViewClearOut>();
			if ((component && !component._childrenOnly) || tr.GetComponent<Collider>())
			{
				UnityEngine.Object.Destroy(tr.gameObject);
			}
			else
			{
				tr.gameObject.layer = base.gameObject.layer;
				for (int i = tr.childCount - 1; i >= 0; i--)
				{
					if (!component || !component._childrenOnly)
					{
						this.CleanUpComponents(tr.GetChild(i));
					}
					else
					{
						UnityEngine.Object.Destroy(tr.GetChild(i).gameObject);
					}
				}
			}
		}

		
		private void PlaceNonEquipment()
		{
			int num = this._itemWhiteList.FirstOrDefault((int wli) => LocalPlayer.Inventory._possessedItems.Any((InventoryItem pi) => pi._itemId == wli));
			if (num > 0 && LocalPlayer.Inventory.RemoveItem(num, 1, false, true))
			{
				if (BoltNetwork.isRunning)
				{
					RackAdd rackAdd = RackAdd.Create(GlobalTargets.OnlyServer);
					rackAdd.Slot = base.GetComponentInParent<CoopRack>().GetSlotIndex(this);
					rackAdd.Rack = base.GetComponentInParent<BoltEntity>();
					rackAdd.ItemId = num;
					rackAdd.Send();
				}
				else
				{
					this._storedItemId = num;
					this.SpawnItemView();
				}
				LocalPlayer.Sfx.PlayPutDown(base.gameObject);
				this._addIcon.gameObject.SetActive(false);
				if (!this._offsetIcons)
				{
					this._addIcon.transform.parent.position = base.transform.position;
				}
				else
				{
					this._takeIcon.transform.parent.position = base.transform.position + this.IconsOffset;
				}
			}
		}

		
		private void SpawnNonEquipmentItemView()
		{
			InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[this._storedItemId][0];
			InventoryItemView inventoryItemView2 = UnityEngine.Object.Instantiate<InventoryItemView>(inventoryItemView);
			Vector3 position = base.transform.position;
			position.y += inventoryItemView.transform.position.y - LocalPlayer.Inventory._inventoryGO.transform.position.y;
			inventoryItemView2.transform.localScale = inventoryItemView.transform.lossyScale;
			inventoryItemView2.transform.parent = base.transform;
			inventoryItemView2.transform.position = position;
			inventoryItemView2.transform.rotation = inventoryItemView.transform.rotation;
			inventoryItemView2.gameObject.layer = base.gameObject.layer;
			inventoryItemView2.gameObject.SetActive(true);
			this._storedItemGO = inventoryItemView2.gameObject;
			UnityEngine.Object.Destroy(inventoryItemView2.GetComponent<Collider>());
			UnityEngine.Object.Destroy(inventoryItemView2);
			VirtualCursorSnapNode component = this._storedItemGO.GetComponent<VirtualCursorSnapNode>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
			StoreInformation component2 = this._storedItemGO.GetComponent<StoreInformation>();
			if (component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			this.OnItemAdded.Invoke(this._storedItemId);
		}

		
		private void TakeCurrentItem()
		{
			if (this._fillMode == WeaponRackSlot.FillMode.EquipedInRightHand)
			{
				LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
				if (!LocalPlayer.Inventory.Equip(this._storedItemId, true))
				{
					ItemUtils.SpawnItem(this._storedItemId, this._storedItemGO.transform.position, this._storedItemGO.transform.rotation, true);
				}
			}
			else if (!LocalPlayer.Inventory.AddItem(this._storedItemId, 1, false, false, null))
			{
				ItemUtils.SpawnItem(this._storedItemId, this._storedItemGO.transform.position, this._storedItemGO.transform.rotation, true);
			}
			this._currentAddItem = ItemDatabase.ItemIndexById(this._storedItemId);
			this.UpdateIcons();
			UnityEngine.Object.Destroy(this._storedItemGO);
			this._storedItemGO = null;
			this._storedItemId = -1;
			if (this.hellDoorSlot)
			{
				this.Removed = true;
				this.Added = false;
			}
			this.OnItemRemoved.Invoke(this._storedItemId);
			if (BoltNetwork.isRunning && !this.hellDoorSlot)
			{
				RackRemove rackRemove = RackRemove.Create(GlobalTargets.OnlyServer);
				rackRemove.Slot = base.GetComponentInParent<CoopRack>().GetSlotIndex(this);
				rackRemove.Rack = base.GetComponentInParent<BoltEntity>();
				rackRemove.Send();
			}
			if (!this._offsetIcons)
			{
				this._addIcon.transform.parent.position = base.transform.position;
			}
			else
			{
				this._addIcon.transform.parent.position = base.transform.position + this.IconsOffset;
			}
		}

		
		private bool IsValidItem(Item item)
		{
			return item.MatchType(this._acceptedItemTypes) && (this._positionningSource != WeaponRackSlot.PositionningSource.Held || item._equipmentSlot == Item.EquipmentSlot.RightHand) && (this._forbidenItemTypes == (Item.Types)0 || !item.MatchType(this._forbidenItemTypes)) && item._maxAmount >= 0 && (this._itemWhiteList.Length == 0 || this._itemWhiteList.Contains(item._id)) && (this._itemBlackList.Length == 0 || !this._itemBlackList.Contains(item._id));
		}

		
		public void ItemIdChanged_Network(int newItemId)
		{
			if (this._storedItemId == newItemId)
			{
				return;
			}
			this._storedItemId = newItemId;
			if (newItemId > 0)
			{
				this.SpawnItemView();
			}
			else if (this._storedItemGO)
			{
				UnityEngine.Object.Destroy(this._storedItemGO);
				this._storedItemGO = null;
				this.OnItemRemoved.Invoke(this._storedItemId);
			}
		}

		
		
		
		public bool Added { get; set; }

		
		
		
		public bool Removed { get; set; }

		
		
		public int StoredItemId
		{
			get
			{
				return this._storedItemId;
			}
		}

		
		public WeaponRackSlot.FillMode _fillMode;

		
		public WeaponRackSlot.PositionningSource _positionningSource;

		
		[EnumFlags]
		public Item.Types _acceptedItemTypes = (Item.Types)(-1);

		
		[EnumFlags]
		public Item.Types _forbidenItemTypes = Item.Types.Projectile;

		
		[ItemIdPicker]
		public int[] _itemWhiteList;

		
		[ItemIdPicker]
		public int[] _itemBlackList;

		
		public GameObject _billboardRotate;

		
		public Renderer _addIcon;

		
		public Renderer _takeIcon;

		
		public bool _offsetIcons;

		
		public bool _applyUpsideDownOffset;

		
		public Transform _upsideDownBaseOffset;

		
		public bool hellDoorSlot;

		
		public RackTypes _type;

		
		[SerializeThis]
		private int _storedItemId = -1;

		
		private int _currentAddItem;

		
		private float _nextActionTime;

		
		private GameObject _storedItemGO;

		
		private readonly Vector3 IconsOffset = Vector3.down * 0.5f;

		
		public WeaponRackSlot.OnItemChanged OnItemAdded;

		
		public WeaponRackSlot.OnItemChanged OnItemRemoved;

		
		[Serializable]
		public class OnItemChanged : UnityEvent<int>
		{
		}

		
		public enum FillMode
		{
			
			EquipedInRightHand,
			
			AutoFromInventory
		}

		
		public enum PositionningSource
		{
			
			Held,
			
			Inventory
		}
	}
}
