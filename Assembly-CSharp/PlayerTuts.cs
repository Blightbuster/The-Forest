using System;
using System.Collections;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

[DoNotSerializePublic]
public class PlayerTuts : MonoBehaviour
{
	private void Start()
	{
		this.NextLightTutTime = Time.realtimeSinceStartup + this._lighterTutDelay * 3f;
		base.Invoke("ShowSprint", 300f);
	}

	private void Update()
	{
		if (Scene.HudGui == null)
		{
			return;
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		bool flag = LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, LocalPlayer.Inventory.LastLight._itemId) || LocalPlayer.Inventory.IsWeaponBurning || LocalPlayer.Stats.Dead;
		if (LocalPlayer.IsInCaves && !flag && !LocalPlayer.WaterViz.InWater && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			this.TryShowLighter();
		}
		else if (Clock.Dark && !flag && this.ShouldShowLighter && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			this.TryShowLighter();
			this.ShouldShowLighter = false;
		}
		else if (((!LocalPlayer.IsInCaves && !Clock.Dark) || LocalPlayer.WaterViz.InWater || LocalPlayer.Stats.Dead || LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World) && Scene.HudGui.Tut_Lighter.activeSelf)
		{
			this.HideLighter();
		}
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && this.HoldingBurnableItem())
		{
			this.ShowMolotovTut();
		}
		else
		{
			this.HideMolotovTut();
		}
		if (!this.craftingTutDone)
		{
			bool flag2 = true;
			for (int i = 0; i < this._craftTutReceiepeItemId.Length; i++)
			{
				if (!LocalPlayer.Inventory.Owns(this._craftTutReceiepeItemId[i], true))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2 && !this.craftingTutDone && LocalPlayer.IsInWorld)
			{
				this.CraftingTut();
			}
		}
		if (!this.axeTutDone)
		{
			for (int j = 0; j < this._axesItemId.Length; j++)
			{
				if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._axesItemId[j]))
				{
					this.ShowAxeTut();
					break;
				}
			}
		}
		if (!this.sleddingTutDone && !this.Showing && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._turtleShellItemId))
		{
			this.ShowSleddingTut();
		}
	}

	private bool HoldingBurnableItem()
	{
		return LocalPlayer.ActiveBurnableItem != null && LocalPlayer.ActiveBurnableItem.IsUnlit();
	}

	public void ToggleVisibility(bool visible)
	{
		Scene.HudGui.Grid.gameObject.SetActive(visible);
	}

	public void GotBook()
	{
		if (!this.Showing)
		{
			this.Showing = true;
			Scene.HudGui.Tut_OpenBook.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
		else
		{
			base.Invoke("GotBook", 5f);
		}
	}

	public void ShowMolotovTut()
	{
		Scene.HudGui.Tut_MolotovTutorial.SetActive(true);
	}

	public void HideMolotovTut()
	{
		Scene.HudGui.Tut_MolotovTutorial.SetActive(false);
	}

	public void MolotovTutDone()
	{
	}

	public void ShowStoryClueTut()
	{
		if (!Scene.HudGui.Tut_StoryClue.activeSelf)
		{
			Scene.HudGui.Tut_StoryClue.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void HideStoryClueTut()
	{
		if (Scene.HudGui.Tut_StoryClue.activeSelf)
		{
			Scene.HudGui.Tut_StoryClue.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ShowSprint()
	{
	}

	public void CloseSprint()
	{
	}

	public void ShowStep1Tut()
	{
		if (!this.bookStep1Done)
		{
			if (!this.Showing && LocalPlayer.IsInWorld)
			{
				this.Showing = true;
				if (this.Book.InitShowStep1())
				{
					this.bookStep1Done = true;
					Scene.HudGui.Tut_BookStage1.SetActive(true);
					Scene.HudGui.Grid.Reposition();
				}
			}
			else
			{
				base.Invoke("ShowStep1Tut", 10f);
			}
		}
	}

	public void ShowStep2Tut()
	{
		if (!this.bookStep2Done)
		{
			if (!this.Showing && LocalPlayer.IsInWorld)
			{
				this.Showing = true;
				if (this.Book.InitShowStep2())
				{
					this.bookStep2Done = true;
					Scene.HudGui.Tut_BookStage2.SetActive(true);
					Scene.HudGui.Grid.Reposition();
				}
			}
			else
			{
				base.Invoke("ShowStep2Tut", 10f);
			}
		}
	}

	public void ShowStep3Tut()
	{
		if (!this.bookStep3Done)
		{
			if (!this.Showing && LocalPlayer.IsInWorld)
			{
				this.Showing = true;
				if (this.Book.InitShowStep3())
				{
					this.bookStep3Done = true;
					Scene.HudGui.Tut_BookStage3.SetActive(true);
					Scene.HudGui.Grid.Reposition();
				}
			}
			else
			{
				base.Invoke("ShowStep3Tut", 10f);
			}
		}
	}

	public void ShelterTutOff()
	{
		Scene.HudGui.Tut_Shelter.SetActive(false);
	}

	public void ShowDeathMP()
	{
		Scene.HudGui.Tut_DeathMP.SetActive(true);
		Scene.HudGui.Grid.Reposition();
	}

	public void HideDeathMP()
	{
		Scene.HudGui.Tut_DeathMP.SetActive(false);
		Scene.HudGui.Grid.Reposition();
	}

	public void ShowReviveMP()
	{
		if (!Scene.HudGui.Tut_ReviveMP.activeSelf)
		{
			Scene.HudGui.Tut_ReviveMP.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void HideReviveMP()
	{
		if (Scene.HudGui && Scene.HudGui.Tut_ReviveMP.activeSelf)
		{
			Scene.HudGui.Tut_ReviveMP.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void TryShowLighter()
	{
		if ((this.NextLightTutTime < Time.realtimeSinceStartup || LocalPlayer.AnimControl.upsideDown) && !LocalPlayer.WaterViz.InWater && !LocalPlayer.IsInEndgame)
		{
			this.ShowLighter();
		}
	}

	public void ShowLighter()
	{
		this.NextLightTutTime = Time.realtimeSinceStartup + this._lighterTutDelay;
		if (!Scene.HudGui.Tut_Lighter.activeSelf)
		{
			Scene.HudGui.Tut_Lighter.SetActive(true);
		}
	}

	public void HideLighter()
	{
		if (Scene.HudGui.Tut_Lighter.activeSelf)
		{
			Scene.HudGui.Tut_Lighter.SetActive(false);
		}
	}

	public void LowHealthTutorial()
	{
		if (!Scene.HudGui.Tut_Health.activeSelf)
		{
			Scene.HudGui.Tut_Health.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void CloseLowHealthTutorial()
	{
		if (Scene.HudGui.Tut_Health.activeSelf)
		{
			Scene.HudGui.Tut_Health.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void HungryTutorial()
	{
		if (!Scene.HudGui.Tut_Hungry.activeSelf)
		{
			Scene.HudGui.Tut_Hungry.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void CloseHungryTutorial()
	{
		if (Scene.HudGui.Tut_Hungry.activeSelf)
		{
			Scene.HudGui.Tut_Hungry.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ShowThirstTut()
	{
		if (!Scene.HudGui.Tut_ThirstDamage.activeSelf)
		{
			Scene.HudGui.Tut_ThirstDamage.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ThirstTutOff()
	{
		if (Scene.HudGui.Tut_ThirstDamage.activeSelf)
		{
			Scene.HudGui.Tut_ThirstDamage.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ShowThirstyTut()
	{
		if (!Scene.HudGui.Tut_Thirsty.activeSelf)
		{
			Scene.HudGui.Tut_Thirsty.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void HideThirstyTut()
	{
		if (Scene.HudGui.Tut_Thirsty.activeSelf)
		{
			Scene.HudGui.Tut_Thirsty.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ShowStarvationTut()
	{
		if (!Scene.HudGui.Tut_Starvation.activeSelf)
		{
			Scene.HudGui.Tut_Starvation.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void StarvationTutOff()
	{
		if (Scene.HudGui.Tut_Starvation.activeSelf)
		{
			Scene.HudGui.Tut_Starvation.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ShowColdDamageTut()
	{
		if (!this.Showing)
		{
			this.Showing = true;
			Scene.HudGui.Tut_ColdDamage.SetActive(true);
			base.Invoke("ColdDamageTutOff", 7f);
		}
		else
		{
			base.Invoke("ShowColdDamageTut", 5f);
		}
	}

	public void ColdDamageTutOff()
	{
		if (this.Showing)
		{
			this.Showing = false;
			Scene.HudGui.Tut_ColdDamage.SetActive(false);
		}
	}

	public void ShowNoInventoryUnderWater()
	{
		if (!Scene.HudGui.Tut_NoInventoryUnderwater.activeSelf)
		{
			Scene.HudGui.Tut_NoInventoryUnderwater.SetActive(true);
			Scene.HudGui.Grid.Reposition();
			base.Invoke("HideNoInventoryUnderWater", 7f);
		}
	}

	public void HideNoInventoryUnderWater()
	{
		Scene.HudGui.Tut_NoInventoryUnderwater.SetActive(false);
		Scene.HudGui.Grid.Reposition();
	}

	public void LowEnergyTutorial()
	{
		if (!Scene.HudGui.Tut_Energy.activeSelf)
		{
			Scene.HudGui.Tut_Energy.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void CloseLowEnergyTutorial()
	{
		if (Scene.HudGui.Tut_Energy.activeSelf)
		{
			Scene.HudGui.Tut_Energy.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ColdTut()
	{
		if (!Scene.HudGui.Tut_Cold.activeSelf)
		{
			Scene.HudGui.Tut_Cold.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void CloseColdTut()
	{
		if (Scene.HudGui.Tut_Cold.activeSelf)
		{
			Scene.HudGui.Tut_Cold.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void BuildToolsTut()
	{
	}

	private void CraftingTut()
	{
		if (!this.craftingTutDone && !this.Showing && !Scene.HudGui.Tut_Crafting.activeSelf)
		{
			this.Showing = true;
			Scene.HudGui.Tut_Crafting.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void CloseCraftingTut()
	{
		if (Scene.HudGui.Tut_Crafting.activeSelf)
		{
			this.Showing = false;
			this.craftingTutDone = true;
			Scene.HudGui.Tut_Crafting.SetActive(false);
			Scene.HudGui.Grid.Reposition();
			this._craftingPageLink.TurnOffAllPages();
			this._craftingPageLink.OnClick();
		}
	}

	public void RecordedBody()
	{
		base.CancelInvoke("TurnOffBodiesMessage");
		Scene.HudGui.PlaneBodiesMsg.SetActive(false);
		Scene.HudGui.Grid.Reposition();
		this.PlaneBodies++;
		Scene.HudGui.PlaneBodiesMsg.SetActive(true);
		Scene.HudGui.PlaneBodiesLabel.text = this.PlaneBodies + "/" + this.TotalPlaneBodies.ToString();
		Scene.HudGui.Grid.Reposition();
		base.Invoke("TurnOffBodiesMessage", 3f);
	}

	private void TurnOffBodiesMessage()
	{
		if (Scene.HudGui.PlaneBodiesMsg.activeSelf)
		{
			Scene.HudGui.PlaneBodiesMsg.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	private void ShowAxeTut()
	{
		if (!this.Showing)
		{
			this.Showing = true;
			this.axeTutDone = true;
			Scene.HudGui.Tut_Axe.SetActive(true);
			Scene.HudGui.Grid.Reposition();
			base.Invoke("CloseAxeTut", 15f);
		}
	}

	private void CloseAxeTut()
	{
		if (Scene.HudGui.Tut_Axe.activeSelf)
		{
			this.Showing = false;
			Scene.HudGui.Tut_Axe.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ShowNewBuildingsAvailableTut()
	{
		if (!Scene.HudGui.Tut_NewBuildingsAvailable.activeSelf)
		{
			Scene.HudGui.Tut_NewBuildingsAvailable.SetActive(true);
			Scene.HudGui.Grid.Reposition();
			base.Invoke("CloseNewBuildingsAvailableTut", 15f);
		}
	}

	private void CloseNewBuildingsAvailableTut()
	{
		if (Scene.HudGui.Tut_NewBuildingsAvailable.activeSelf)
		{
			Scene.HudGui.Tut_NewBuildingsAvailable.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void ShowSleddingTut()
	{
		if (!this.Showing && !this.sleddingTutDone)
		{
			this.Showing = true;
			this.sleddingTutDone = true;
			Scene.HudGui.Tut_Sledding.SetActive(true);
			base.Invoke("CloseSleddingTut", 15f);
		}
	}

	public void CloseSleddingTut()
	{
		if (Scene.HudGui.Tut_Sledding.activeSelf)
		{
			this.Showing = false;
			Scene.HudGui.Tut_Sledding.SetActive(false);
		}
	}

	private void ShowWorldMapTut(int mapId)
	{
		this.worldMapTutDone = true;
		base.StartCoroutine(this.ShowWorldMapTutDelayed(mapId));
	}

	private IEnumerator ShowWorldMapTutDelayed(int mapId)
	{
		yield return YieldPresets.WaitOneSecond;
		if (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, mapId))
		{
			LocalPlayer.Inventory.Equip(mapId, false);
			base.StartCoroutine(this.CloseWorldMapTut(mapId));
		}
		yield break;
	}

	private IEnumerator CloseWorldMapTut(int mapId)
	{
		yield return YieldPresets.WaitTenSeconds;
		if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, mapId))
		{
			LocalPlayer.Inventory.StashEquipedWeapon(true);
		}
		yield break;
	}

	public void CloseBuildToolsTut()
	{
	}

	public void CaveTut()
	{
	}

	private void ResetCaveTutDelay()
	{
	}

	private void CloseCaveTut()
	{
	}

	public void ShowMushroomPage()
	{
	}

	private void CloseMushroomPage()
	{
	}

	private void ResetMushroomDelay()
	{
	}

	public void BloodyTut()
	{
		if (!Scene.HudGui.Tut_Bloody.activeSelf)
		{
			Scene.HudGui.Tut_Bloody.SetActive(true);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void CloseBloodyTut()
	{
		if (Scene.HudGui.Tut_Bloody.activeSelf)
		{
			Scene.HudGui.Tut_Bloody.SetActive(false);
			Scene.HudGui.Grid.Reposition();
		}
	}

	public void CloseAllBookTuts()
	{
		Scene.HudGui.Tut_BookStage1.SetActive(false);
		Scene.HudGui.Tut_BookStage2.SetActive(false);
		Scene.HudGui.Tut_BookStage3.SetActive(false);
		Scene.HudGui.Tut_OpenBook.SetActive(false);
		Scene.HudGui.Tut_Axe.SetActive(false);
		Scene.HudGui.Tut_Crafting.SetActive(false);
		Scene.HudGui.Grid.Reposition();
		this.Showing = false;
	}

	public void CloseAll()
	{
		this.Showing = true;
		this.CloseAllBookTuts();
		this.Showing = true;
		this.HideMolotovTut();
		this.Showing = true;
		this.CloseColdTut();
		this.Showing = true;
		this.ThirstTutOff();
		this.Showing = true;
		this.HideThirstyTut();
		this.Showing = true;
		this.StarvationTutOff();
		this.Showing = true;
		this.ColdDamageTutOff();
		this.CloseNewBuildingsAvailableTut();
	}

	public SurvivalBook Book;

	[ItemIdPicker(Item.Types.Equipment)]
	public int[] _axesItemId;

	[ItemIdPicker(Item.Types.CraftingMaterial)]
	public int[] _craftTutReceiepeItemId;

	[ItemIdPicker(Item.Types.Equipment)]
	public int[] _worldMapPiecesItemIds;

	[ItemIdPicker(Item.Types.Equipment)]
	public int _turtleShellItemId;

	public float _lighterTutDelay = 60f;

	public SelectPageNumber _craftingPageLink;

	public Renderer _bookInventoryView;

	private float NextLightTutTime;

	private int TotalPlaneBodies = 130;

	private bool Showing;

	[SerializeThis]
	private int PlaneBodies;

	[SerializeThis]
	private bool bookStep1Done;

	[SerializeThis]
	private bool bookStep2Done;

	[SerializeThis]
	private bool bookStep3Done;

	[SerializeThis]
	private bool ShouldShowLighter;

	[SerializeThis]
	private bool bookTutDone;

	[SerializeThis]
	private bool craftingTutDone;

	[SerializeThis]
	private bool receipeTutDone;

	[SerializeThis]
	private bool axeTutDone;

	[SerializeThis]
	private bool treeStructureTutDone;

	[SerializeThis]
	private bool worldMapTutDone;

	[SerializeThis]
	private bool molotovTutDone;

	[SerializeThis]
	private bool sleddingTutDone;
}
