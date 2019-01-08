using System;
using System.Collections;
using Bolt;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheForest.Items.World
{
	public class QuickSelectViews : EntityBehaviour<IPlayerState>
	{
		public void Awake()
		{
			this.Init();
		}

		private void OnEnable()
		{
			if (this._localPlayer)
			{
				this.ShowLocalPlayerViews();
			}
		}

		public void OnDestroy()
		{
			if (this._localPlayer && this._initDone)
			{
				this._initDone = false;
				EventRegistry.Player.Unsubscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.OnItemAdded));
				EventRegistry.Player.Unsubscribe(TfEvent.EquippedItem, new EventRegistry.SubscriberCallback(this.OnItemChange));
				EventRegistry.Player.Unsubscribe(TfEvent.RemovedItem, new EventRegistry.SubscriberCallback(this.OnItemChange));
			}
		}

		public void ShowLocalPlayerViews()
		{
			if (SteamDSConfig.isDedicatedServer)
			{
				return;
			}
			if (this)
			{
				this.ShowViews(LocalPlayer.Inventory.QuickSelectItemIds);
			}
			else
			{
				this.OnDestroy();
			}
		}

		public void ShowViews(IEnumerable itemIds)
		{
			int num = 0;
			IEnumerator enumerator = itemIds.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					int num2 = (int)obj;
					bool flag = num2 > 0;
					Item item = (!flag) ? null : ItemDatabase.ItemById(num2);
					if (flag && this._localPlayer)
					{
						if (!item.MatchType(Item.Types.Equipment))
						{
							flag = false;
						}
						else
						{
							flag = LocalPlayer.Inventory.Owns(num2, false);
							if (flag && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
							{
								flag = (!LocalPlayer.Inventory.HasInSlot(item._equipmentSlot, num2) && !LocalPlayer.Inventory.HasInNextSlot(item._equipmentSlot, num2));
							}
							if (!flag && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory)
							{
								foreach (ReceipeIngredient receipeIngredient in LocalPlayer.Inventory._craftingCog.Ingredients)
								{
									if (receipeIngredient._itemID == num2)
									{
										flag = true;
										break;
									}
								}
							}
						}
					}
					bool flag2 = this._slotsCurrentId[num] > 0 && this._slots[num].childCount > 0;
					bool flag3 = flag2 && this._slotsCurrentId[num] != num2;
					if (flag != flag2 || flag3)
					{
						if (!flag || flag3)
						{
							QuickSelectViewsPool.Despawn(this._slotsCurrentId[num], this._slots[num].GetChild(0), this._shadowsOnly);
						}
						if (flag && LocalPlayer.Inventory && !QuickSelectViewsPool.Spawn(num2, this._slots[num], this._shadowsOnly))
						{
							if (item.MatchType(Item.Types.Equipment))
							{
								this.SpawnEquipment(num2, num);
							}
							else
							{
								this.SpawnNonEquipment(num2, num);
							}
						}
						this._slotsCurrentId[num] = num2;
					}
					num++;
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
		}

		public void OnItemChange(object o)
		{
			this.OnItemChange((int)o);
		}

		public void OnItemChange(int itemId)
		{
			this.ShowLocalPlayerViews();
		}

		public void OnItemAdded(object o)
		{
			if (this)
			{
				this.ShowLocalPlayerViews();
			}
		}

		private void Init()
		{
			if (!this._initDone && !CoopPeerStarter.DedicatedHost)
			{
				this._initDone = true;
				if (this._slotsCurrentId == null)
				{
					this._slotsCurrentId = new int[this._slots.Length];
				}
				if (this._localPlayer)
				{
					EventRegistry.Player.Subscribe(TfEvent.EquippedItem, new EventRegistry.SubscriberCallback(this.OnItemChange));
					EventRegistry.Player.Subscribe(TfEvent.RemovedItem, new EventRegistry.SubscriberCallback(this.OnItemChange));
					EventRegistry.Player.Subscribe(TfEvent.AddedItem, new EventRegistry.SubscriberCallback(this.OnItemAdded));
				}
			}
		}

		private void SpawnEquipment(int itemId, int slotId)
		{
			Reparent.Locked = true;
			InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inventoryItemView._held);
			FakeParent component = inventoryItemView._held.GetComponent<FakeParent>();
			gameObject.transform.parent = this._slots[slotId];
			gameObject.transform.localPosition = ((!component) ? inventoryItemView._held.transform.localPosition : component.RealLocalPosition);
			gameObject.transform.localRotation = ((!component) ? inventoryItemView._held.transform.localRotation : component.RealLocalRotation);
			gameObject.layer = base.gameObject.layer;
			Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
			if (componentInChildren)
			{
				UnityEngine.Object.Destroy(componentInChildren);
			}
			this.CleanUpComponents(gameObject.transform);
			gameObject.SetActive(true);
			foreach (MonoBehaviour obj in gameObject.GetComponentsInChildren<MonoBehaviour>())
			{
				UnityEngine.Object.Destroy(obj);
			}
			Reparent.Locked = false;
			if (inventoryItemView._materialWhenAttachedToPack != null)
			{
				MeshRenderer meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
				if (!meshRenderer)
				{
					meshRenderer = gameObject.GetComponent<MeshRenderer>();
				}
				if (meshRenderer)
				{
					Material[] sharedMaterials = meshRenderer.sharedMaterials;
					sharedMaterials[0] = inventoryItemView._materialWhenAttachedToPack;
					meshRenderer.sharedMaterials = sharedMaterials;
				}
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
				if (this._shadowsOnly)
				{
					Renderer component2 = tr.GetComponent<Renderer>();
					if (component2)
					{
						component2.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
					}
				}
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

		private void SpawnNonEquipment(int itemId, int slotId)
		{
			InventoryItemView inventoryItemView = LocalPlayer.Inventory.InventoryItemViewsCache[itemId][0];
			InventoryItemView inventoryItemView2 = UnityEngine.Object.Instantiate<InventoryItemView>(inventoryItemView);
			Vector3 zero = Vector3.zero;
			zero.y += inventoryItemView.transform.position.y - LocalPlayer.Inventory._inventoryGO.transform.position.y;
			inventoryItemView2.transform.localScale = inventoryItemView.transform.lossyScale;
			inventoryItemView2.transform.parent = this._slots[slotId];
			inventoryItemView2.transform.localPosition = zero;
			inventoryItemView2.transform.localRotation = inventoryItemView.transform.localRotation * Quaternion.Euler(0f, 0f, 90f);
			inventoryItemView2.gameObject.layer = base.gameObject.layer;
			UnityEngine.Object.DestroyImmediate(inventoryItemView2.GetComponent<Collider>());
			this.CleanUpComponents(inventoryItemView2.transform);
			inventoryItemView2.gameObject.SetActive(true);
			UnityEngine.Object.Destroy(inventoryItemView2.GetComponent<Collider>());
			VirtualCursorSnapNode component = inventoryItemView2.GetComponent<VirtualCursorSnapNode>();
			if (component)
			{
				UnityEngine.Object.Destroy(component);
			}
			StoreInformation component2 = inventoryItemView2.GetComponent<StoreInformation>();
			if (component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			UnityEngine.Object.Destroy(inventoryItemView2);
		}

		public Transform[] _slots;

		public bool _localPlayer;

		public bool _shadowsOnly;

		private bool _initDone;

		private int[] _slotsCurrentId;
	}
}
