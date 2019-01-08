using System;
using System.Collections;
using Bolt;
using TheForest.World;
using UnityEngine;

public class CoopHellDoors : EntityBehaviour<IWorldHellDoors>
{
	private void Awake()
	{
		CoopHellDoors.Instance = this;
	}

	private void OnDestroy()
	{
		LoadSave.OnGameStart -= this.DoorItemsChanged;
	}

	public override void Attached()
	{
		base.state.AddCallback("NewProperty[]", new PropertyCallbackSimple(this.DoorItemsChanged));
		if (base.entity.isOwner)
		{
			base.StartCoroutine(this.LoadSaveRoutine());
		}
		else
		{
			LoadSave.OnGameStart += this.DoorItemsChanged;
		}
	}

	private IEnumerator LoadSaveRoutine()
	{
		yield return null;
		yield return null;
		for (int i = 0; i < base.state.NewProperty.Length; i++)
		{
			int num = Mathf.Min(base.state.NewProperty[i].Items.Length, this.Doors[i]._slots.Length);
			for (int j = 0; j < num; j++)
			{
				if (base.state.NewProperty[i].Items[j] != this.Doors[i]._slots[j].StoredItemId)
				{
					this.Doors[i]._slots[j].Added = false;
					AddItemToDoor addItemToDoor = AddItemToDoor.Create(GlobalTargets.OnlyServer);
					addItemToDoor.Door = i;
					addItemToDoor.Slot = j;
					addItemToDoor.Item = this.Doors[i]._slots[j].StoredItemId;
					addItemToDoor.Send();
				}
			}
		}
		yield break;
	}

	private void OnWeightChanged()
	{
		if (base.entity.IsAttached())
		{
			if (base.entity.isFrozen)
			{
				base.entity.Freeze(false);
			}
			for (int i = 0; i < base.state.NewProperty.Length; i++)
			{
				int num = Mathf.Min(base.state.NewProperty[i].Items.Length, this.Doors[i]._slots.Length);
				for (int j = 0; j < num; j++)
				{
					if (base.state.NewProperty[i].Items[j] != this.Doors[i]._slots[j].StoredItemId)
					{
						if (this.Doors[i]._slots[j].Removed)
						{
							this.Doors[i]._slots[j].Removed = false;
							AddItemToDoor addItemToDoor = AddItemToDoor.Create(GlobalTargets.OnlyServer);
							addItemToDoor.Door = i;
							addItemToDoor.Slot = j;
							addItemToDoor.Item = -1;
							addItemToDoor.Send();
						}
						else if (this.Doors[i]._slots[j].Added)
						{
							this.Doors[i]._slots[j].Added = false;
							AddItemToDoor addItemToDoor2 = AddItemToDoor.Create(GlobalTargets.OnlyServer);
							addItemToDoor2.Door = i;
							addItemToDoor2.Slot = j;
							addItemToDoor2.Item = this.Doors[i]._slots[j].StoredItemId;
							addItemToDoor2.Send();
						}
						this._lastSend = Time.time;
					}
				}
			}
		}
	}

	private void DoorItemsChanged()
	{
		for (int i = 0; i < base.state.NewProperty.Length; i++)
		{
			int num = Mathf.Min(base.state.NewProperty[i].Items.Length, this.Doors[i]._slots.Length);
			for (int j = 0; j < num; j++)
			{
				this.Doors[i]._slots[j].ItemIdChanged_Network(base.state.NewProperty[i].Items[j]);
			}
		}
	}

	private float _lastSend;

	public static CoopHellDoors Instance;

	public DoorWeightOpener[] Doors;
}
