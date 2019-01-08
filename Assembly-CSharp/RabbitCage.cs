using System;
using Bolt;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

[DoNotSerializePublic]
public class RabbitCage : EntityBehaviour<IRabbitCage>
{
	public override void Attached()
	{
		if (base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			base.state.RabbitCount = this.Rabbits;
		}
		base.state.AddCallback("RabbitCount", new PropertyCallbackSimple(this.RabbitCountChanged));
	}

	private void RabbitCountChanged()
	{
		this.Rabbits = base.state.RabbitCount;
		for (int i = 0; i < this.RabbitRender.Length; i++)
		{
			this.RabbitRender[i].SetActive(false);
		}
		for (int j = 0; j < this.RabbitsReal; j++)
		{
			this.RabbitRender[j].SetActive(true);
		}
	}

	private int RabbitsReal
	{
		get
		{
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
			{
				return base.state.RabbitCount;
			}
			return this.Rabbits;
		}
		set
		{
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
			{
				base.state.RabbitCount = value;
			}
			this.Rabbits = value;
		}
	}

	private void Awake()
	{
		base.enabled = false;
	}

	private void OnDeserialized()
	{
		for (int i = 0; i < this.RabbitsReal; i++)
		{
			this.RabbitRender[i].SetActive(true);
		}
	}

	private void GrabEnter()
	{
		base.enabled = true;
		if (LocalPlayer.Inventory && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._rabbitAliveItemId))
		{
			LocalPlayer.Inventory.BlockDrop = true;
		}
	}

	private void GrabExit()
	{
		if (LocalPlayer.Inventory)
		{
			LocalPlayer.Inventory.BlockDrop = false;
		}
		if (Scene.HudGui)
		{
			Scene.HudGui.HolderWidgets[5].ShutDown();
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		this.GrabExit();
	}

	private void Update()
	{
		bool flag = LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._rabbitAliveItemId);
		bool flag2 = LocalPlayer.Inventory.HasInNextSlot(Item.EquipmentSlot.RightHand, this._rabbitAliveItemId);
		bool flag3 = this.RabbitsReal > 0 && !flag && !flag2 && !LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.RightHand);
		if (flag3 && TheForest.Utils.Input.GetButtonDown("Take"))
		{
			LocalPlayer.Sfx.PlayItemCustomSfx(this._rabbitAliveItemId, true);
			this.TakeRabbit();
		}
		bool flag4 = this.RabbitsReal < 7 && flag;
		if (flag4 && TheForest.Utils.Input.GetButtonDown("Craft"))
		{
			Debug.Log("adding rabbit");
			LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, true);
			this.AddRabbit();
		}
		Scene.HudGui.HolderWidgets[5].Show(flag3, flag4, this.TakeIcon.transform);
	}

	private void TakeRabbit()
	{
		LocalPlayer.Inventory.AddItem(this._rabbitAliveItemId, 1, false, false, null);
		if (BoltNetwork.isRunning)
		{
			RabbitTake rabbitTake = RabbitTake.Create(GlobalTargets.OnlyServer);
			rabbitTake.Cage = base.entity;
			rabbitTake.Send();
		}
		else
		{
			this.RabbitRender[this.RabbitsReal - 1].SetActive(false);
			this.RabbitsReal--;
		}
	}

	private void AddRabbit()
	{
		if (BoltNetwork.isRunning)
		{
			RabbitAdd rabbitAdd = RabbitAdd.Create(GlobalTargets.OnlyServer);
			rabbitAdd.Cage = base.entity;
			rabbitAdd.Send();
		}
		else
		{
			this.RabbitsReal++;
			this.RabbitRender[this.RabbitsReal - 1].SetActive(true);
		}
	}

	public void BreedingChance()
	{
		if (!BoltNetwork.isClient && this.Rabbits >= 2 && this.Rabbits < 5 && UnityEngine.Random.Range(0, 9) == 0)
		{
			this.AddRabbit();
		}
	}

	public GameObject[] RabbitRender;

	public GameObject TakeIcon;

	public GameObject AddIcon;

	[ItemIdPicker(Item.Types.Equipment)]
	public int _rabbitAliveItemId;

	[SerializeThis]
	private int Rabbits;
}
