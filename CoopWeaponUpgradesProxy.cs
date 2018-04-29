using System;
using Bolt;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class CoopWeaponUpgradesProxy : EntityBehaviour<IWeaponUpgradeProxyState>
{
	
	public override void Attached()
	{
		if (!this.entity.isOwner)
		{
			this.SetTargetItem();
			if (!this._heldItemGo)
			{
				base.state.AddCallback("ItemId", new PropertyCallbackSimple(this.SetTargetItem));
				base.state.AddCallback("TargetPlayer", new PropertyCallbackSimple(this.SetTargetItem));
			}
			base.state.AddCallback("Token", new PropertyCallbackSimple(this.RefreshUpgradeViews));
			base.state.AddCallback("Cloth", new PropertyCallbackSimple(this.RefreshCloth));
			base.state.AddCallback("Color", new PropertyCallbackSimple(this.RefreshColor));
			base.state.AddCallback("AltRenderer", new PropertyCallbackSimple(this.RefreshAltRenderer));
		}
	}

	
	private void SetTargetItem()
	{
		if (!this._heldItemGo && base.state.TargetPlayer != null && base.state.ItemId > 0)
		{
			CoopPlayerRemoteSetup component = base.state.TargetPlayer.GetComponent<CoopPlayerRemoteSetup>();
			foreach (GameObject gameObject in component.rightHand.Available)
			{
				HeldItemIdentifier component2 = gameObject.GetComponent<HeldItemIdentifier>();
				if (component2 && component2._itemId == base.state.ItemId)
				{
					this._heldItemGo = gameObject;
					CoopWeaponUpgrades component3 = gameObject.GetComponent<CoopWeaponUpgrades>();
					this._mirror = component3._mirror;
					this._cloth = component3._cloth;
					this._painting = component3._painting;
					this._altRenderers = component3._altRenderers;
					if (base.state.Token != null)
					{
						this.RefreshUpgradeViews();
					}
					base.state.RemoveCallback("ItemId", new PropertyCallbackSimple(this.SetTargetItem));
					base.state.RemoveCallback("TargetPlayer", new PropertyCallbackSimple(this.SetTargetItem));
					break;
				}
			}
		}
	}

	
	private void RefreshUpgradeViews()
	{
		if (LocalPlayer.Inventory)
		{
			this.SetTargetItem();
			if (this._mirror)
			{
				foreach (object obj in this._mirror)
				{
					Transform transform = (Transform)obj;
					UnityEngine.Object.Destroy(transform.gameObject);
				}
				CoopWeaponUpgradesToken coopWeaponUpgradesToken = (CoopWeaponUpgradesToken)base.state.Token;
				foreach (UpgradeViewReceiver.UpgradeViewData upgradeViewData in coopWeaponUpgradesToken.Views)
				{
					Transform transform2 = UnityEngine.Object.Instantiate<Transform>(LocalPlayer.Inventory._craftingCog._upgradeCog.SupportedItemsCache[upgradeViewData.ItemId]._prefab);
					transform2.parent = this._mirror;
					transform2.localPosition = upgradeViewData.Position;
					transform2.localRotation = upgradeViewData.Rotation;
				}
			}
		}
	}

	
	private void RefreshCloth()
	{
		if (LocalPlayer.Inventory)
		{
			this.SetTargetItem();
			if (this._cloth)
			{
				this._cloth.enabled = base.state.Cloth;
				if (this._cloth.gameObject.activeSelf != this._cloth.enabled)
				{
					this._cloth.gameObject.SetActive(this._cloth.enabled);
				}
			}
		}
	}

	
	private void RefreshColor()
	{
		if (LocalPlayer.Inventory)
		{
			this.SetTargetItem();
			if (this._painting)
			{
				this._painting.Color = (EquipmentPainting.Colors)base.state.Color;
			}
		}
	}

	
	private void RefreshAltRenderer()
	{
		if (LocalPlayer.Inventory)
		{
			this.SetTargetItem();
			if (this._altRenderers != null)
			{
				for (int i = 0; i < this._altRenderers.Length; i++)
				{
					bool flag = i == base.state.AltRenderer;
					if (this._altRenderers[i].enabled != flag)
					{
						this._altRenderers[i].enabled = flag;
					}
				}
			}
		}
	}

	
	private GameObject _heldItemGo;

	
	private Transform _mirror;

	
	private Renderer _cloth;

	
	private EquipmentPainting _painting;

	
	private Renderer[] _altRenderers;
}
