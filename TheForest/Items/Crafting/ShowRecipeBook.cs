using System;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Crafting
{
	
	public class ShowRecipeBook : MonoBehaviour
	{
		
		private void Start()
		{
			if (!this._spreadTrigger)
			{
				this._spreadTrigger = base.transform.GetComponent<inventoryItemSpreadTrigger>();
			}
		}

		
		private void Update()
		{
			if (Scene.HudGui)
			{
				bool activeSelf = Scene.HudGui._receipeList.transform.GetChild(0).gameObject.activeSelf;
				this.ToggleRenderers(activeSelf);
			}
		}

		
		private void OnDisable()
		{
			if (this._hovered)
			{
				if (Scene.HudGui)
				{
					Scene.HudGui.HideRecipeList();
				}
				this.Highlight(false);
				this._hovered = false;
			}
		}

		
		private void OnDestroy()
		{
			if (this._renderers != null)
			{
				foreach (ShowRecipeBook.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Clear();
				}
			}
			this._renderers = null;
		}

		
		public void OnMouseExitCollider()
		{
			this.OnDisable();
			VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.Inventory);
		}

		
		public void OnMouseEnterCollider()
		{
			if (!this._hovered && !LocalPlayer.Inventory._craftingCog._upgradeCog.enabled && (LocalPlayer.Inventory._craftingCog.CraftOverride == null || !LocalPlayer.Inventory._craftingCog.CraftOverride.IsCombining))
			{
				this._hovered = true;
				this.Highlight(true);
				VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.InventoryHover);
				Scene.HudGui.ShowRecipeListDelayed();
			}
		}

		
		public void Highlight(bool onoff)
		{
			if (this._renderers != null)
			{
				foreach (ShowRecipeBook.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Highlight(onoff);
				}
			}
		}

		
		public void ToggleRenderers(bool onoff)
		{
			if (this._renderers != null)
			{
				foreach (ShowRecipeBook.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Toggle(onoff);
				}
			}
			if (this._spreadTrigger)
			{
				this._spreadTrigger.toggleActive(onoff);
			}
			if (this._collider)
			{
				this._collider.enabled = onoff;
			}
		}

		
		public ShowRecipeBook.RendererDefinition[] _renderers;

		
		public inventoryItemSpreadTrigger _spreadTrigger;

		
		public Collider _collider;

		
		private bool _hovered;

		
		[Serializable]
		public class RendererDefinition
		{
			
			public void Highlight(bool onoff)
			{
				if (onoff && this._defaultMaterial != this._renderer.sharedMaterial && this._renderer.sharedMaterial != this._selectedMaterial)
				{
					this._defaultMaterial = this._renderer.sharedMaterial;
				}
				this._renderer.sharedMaterial = ((!onoff) ? this._defaultMaterial : this._selectedMaterial);
			}

			
			public void Toggle(bool onoff)
			{
				if (this._renderer.enabled != onoff)
				{
					this._renderer.enabled = onoff;
				}
			}

			
			public void Clear()
			{
				this._renderer = null;
				this._defaultMaterial = null;
				this._selectedMaterial = null;
			}

			
			public Renderer _renderer;

			
			public Material _defaultMaterial;

			
			public Material _selectedMaterial;
		}
	}
}
