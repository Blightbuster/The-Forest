using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.World;
using UnityEngine;


public class CoopRack : EntityBehaviour<IWeaponRackState>
{
	
	public override void Attached()
	{
		base.state.AddCallback("Slots[]", new PropertyCallback(this.SlotChanged));
		if (this.entity.isOwner)
		{
			base.StartCoroutine(this.UpdateSlots());
		}
	}

	
	private IEnumerator UpdateSlots()
	{
		while (this.entity.isAttached)
		{
			for (int i = 0; i < this.Slots.Length; i++)
			{
				base.state.Slots[i] = this.Slots[i].StoredItemId;
			}
			yield return YieldPresets.WaitThreeSeconds;
		}
		yield break;
	}

	
	public int GetSlotIndex(WeaponRackSlot weaponRackSlot)
	{
		for (int i = 0; i < this.Slots.Length; i++)
		{
			if (object.ReferenceEquals(this.Slots[i], weaponRackSlot))
			{
				return i;
			}
		}
		return -1;
	}

	
	private void SlotChanged(IState _, string path, ArrayIndices indices)
	{
		for (int i = 0; i < this.Slots.Length; i++)
		{
			this.Slots[i].ItemIdChanged_Network(base.state.Slots[i]);
		}
	}

	
	[SerializeField]
	private WeaponRackSlot[] Slots;
}
