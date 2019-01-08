using System;
using System.Collections;
using TheForest.Items.Inventory;
using UnityEngine;

namespace TheForest.Items.World
{
	[AddComponentMenu("Items/World/Decaying PickUp")]
	public class DecayingPickUp : PickUp
	{
		protected override void Awake()
		{
			base.Awake();
			base.StartCoroutine(this.DelayedAwake());
		}

		private IEnumerator DelayedAwake()
		{
			yield return null;
			yield break;
		}

		protected override bool MainEffect()
		{
			if (base.MainEffect())
			{
				if (DecayingInventoryItemView.LastUsed != null)
				{
					DecayingInventoryItemView.LastUsed.SetDecayState(this._state);
					DecayingInventoryItemView.LastUsed = null;
				}
				return true;
			}
			return false;
		}

		public DecayingInventoryItemView.DecayStates _state;
	}
}
