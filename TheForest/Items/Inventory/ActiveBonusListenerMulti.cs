using System;
using System.Collections.Generic;
using TheForest.Items.Craft;
using UniLinq;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[DoNotSerializePublic]
	public class ActiveBonusListenerMulti : MonoBehaviour
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
			this._itemView.Properties.ActiveBonusChanged -= this.OnActiveBonusChanged;
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
			bool flag = false;
			foreach (WeaponStatUpgrade.Types types in this._bonusToActivate)
			{
				if (types == bonus)
				{
					flag = true;
					break;
				}
			}
			if (this._inverse)
			{
				flag = !flag;
			}
			for (int j = 0; j < this._targetGOs.Count; j++)
			{
				if (this._targetGOs[j].activeSelf != flag)
				{
					this._targetGOs[j].SetActive(flag);
				}
			}
			for (int k = 0; k < this._targetObjects.Count; k++)
			{
				if (this._targetObjects[k] is Renderer)
				{
					(this._targetObjects[k] as Renderer).enabled = flag;
				}
				else if (this._targetObjects[k] is Behaviour)
				{
					(this._targetObjects[k] as Behaviour).enabled = flag;
				}
			}
		}

		
		public InventoryItemView _itemView;

		
		public List<GameObject> _targetGOs;

		
		public List<UnityEngine.Object> _targetObjects;

		
		public WeaponStatUpgrade.Types[] _bonusToActivate;

		
		public bool _inverse;

		
		public bool GrabParentView;

		
		public bool GrabBoneAmmoSibbling;

		
		public bool GrabParentRenderer;

		
		public bool AddBoneAmmoRendererRendererToView;
	}
}
