using System;
using System.Collections;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	public class InventoryItemViewProxy : MonoBehaviour
	{
		
		private void Update()
		{
			if (!this._targetView || !this._targetView.gameObject.activeInHierarchy)
			{
				this._targetView = null;
			}
		}

		
		private IEnumerator OnMouseEnterCollider()
		{
			bool hasValidTargetView = this._targetView && this._targetView.gameObject.activeInHierarchy;
			if (hasValidTargetView)
			{
				this._exited = false;
				yield return null;
				if (this._targetView && !this._exited)
				{
					this._targetView.OnMouseEnterCollider();
				}
			}
			yield break;
		}

		
		private void OnMouseExitCollider()
		{
			bool flag = this._targetView && this._targetView.gameObject.activeInHierarchy;
			this._exited = true;
			if (flag)
			{
				this._targetView.OnMouseExitCollider();
			}
		}

		
		public void Unset()
		{
			bool flag = this._targetView && this._targetView.gameObject.activeInHierarchy;
			if (flag)
			{
				this._targetView.OnMouseExitCollider();
			}
			this._targetView = null;
		}

		
		public InventoryItemView _targetView;

		
		private bool _exited;
	}
}
