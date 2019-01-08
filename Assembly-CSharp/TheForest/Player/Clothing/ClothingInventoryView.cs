using System;
using TheForest.Items.Inventory;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Clothing
{
	[AddComponentMenu("Items/Inventory/Clothing Inventory View")]
	[DoNotSerializePublic]
	public class ClothingInventoryView : MonoBehaviour
	{
		private void Start()
		{
			if (!this._hovered)
			{
				base.enabled = false;
			}
		}

		private void Update()
		{
			if (LocalPlayer.Inventory._craftingCog._upgradeCog.enabled || (LocalPlayer.Inventory._craftingCog.CraftOverride != null && LocalPlayer.Inventory._craftingCog.CraftOverride.IsCombining))
			{
				return;
			}
			if (TheForest.Utils.Input.GetButtonUp("Equip") && LocalPlayer.Clothing.SetWornOutfit(this._outfitId))
			{
				LocalPlayer.Clothing.RefreshVisibleClothing();
				LocalPlayer.Stats.CheckArmsStart();
				LocalPlayer.Inventory.Close();
			}
			if (TheForest.Utils.Input.GetButtonUp("Combine"))
			{
				LocalPlayer.Clothing.DropOutfit(this._outfitId);
			}
		}

		private void OnDisable()
		{
			if (this._hovered)
			{
				if (Scene.HudGui)
				{
					Scene.HudGui.HideClothingOutitInfoView(this._outfitId);
				}
				this.Highlight(false);
				this._hovered = false;
			}
			base.enabled = false;
		}

		private void OnDestroy()
		{
			if (this._renderers != null)
			{
				foreach (ClothingInventoryView.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Clear();
				}
			}
			this._renderers = null;
		}

		public virtual void OnDeserialized()
		{
			if (base.GetComponent<EmptyObjectIdentifier>())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}

		public void OnMouseExitCollider()
		{
			if (base.enabled)
			{
				base.enabled = false;
				VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.Inventory);
				Scene.HudGui.HideClothingOutitInfoView(this._outfitId);
			}
		}

		public void OnMouseEnterCollider()
		{
			if (!base.enabled && !LocalPlayer.Inventory._craftingCog._upgradeCog.enabled && (LocalPlayer.Inventory._craftingCog.CraftOverride == null || !LocalPlayer.Inventory._craftingCog.CraftOverride.IsCombining))
			{
				this._hovered = true;
				base.enabled = true;
				VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.InventoryHover);
				this.Highlight(true);
				Scene.HudGui.ShowClothingOutitInfoViewDelayed(this, (this._renderers.Length <= 0) ? null : this._renderers[0]._renderer);
				LocalPlayer.Sfx.PlayEvent(this._inventoryHoverSfx, PlayerInventory.SfxListenerSpacePosition(base.transform.position));
			}
		}

		public void Highlight(bool onoff)
		{
			if (this._renderers != null)
			{
				foreach (ClothingInventoryView.RendererDefinition rendererDefinition in this._renderers)
				{
					rendererDefinition.Highlight(onoff);
				}
			}
		}

		[ClothingItemIdPicker]
		public int _outfitId;

		public ClothingInventoryView.RendererDefinition[] _renderers;

		public string _inventoryHoverSfx;

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
