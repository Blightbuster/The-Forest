using System;
using Bolt;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class MultiItemHolder : EntityEventListener<IMultiItemHolderState>
	{
		
		private void Awake()
		{
			base.enabled = false;
			this._itemsAmount = new int[this._acceptedItems.Length];
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
			if (BoltNetwork.isServer)
			{
			}
			if (BoltNetwork.isClient)
			{
			}
			if (this.CurrentItemAmount > 0 && LocalPlayer.Inventory.HasRoomFor(this.CurrentItemId, 1))
			{
				this.CurrentTakeIcon.SetActive(true);
				if (TheForest.Utils.Input.GetButtonDown("Take") && LocalPlayer.Inventory.AddItem(this.CurrentItemId, 1, false, false, null))
				{
					LocalPlayer.Sfx.PlayWhoosh();
					if (BoltNetwork.isRunning)
					{
						ItemHolderTakeItem itemHolderTakeItem = ItemHolderTakeItem.Create(GlobalTargets.OnlyServer);
						itemHolderTakeItem.Target = this.entity;
						itemHolderTakeItem.Player = LocalPlayer.Entity;
						itemHolderTakeItem.ContentType = this.Current;
						itemHolderTakeItem.Send();
					}
					else
					{
						this.CurrentItemAmount--;
						this.UpdateRenderers();
					}
				}
			}
			else if (this.CurrentTakeIcon.activeSelf)
			{
				this.CurrentTakeIcon.SetActive(false);
			}
			if (this.CurrentItemAmount < this.CurrentItemMaxCapacity && LocalPlayer.Inventory.Owns(this.CurrentItemId, true))
			{
				this.CurrentAddIcon.SetActive(true);
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					if (this._addItemEvent.Length > 0)
					{
						FMODCommon.PlayOneshot(this._addItemEvent, base.transform);
					}
					else
					{
						LocalPlayer.Sfx.PlayPutDown(base.gameObject);
					}
					if (LocalPlayer.Inventory.RemoveItem(this.CurrentItemId, 1, false, true))
					{
						if (BoltNetwork.isRunning)
						{
							ItemHolderAddItem itemHolderAddItem = ItemHolderAddItem.Create(GlobalTargets.OnlyServer);
							itemHolderAddItem.Target = this.entity;
							itemHolderAddItem.ContentType = this.Current;
							itemHolderAddItem.Send();
						}
						else
						{
							this.CurrentItemAmount++;
							this.UpdateRenderers();
						}
					}
				}
			}
			else
			{
				this.CurrentAddIcon.SetActive(false);
			}
		}

		
		private void OnDeserialized()
		{
			if (!BoltNetwork.isClient)
			{
				this.UpdateRenderers();
			}
		}

		
		public void GrabEnter()
		{
			base.enabled = (!BoltNetwork.isRunning || this.entity.isAttached);
		}

		
		public void GrabExit()
		{
			base.enabled = false;
			this.CurrentAddIcon.SetActive(false);
			this.CurrentTakeIcon.SetActive(false);
		}

		
		private void UpdateRenderers()
		{
			for (int i = 0; i < this._acceptedItems.Length; i++)
			{
				int j = 0;
				for (int k = 0; k < this._itemsAmount[i]; k++)
				{
					Renderer renderer = this._acceptedItems[i]._renderers[j++];
					if (!renderer.gameObject.activeSelf)
					{
						renderer.gameObject.SetActive(true);
					}
				}
				while (j < this._acceptedItems[i]._renderers.Length)
				{
					Renderer renderer2 = this._acceptedItems[i]._renderers[j++];
					if (renderer2.gameObject.activeSelf)
					{
						renderer2.gameObject.SetActive(false);
					}
				}
			}
		}

		
		public override void Attached()
		{
			base.state.AddCallback("ItemCount[]", new PropertyCallbackSimple(this.ItemCountChangedMP));
			if (BoltNetwork.isServer)
			{
				for (int i = 0; i < this._acceptedItems.Length; i++)
				{
					base.state.ItemCount[i] = this._itemsAmount[i];
				}
			}
		}

		
		public void TakeItemMP(BoltEntity targetPlayer, int contentType)
		{
			if (this._itemsAmount[contentType] > 0)
			{
				this.entity.Freeze(false);
				base.state.ItemCount[contentType] = (this._itemsAmount[contentType] = Mathf.Max(0, this._itemsAmount[contentType] - 1));
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
				itemRemoveFromPlayer.ItemId = this._acceptedItems[contentType]._itemId;
				itemRemoveFromPlayer.Send();
			}
		}

		
		public void AddItemMP(int contentType)
		{
			int num = this._acceptedItems[contentType]._renderers.Length;
			if (this._itemsAmount[contentType] < num)
			{
				base.state.ItemCount[contentType] = (this._itemsAmount[contentType] = Mathf.Min(this._itemsAmount[contentType] + 1, num));
				this.entity.Freeze(false);
			}
		}

		
		private void ItemCountChangedMP()
		{
			if (BoltNetwork.isClient)
			{
				for (int i = 0; i < this._acceptedItems.Length; i++)
				{
					this._itemsAmount[i] = base.state.ItemCount[i];
				}
			}
			this.UpdateRenderers();
		}

		
		
		
		public int Current { get; set; }

		
		
		private int CurrentItemId
		{
			get
			{
				return this._acceptedItems[this.Current]._itemId;
			}
		}

		
		
		
		private int CurrentItemAmount
		{
			get
			{
				return this._itemsAmount[this.Current];
			}
			set
			{
				this._itemsAmount[this.Current] = value;
			}
		}

		
		
		private int CurrentItemMaxCapacity
		{
			get
			{
				return this._acceptedItems[this.Current]._renderers.Length;
			}
		}

		
		
		private Renderer[] CurrentItemRenderers
		{
			get
			{
				return this._acceptedItems[this.Current]._renderers;
			}
		}

		
		
		private GameObject CurrentTakeIcon
		{
			get
			{
				return this._acceptedItems[this.Current]._takeIcon;
			}
		}

		
		
		private GameObject CurrentAddIcon
		{
			get
			{
				return this._acceptedItems[this.Current]._addIcon;
			}
		}

		
		
		private int TotalHeld
		{
			get
			{
				int num = 0;
				foreach (int num2 in this._itemsAmount)
				{
					num += num2;
				}
				return num;
			}
		}

		
		public MultiItemHolder.ItemInfo[] _acceptedItems;

		
		[Header("FMOD")]
		public string _addItemEvent;

		
		[SerializeThis]
		private int[] _itemsAmount;

		
		private bool _hasPreloaded;

		
		[Serializable]
		public class ItemInfo
		{
			
			[ItemIdPicker]
			public int _itemId;

			
			public GameObject _takeIcon;

			
			public GameObject _addIcon;

			
			public Renderer[] _renderers;
		}
	}
}
