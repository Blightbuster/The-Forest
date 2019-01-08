using System;
using System.Collections;
using Bolt;
using PathologicalGames;
using TheForest.Items.World;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[DoNotSerializePublic]
	public class GardenDirtPile : EntityBehaviour<IGardenDirtPileState>, IEntityReplicationFilter
	{
		private void Awake()
		{
			this._usedPickups = new bool[this._pickups.Length];
			if (BoltNetwork.isClient)
			{
				this.Growth();
			}
		}

		private void OnDeserialized()
		{
			if (!BoltNetwork.isClient)
			{
				this.Init();
			}
		}

		private void OnSpawned()
		{
			if (!LevelSerializer.IsDeserializing)
			{
				this._plantedTime = Scene.Clock.ElapsedGameTime;
			}
			this.Init();
		}

		private void Init()
		{
			if (!this._plantRenderer)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				this._plantRenderer.transform.localScale = Vector3.zero;
				if (this._growType == GardenDirtPile.GrowTypes.Plant)
				{
					base.InvokeRepeating("Growth", 0.1f, 30f);
				}
				else
				{
					base.InvokeRepeating("Growth", 0.1f, 60f);
				}
			}
		}

		public void Growth()
		{
			if (this._growType == GardenDirtPile.GrowTypes.Plant)
			{
				this.PlantGrowth();
			}
			else if (this._growType == GardenDirtPile.GrowTypes.Spore)
			{
				this.SporeGrowth();
			}
		}

		private void PlantGrowth()
		{
			float num = Scene.Clock.ElapsedGameTime - this._plantedTime;
			float num2 = Mathf.Clamp(0.3f * Mathf.Min(num / this._growthDuration, 3f), 0f, 0.9f);
			this._plantRenderer.transform.localScale = new Vector3(num2, num2, num2);
			int num3 = Mathf.FloorToInt(num / this._growthDuration);
			if (num3 < 5)
			{
				num3 = Mathf.Min(num3, 3);
				for (int i = 0; i < this._pickups.Length; i++)
				{
					PickUp pickUp = this._pickups[i];
					if (num3 > 0)
					{
						if (this._usedPickups[i] || pickUp.Used)
						{
							pickUp.Used = true;
							this._usedPickups[i] = true;
							if (BoltNetwork.isRunning && base.entity.isAttached && base.entity.isOwner && base.state.UsedPickups[i] == 0)
							{
								base.state.UsedPickups[i] = 1;
							}
							if (pickUp._destroyTarget.gameObject.activeSelf)
							{
								pickUp._destroyTarget.gameObject.SetActive(false);
							}
						}
						else
						{
							pickUp._amount._min = num3;
							pickUp._amount._max = num3;
							if (!pickUp.gameObject.activeSelf)
							{
								pickUp.gameObject.SetActive(true);
							}
						}
					}
					else if (pickUp.gameObject.activeSelf)
					{
						pickUp.gameObject.SetActive(false);
					}
				}
			}
			else if (!BoltNetwork.isClient)
			{
				this.Clear();
			}
		}

		private void SporeGrowth()
		{
			float num = Scene.Clock.ElapsedGameTime - this._plantedTime;
			float num2 = Mathf.Lerp(0f, 0.7f, 0.3f * Mathf.Min(num / this._growthDuration, 3f));
			this._plantRenderer.transform.localScale = new Vector3(num2, num2, num2);
			if (!BoltNetwork.isClient && num2 > 0.1f)
			{
				if (!this._pickups[0].gameObject.activeSelf)
				{
					this._pickups[0].gameObject.SetActive(true);
				}
				if (num < 5f)
				{
					if (UnityEngine.Random.Range(0, Mathf.RoundToInt(10f - num2 * 10f)) == 0)
					{
						Garden componentInChildren = base.transform.parent.parent.GetComponentInChildren<Garden>();
						if (this.SlotNum > 0 && componentInChildren.GrowSpots[this.SlotNum - 1].childCount == 0)
						{
							int itemId = this._pickups[0]._itemId;
							int seedNum = componentInChildren._seeds.FindIndex((Garden.SeedTypes s) => s._itemId == itemId);
							componentInChildren.SpawnDirtPile(seedNum, this.SlotNum - 1);
							return;
						}
						if (this.SlotNum < componentInChildren.GrowSpots.Length - 1 && componentInChildren.GrowSpots[this.SlotNum + 1].childCount == 0)
						{
							int itemId = this._pickups[0]._itemId;
							int seedNum2 = componentInChildren._seeds.FindIndex((Garden.SeedTypes s) => s._itemId == itemId);
							componentInChildren.SpawnDirtPile(seedNum2, this.SlotNum + 1);
							return;
						}
					}
				}
				else
				{
					this.Clear();
				}
			}
			else if (this._pickups[0].gameObject.activeSelf != num2 > 0.1f)
			{
				this._pickups[0].gameObject.SetActive(num2 > 0.1f);
			}
		}

		private void Clear()
		{
			if (!BoltNetwork.isClient)
			{
				if (PoolManager.Pools["misc"].IsSpawned(base.transform))
				{
					if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
					{
						BoltNetwork.Detach(base.entity);
					}
					base.transform.parent = null;
					PoolManager.Pools["misc"].Despawn(base.transform);
					for (int i = 0; i < this._pickups.Length; i++)
					{
						this._pickups[i].transform.parent.gameObject.SetActive(true);
					}
				}
				else
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		private void SetPickupUsed(int num)
		{
			for (int i = 0; i < this._pickups.Length; i++)
			{
				PickUp pickUp = this._pickups[i];
				if (pickUp._destroyTarget.transform.GetSiblingIndex() == num)
				{
					if (!pickUp.Used)
					{
						pickUp.Used = true;
						this._usedPickups[i] = true;
						if (BoltNetwork.isRunning && base.entity.isAttached && base.entity.isOwner && base.state.UsedPickups[i] == 0)
						{
							base.entity.Freeze(false);
							base.state.UsedPickups[i] = 1;
						}
						if (pickUp._destroyTarget.gameObject.activeSelf)
						{
							pickUp._destroyTarget.gameObject.SetActive(false);
						}
					}
					break;
				}
			}
		}

		public override void Attached()
		{
			if (BoltNetwork.isClient)
			{
				if (base.state.PlantedTime >= 0f)
				{
					this.InitPlantedTimeFromStateData();
				}
				else
				{
					base.state.AddCallback("PlantedTime", new PropertyCallbackSimple(this.InitPlantedTimeFromStateData));
				}
				if (base.state.SlotNum >= 0)
				{
					this.InitSlotNumFromStateData();
				}
				else
				{
					base.state.AddCallback("SlotNum", new PropertyCallbackSimple(this.InitSlotNumFromStateData));
				}
				base.state.AddCallback("UsedPickups[]", new PropertyCallbackSimple(this.RefreshUsedPickups));
				base.StartCoroutine(this.WaitForGarden());
			}
			else
			{
				base.state.PlantedTime = this._plantedTime;
				base.state.SlotNum = this._slotNum;
				base.StartCoroutine(this.WaitForGardenEntity());
			}
		}

		private void InitPlantedTimeFromStateData()
		{
			this._plantedTime = base.state.PlantedTime;
			if (this._plantedTime >= 0f && this.SlotNum >= 0)
			{
				this.Init();
			}
		}

		private void InitSlotNumFromStateData()
		{
			this.SlotNum = base.state.SlotNum;
			if (this._plantedTime >= 0f && this.SlotNum >= 0)
			{
				this.Init();
			}
		}

		private void RefreshUsedPickups()
		{
			int num = Mathf.Min(this._usedPickups.Length, base.state.UsedPickups.Length);
			for (int i = 0; i < num; i++)
			{
				if (this._usedPickups[i] != (base.state.UsedPickups[i] == 1))
				{
					PickUp pickUp = this._pickups[i];
					this._usedPickups[i] = !this._usedPickups[i];
					pickUp._destroyTarget.SetActive(!this._usedPickups[i]);
					pickUp.Used = this._usedPickups[i];
				}
			}
		}

		private IEnumerator WaitForGarden()
		{
			Garden g = null;
			while (!g)
			{
				yield return null;
				if (base.state.Garden && base.state.Garden.isAttached)
				{
					g = base.state.Garden.GetComponentInChildren<Garden>();
				}
			}
			base.transform.parent = g.GrowSpots[this._slotNum];
			yield break;
		}

		private IEnumerator WaitForGardenEntity()
		{
			BoltEntity entity = base.transform.parent.GetComponentInParent<BoltEntity>();
			while (!base.state.Garden)
			{
				yield return null;
				base.state.Garden = entity;
			}
			if (base.state.PlantedTime < 0f || base.state.SlotNum < 0)
			{
				base.state.PlantedTime = this._plantedTime;
				base.state.SlotNum = this._slotNum;
			}
			yield break;
		}

		bool IEntityReplicationFilter.AllowReplicationTo(BoltConnection connection)
		{
			if (this && base.entity && base.entity.isAttached && base.state != null && base.state.Garden)
			{
				if (base.state.Garden.isFrozen)
				{
					base.state.Garden.Freeze(false);
				}
				return connection.ExistsOnRemote(base.state.Garden) == ExistsResult.Yes;
			}
			return false;
		}

		public int SlotNum
		{
			get
			{
				return this._slotNum;
			}
			set
			{
				this._slotNum = value;
			}
		}

		public float _growthDuration = 1f;

		public GameObject _plantRenderer;

		public PickUp[] _pickups;

		public GardenDirtPile.GrowTypes _growType;

		[SerializeThis]
		private float _plantedTime = -1f;

		[SerializeThis]
		private int _slotNum = -1;

		[SerializeThis]
		private bool[] _usedPickups;

		public enum GrowTypes
		{
			Plant,
			Spore
		}
	}
}
