using System;
using Bolt;
using TheForest.Items;
using TheForest.Tools;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class TrophyMaker : EntityBehaviour
	{
		private void Awake()
		{
			base.enabled = false;
		}

		private void Update()
		{
			bool flag = LocalPlayer.Inventory.RightHand && this._itemIdWhiteList.Contains(LocalPlayer.Inventory.RightHand._itemId);
			if (flag)
			{
				this._sheenBillboard.SetActive(false);
				this._pickupBillboard.SetActive(true);
				int itemId = LocalPlayer.Inventory.RightHand._itemId;
				if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false) && LocalPlayer.Inventory.RemoveItem(itemId, 1, false, true))
				{
					LocalPlayer.Sfx.PlayTwinkle();
					if (BoltNetwork.isRunning)
					{
						PlaceTrophy placeTrophy = global::PlaceTrophy.Create(GlobalTargets.OnlyServer);
						placeTrophy.Maker = base.entity;
						placeTrophy.ItemId = itemId;
						placeTrophy.Send();
						EventRegistry.Achievements.Publish(TfEvent.Achievements.CreatedTrophy, itemId);
					}
					else
					{
						this.PlaceTrophy(itemId);
						EventRegistry.Achievements.Publish(TfEvent.Achievements.CreatedTrophy, itemId);
					}
				}
			}
			else
			{
				this._sheenBillboard.SetActive(true);
				this._pickupBillboard.SetActive(false);
				base.enabled = false;
			}
		}

		private void GrabEnter()
		{
			base.enabled = (LocalPlayer.Inventory.RightHand && this._itemIdWhiteList.Contains(LocalPlayer.Inventory.RightHand._itemId));
		}

		private void GrabExit()
		{
			this._sheenBillboard.SetActive(true);
			this._pickupBillboard.SetActive(false);
			base.enabled = false;
		}

		public void PlaceTrophy(int itemId)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(Prefabs.Instance.TrophyPrefabs.First((Prefabs.TrophyPrefab tp) => tp._itemId == itemId)._prefab, base.transform.parent.position, base.transform.parent.rotation);
			transform.parent = base.transform.parent.parent;
			if (BoltNetwork.isRunning)
			{
				BoltNetwork.Attach(transform.gameObject);
			}
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}

		[ItemIdPicker]
		public int[] _itemIdWhiteList;

		public GameObject _sheenBillboard;

		public GameObject _pickupBillboard;
	}
}
