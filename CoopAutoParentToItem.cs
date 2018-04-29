using System;
using Bolt;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class CoopAutoParentToItem : EntityBehaviour<IWeaponFire>
{
	
	public override void Attached()
	{
		if (base.entity && base.entity.isAttached)
		{
			if (base.entity.isOwner)
			{
				HeldItemIdentifier componentInParent = base.GetComponentInParent<HeldItemIdentifier>();
				base.state.Position = ((!this.attachToCloth) ? (componentInParent.transform.position - base.transform.position) : base.transform.parent.localPosition);
				base.state.Rotation = Quaternion.Inverse(componentInParent.transform.rotation) * base.transform.rotation;
				base.state.TargetPlayer = LocalPlayer.Entity;
			}
			else
			{
				base.state.AddCallback("TargetPlayer", new PropertyCallbackSimple(this.SetParent));
				base.state.AddCallback("Position", new PropertyCallbackSimple(this.SetPosition));
				base.state.AddCallback("Rotation", new PropertyCallbackSimple(this.SetRotation));
				this.SetParent();
			}
		}
	}

	
	private void SetParent()
	{
		if (base.state.TargetPlayer)
		{
			CoopPlayerRemoteSetup component = base.state.TargetPlayer.GetComponent<CoopPlayerRemoteSetup>();
			Item.EquipmentSlot slot = this._slot;
			Transform transform;
			if (slot != Item.EquipmentSlot.RightHand)
			{
				if (slot != Item.EquipmentSlot.LeftHand)
				{
					transform = null;
				}
				else
				{
					transform = component.leftHand.ActiveItem;
				}
			}
			else
			{
				coopClientHeldFirePos componentInChildren = component.rightHand.ActiveItem.GetComponentInChildren<coopClientHeldFirePos>();
				if (componentInChildren)
				{
					transform = componentInChildren.transform;
				}
				else
				{
					transform = component.rightHand.ActiveItem;
				}
			}
			if (base.entity && base.entity.isAttached && transform)
			{
				base.transform.parent = transform;
				base.transform.localPosition = base.state.Position;
				base.transform.localRotation = base.state.Rotation;
			}
		}
		else
		{
			base.transform.parent = null;
		}
	}

	
	private void SetPosition()
	{
		if (base.entity && base.entity.isAttached && base.transform.parent)
		{
			base.transform.localPosition = base.state.Position;
		}
	}

	
	private void SetRotation()
	{
		if (base.entity && base.entity.isAttached && base.transform.parent)
		{
			base.transform.localRotation = base.state.Rotation;
		}
	}

	
	public Item.EquipmentSlot _slot;

	
	public bool attachToCloth;
}
