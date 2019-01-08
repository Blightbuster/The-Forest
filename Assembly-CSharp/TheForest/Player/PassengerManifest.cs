using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	[DoNotSerializePublic]
	public class PassengerManifest : MonoBehaviour
	{
		private void Awake()
		{
			this._countText.text = this._foundPassengersIdsCount + "/" + this._foundGOs.Length;
		}

		private void Update()
		{
			if (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId) && TheForest.Utils.Input.GetButtonDown("Fire1"))
			{
				LocalPlayer.Inventory.EquipPreviousWeapon(true);
				base.StopAllCoroutines();
			}
		}

		private void OnDeserialized()
		{
			this._foundPassengersIds.RemoveRange(this._foundPassengersIdsCount, this._foundPassengersIds.Count - this._foundPassengersIdsCount);
			this._foundPassengersIdsCount = this._foundPassengersIds.Count;
			for (int i = 0; i < this._foundPassengersIds.Count; i++)
			{
				int passengerNum = PassengerDatabase.Instance.GetPassengerNum(this._foundPassengersIds[i]);
				if (passengerNum >= 0)
				{
					this._foundGOs[passengerNum].SetActive(true);
				}
			}
			this._countText.text = this._foundPassengersIdsCount + "/" + this._foundGOs.Length;
		}

		public bool FoundPassenger(int passengerId)
		{
			if (!LocalPlayer.AnimControl || !LocalPlayer.Inventory)
			{
				return false;
			}
			if (!LocalPlayer.AnimControl.upsideDown && LocalPlayer.Inventory.Owns(this._itemId, true) && !this._foundPassengersIds.Contains(passengerId))
			{
				int passengerNum = PassengerDatabase.Instance.GetPassengerNum(passengerId);
				if (passengerNum >= 0 && passengerNum < this._foundGOs.Length)
				{
					this._foundPassengersIds.Add(passengerId);
					this._foundPassengersIdsCount = this._foundPassengersIds.Count;
					if (this._foundPassengersIdsCount == 1 && !LocalPlayer.Vis.currentlyTargetted && Scene.SceneTracker.closeEnemies.Count == 0)
					{
						base.StartCoroutine(this.ToggleManifest());
					}
					Scene.HudGui.ShowFoundPassenger(passengerId, this._displayName[passengerNum]);
					this._foundGOs[passengerNum].SetActive(true);
				}
				this._countText.text = this._foundPassengersIdsCount + "/" + this._foundGOs.Length;
				EventRegistry.Player.Publish(TfEvent.FoundPassenger, this._foundPassengersIdsCount);
				return true;
			}
			return false;
		}

		public void Clone(PassengerManifest man)
		{
			this._foundPassengersIds = man._foundPassengersIds;
			this._foundPassengersIdsCount = man._foundPassengersIdsCount;
			this.OnDeserialized();
		}

		private IEnumerator ToggleManifest()
		{
			LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
			InventoryItemView iiv = LocalPlayer.Inventory.EquipmentSlotsPrevious[0];
			LocalPlayer.Inventory.Equip(this._itemId, false);
			LocalPlayer.Inventory.EquipmentSlotsPrevious[0] = iiv;
			yield return YieldPresets.WaitThreeSeconds;
			if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
			{
				LocalPlayer.Inventory.EquipPreviousWeapon(true);
			}
			yield break;
		}

		public PassengerDatabase _db;

		public GameObject[] _foundGOs;

		public string[] _displayName;

		public TextMesh _countText;

		[ItemIdPicker(Item.Types.Equipment)]
		public int _itemId;

		[SerializeThis]
		private List<int> _foundPassengersIds = new List<int>();

		[SerializeThis]
		private int _foundPassengersIdsCount;
	}
}
