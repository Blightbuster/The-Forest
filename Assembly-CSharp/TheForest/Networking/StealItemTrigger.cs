using System;
using TheForest.Items;
using TheForest.Items.Core;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Networking
{
	public class StealItemTrigger : MonoBehaviour
	{
		private void Awake()
		{
			base.enabled = false;
		}

		private void OnEnable()
		{
			if (this._sheenIcon)
			{
				this._sheenIcon.SetActive(true);
				this._pickupIcon.SetActive(false);
			}
		}

		private void OnDisable()
		{
			if (this._sheenIcon)
			{
				this._sheenIcon.SetActive(this._pickupIcon.activeSelf && base.gameObject.activeSelf);
				this._pickupIcon.SetActive(false);
			}
		}

		private void OnDestroy()
		{
		}

		private void Update()
		{
			bool flag = !this._heldStorage || this._heldStorage.UsedSlots.Count > 0;
			if (this._nextSteal < Time.realtimeSinceStartup)
			{
				if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false))
				{
					this._nextSteal = Time.realtimeSinceStartup + 5f;
					LocalPlayer.Sfx.PlayWhoosh();
					if (this._entity.isAttached && this._entity.source != LocalPlayer.Entity.source)
					{
						StealItem stealItem = StealItem.Create(this._entity.source);
						stealItem.thief = LocalPlayer.Entity;
						stealItem.robbed = this._entity;
						stealItem.Send();
					}
					else
					{
						ItemStorage componentInParent = this._entity.GetComponentInParent<ItemStorage>();
						if (componentInParent)
						{
							bool useAltWorldPrefab = LocalPlayer.Inventory.UseAltWorldPrefab;
							LocalPlayer.Inventory.UseAltWorldPrefab = true;
							for (int i = 0; i < componentInParent.UsedSlots.Count; i++)
							{
								LocalPlayer.Inventory.AddItem(componentInParent.UsedSlots[i]._itemId, componentInParent.UsedSlots[i]._amount, componentInParent.UsedSlots[i]._itemId != LocalPlayer.Inventory._defaultWeaponItemId, false, componentInParent.UsedSlots[i]._properties);
							}
							LocalPlayer.Inventory.FixMaxAmountBonuses();
							if (this._entity.isAttached)
							{
								BoltNetwork.Destroy(componentInParent.gameObject);
							}
							else
							{
								UnityEngine.Object.Destroy(componentInParent.gameObject);
							}
							LocalPlayer.Inventory.UseAltWorldPrefab = useAltWorldPrefab;
						}
					}
					this._sheenIcon.SetActive(false);
					this._pickupIcon.SetActive(false);
					base.gameObject.SetActive(false);
					base.enabled = false;
				}
				else if (!this._pickupIcon.activeSelf != flag || this._sheenIcon.activeSelf)
				{
					this._sheenIcon.SetActive(false);
					this._pickupIcon.SetActive(flag);
				}
			}
			else if (this._pickupIcon.activeSelf || this._sheenIcon.activeSelf != flag)
			{
				this._sheenIcon.SetActive(flag);
				this._pickupIcon.SetActive(false);
			}
		}

		public void ActivateIfIsStealableItem(GameObject held)
		{
			HeldItemIdentifier component = held.GetComponent<HeldItemIdentifier>();
			if (component)
			{
				int itemId = component._itemId;
				this._heldStorage = null;
				if (itemId == this._metalTinTrayItemId)
				{
					this._nextSteal = Time.realtimeSinceStartup + 0.5f;
					base.gameObject.SetActive(true);
					this._heldStorage = held.GetComponent<ItemStorage>();
					return;
				}
				for (int i = 0; i < this._stealableItems.Length; i++)
				{
					if (this._stealableItems[i] == itemId)
					{
						this._nextSteal = Time.realtimeSinceStartup + 0.5f;
						base.gameObject.SetActive(true);
						return;
					}
				}
				this._sheenIcon.SetActive(false);
				this._pickupIcon.SetActive(false);
				base.gameObject.SetActive(false);
				base.enabled = false;
			}
		}

		private void GrabEnter()
		{
			base.enabled = true;
		}

		private void GrabExit()
		{
			base.enabled = false;
		}

		public GameObject _sheenIcon;

		public GameObject _pickupIcon;

		public BoltEntity _entity;

		[ItemIdPicker]
		public int[] _stealableItems;

		[ItemIdPicker]
		public int _metalTinTrayItemId;

		private float _nextSteal;

		private ItemStorage _heldStorage;
	}
}
