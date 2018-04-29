using System;
using Bolt;
using ModAPI;
using PathologicalGames;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UltimateCheatmenu;
using UniLinq;
using UnityEngine;


[DoNotSerializePublic]
public class Garden : MonoBehaviour
{
	
	private void Awake()
	{
		base.enabled = false;
	}

	
	private void Update()
	{
		bool flag = LocalPlayer.Inventory.Owns(this.CurrentSeedItemId, true);
		bool flag2 = this.BusySlots < this.GrowSpots.Length && flag;
		if (flag2)
		{
			if (TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.RemoveItem(this.CurrentSeedItemId, 1, true, true))
			{
				this.PlantSeed();
			}
			int num = this.NextSeedType();
			if (num != this._currentSeedType)
			{
				if (TheForest.Utils.Input.GetButtonDown("Rotate"))
				{
					LocalPlayer.Sfx.PlayWhoosh();
					this._currentSeedType = num;
				}
				this.Widget.ShowList(this.CurrentSeedItemId, this.RotateIcon.transform, SideIcons.Craft);
			}
			else
			{
				this.Widget.ShowSingle(this.CurrentSeedItemId, this.RotateIcon.transform, SideIcons.Craft);
			}
		}
		else if (!flag)
		{
			this._currentSeedType = this.NextSeedType();
			this.Widget.Shutdown();
		}
	}

	
	private void OnDisable()
	{
		if (Scene.HudGui)
		{
			this.Widget.Shutdown();
		}
	}

	
	private void GrabEnter(GameObject grabber)
	{
		base.enabled = true;
		this._currentSeedType--;
		this._currentSeedType = this.NextSeedType();
	}

	
	private void GrabExit()
	{
		base.enabled = false;
		this.Widget.Shutdown();
	}

	
	private void OnBeginCollapse()
	{
		if (BoltNetwork.isServer && Scene.HudGui)
		{
			foreach (Transform transform in this.GrowSpots)
			{
				if (transform && transform.childCount > 0)
				{
					BoltNetwork.Destroy(transform.GetChild(0).gameObject);
				}
			}
		}
	}

	
	private int NextSeedType()
	{
		int num = this._seeds.Length;
		int num2 = this._currentSeedType;
		while (num-- > 0)
		{
			num2 = (int)Mathf.Repeat((float)(num2 + 1), (float)this._seeds.Length);
			if (LocalPlayer.Inventory.Owns(this._seeds[num2]._itemId, true))
			{
				break;
			}
		}
		return num2;
	}

	
	public void __PlantSeed__Original()
	{
		EventRegistry.Achievements.Publish(TfEvent.Achievements.PlantedSeed, this.CurrentSeedItemId);
		if (BoltNetwork.isRunning)
		{
			if (BoltNetwork.isClient)
			{
				LocalPlayer.Sfx.PlayDigDirtPile(base.gameObject);
			}
			GrowGarden growGarden = GrowGarden.Create(GlobalTargets.OnlyServer);
			growGarden.Garden = base.GetComponentInParent<BoltEntity>();
			growGarden.SeedNum = this._currentSeedType;
			growGarden.Send();
		}
		else
		{
			this.PlantSeed_Real(this._currentSeedType);
		}
	}

	
	public void PlantSeed_Real(int seedNum)
	{
		int num = 0;
		while (num < this.GrowSpots.Length && this.GrowSpots[num].childCount > 0)
		{
			num++;
		}
		if (num < this.GrowSpots.Length)
		{
			this.SpawnDirtPile(seedNum, num);
		}
	}

	
	public void SpawnDirtPile(int seedNum, int growSpotNum)
	{
		Transform transform = this.GrowSpots[growSpotNum].transform;
		Transform transform2 = PoolManager.Pools["misc"].Spawn(this._seeds[seedNum]._dirtPilePrefab.transform, transform.position, transform.rotation);
		GardenDirtPile component = transform2.GetComponent<GardenDirtPile>();
		component.SlotNum = growSpotNum;
		if (!BoltNetwork.isRunning)
		{
			float num = UnityEngine.Random.Range(0.7f, 1f);
			transform2.localScale = new Vector3(num, num, num);
		}
		transform2.parent = transform;
		LocalPlayer.Sfx.PlayDigDirtPile(component.gameObject);
		if (BoltNetwork.isRunning)
		{
			BoltEntity component2 = component.GetComponent<BoltEntity>();
			if (component2 && !component2.isAttached)
			{
				BoltNetwork.Attach(component2);
			}
		}
	}

	
	
	
	public bool Test { get; set; }

	
	
	public int BusySlots
	{
		get
		{
			return this.GrowSpots.Sum((Transform gs) => gs.childCount);
		}
	}

	
	
	private int CurrentSeedItemId
	{
		get
		{
			return this._seeds[this._currentSeedType]._itemId;
		}
	}

	
	
	private ItemListWidget Widget
	{
		get
		{
			return Scene.HudGui.GardenWidgets[(int)this._dirtPileType];
		}
	}

	
	public void PlantSeed()
	{
		try
		{
			if (UCheatmenu.seedtype >= 0)
			{
				this._currentSeedType = UCheatmenu.seedtype;
			}
			if (BoltNetwork.isRunning)
			{
				if (BoltNetwork.isClient)
				{
					LocalPlayer.Sfx.PlayDigDirtPile(base.gameObject);
				}
				GrowGarden growGarden = GrowGarden.Create(GlobalTargets.OnlyServer);
				growGarden.Garden = base.GetComponentInParent<BoltEntity>();
				growGarden.SeedNum = this._currentSeedType;
				growGarden.Send();
				return;
			}
			this.PlantSeed_Real(this._currentSeedType);
		}
		catch (Exception ex)
		{
			Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
			this.__PlantSeed__Original();
		}
	}

	
	public Transform[] GrowSpots;

	
	public GameObject RotateIcon;

	
	public Garden.SeedTypes[] _seeds;

	
	public Garden.DirtPileTypes _dirtPileType;

	
	private int _currentSeedType;

	
	[Serializable]
	public class SeedTypes
	{
		
		[ItemIdPicker]
		public int _itemId;

		
		public GardenDirtPile _dirtPilePrefab;
	}

	
	public enum DirtPileTypes
	{
		
		All,
		
		SmallOnly,
		
		Underground
	}
}
