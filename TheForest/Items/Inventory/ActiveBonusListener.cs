using System;
using System.Collections.Generic;
using TheForest.Items.Craft;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[AddComponentMenu("Items/Inventory/Active Bonus Listener")]
	[DoNotSerializePublic]
	public class ActiveBonusListener : MonoBehaviour
	{
		
		private void Awake()
		{
			this._itemView.Properties.ActiveBonusChanged += this.OnActiveBonusChanged;
		}

		
		private void OnEnable()
		{
			this.OnActiveBonusChanged(this._itemView.ActiveBonus);
		}

		
		private void OnDestroy()
		{
			if (this._itemView != null && this._itemView.Properties != null)
			{
				this._itemView.Properties.ActiveBonusChanged -= this.OnActiveBonusChanged;
			}
		}

		
		private void OnDrawGizmosSelected()
		{
			if (this.GrabParentView)
			{
				this.GrabParentView = false;
				this._itemView = base.GetComponentInParent<InventoryItemView>();
			}
			if (this.GrabBoneAmmoSibbling)
			{
				this.GrabBoneAmmoSibbling = false;
				this._targetGOs.Add(base.transform.parent.GetChild(base.transform.GetSiblingIndex() - 1).gameObject);
			}
			if (this.GrabModernAmmoSibbling)
			{
				this.GrabModernAmmoSibbling = false;
				this._targetGOs.Clear();
				base.transform.SetAsLastSibling();
				this._targetGOs.Add(base.transform.parent.GetChild(base.transform.GetSiblingIndex() - 1).gameObject);
			}
			if (this.GrabParentRenderer)
			{
				this.GrabParentRenderer = false;
				this._targetObjects.Add(this._itemView.GetComponent<Renderer>());
			}
			if (this.AddBoneAmmoRendererRendererToView)
			{
				this.AddBoneAmmoRendererRendererToView = false;
				List<InventoryItemView.RendererDefinition> list = this._itemView._renderers.ToList<InventoryItemView.RendererDefinition>();
				list.Add(new InventoryItemView.RendererDefinition
				{
					_renderer = this._targetGOs.First<GameObject>().GetComponent<Renderer>(),
					_defaultMaterial = this._targetGOs.First<GameObject>().GetComponent<Renderer>().sharedMaterial
				});
				this._itemView._renderers = list.ToArray();
			}
		}

		
		private void OnActiveBonusChanged(WeaponStatUpgrade.Types bonus)
		{
			bool flag = bonus == this._bonusToActivate;
			if (this._inverse)
			{
				flag = !flag;
			}
			for (int i = 0; i < this._targetGOs.Count; i++)
			{
				if (this._targetGOs[i].activeSelf != flag)
				{
					this._targetGOs[i].SetActive(flag);
				}
			}
			for (int j = 0; j < this._targetObjects.Count; j++)
			{
				if (this._targetObjects[j] is Renderer)
				{
					(this._targetObjects[j] as Renderer).enabled = flag;
				}
				else if (this._targetObjects[j] is Behaviour)
				{
					(this._targetObjects[j] as Behaviour).enabled = flag;
				}
			}
		}

		
		public InventoryItemView _itemView;

		
		public List<GameObject> _targetGOs;

		
		public List<UnityEngine.Object> _targetObjects;

		
		public WeaponStatUpgrade.Types _bonusToActivate;

		
		public bool _inverse;

		
		public bool GrabParentView;

		
		public bool GrabBoneAmmoSibbling;

		
		public bool GrabParentRenderer;

		
		public bool AddBoneAmmoRendererRendererToView;

		
		public bool GrabModernAmmoSibbling;
	}
}
