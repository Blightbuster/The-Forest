using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items.World;
using TheForest.Networking;
using TheForest.Save;
using TheForest.UI.Multiplayer;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

public class BoltPlayerSetup : EntityEventListener<IPlayerState>, IPriorityCalculator
{
	private void Awake()
	{
		this.variations = base.GetComponent<CoopPlayerVariations>();
	}

	private void Start()
	{
		this.removeClothing = base.GetComponentInChildren<coopPlayerRemoveUnusedClothing>();
	}

	private void FindHands()
	{
		foreach (itemConstrainToHand itemConstrainToHand in base.transform.GetAllComponentsInChildren<itemConstrainToHand>())
		{
			if (itemConstrainToHand.gameObject.name == "rightHandHeld")
			{
				this.rh = itemConstrainToHand;
				if (this.rh && this.lh && this.feet)
				{
					break;
				}
			}
			if (itemConstrainToHand.gameObject.name == "leftHandHeld")
			{
				this.lh = itemConstrainToHand;
				if (this.rh && this.lh && this.feet)
				{
					break;
				}
			}
			if (itemConstrainToHand.gameObject.name == "char_Hips")
			{
				this.feet = itemConstrainToHand;
				if (this.rh && this.lh && this.feet)
				{
					break;
				}
			}
		}
	}

	public void WaitForInventoryToBeEnabledAndThenDo(Action action)
	{
		base.StartCoroutine(this.WaitForInv(action));
	}

	private IEnumerator WaitForInv(Action action)
	{
		yield return new WaitForSeconds(1f);
		while (!LocalPlayer.Inventory.enabled)
		{
			yield return null;
		}
		action();
		yield break;
	}

	private void PlayerVariationSetup()
	{
		if (BoltNetwork.isServer)
		{
			CoopServerInfo.Instance.entity.Freeze(false);
			CoopServerInfo.Instance.state.UsedPlayerVariations |= 1 << base.state.PlayerVariation * 4;
			if (CoopServerInfo.Instance.state.UsedPlayerVariations == (1 << this.variations.BodyVariationCount) - 1)
			{
				CoopServerInfo.Instance.state.UsedPlayerVariations = 0;
			}
			if (CoopServerInfo.Instance.state.UsedPlayerClothingVariations == (1 << this.variations.ClothingVariationCount) - 1)
			{
				CoopServerInfo.Instance.state.UsedPlayerClothingVariations = 0;
			}
		}
		this.ClothingIds.Clear();
		for (int i = 0; i < 4; i++)
		{
			if (base.state.PlayerClothingIds[i] > 0)
			{
				this.ClothingIds.Add(base.state.PlayerClothingIds[i]);
			}
		}
		if (this.ClothingIds.Count == 0)
		{
			this.ClothingIds.Add(-1);
		}
		this.variations.SetVariation(base.state.PlayerVariation, base.state.PlayerVariationHair, this.ClothingIds);
		this.RefreshPlayerMaterials();
	}

	private void RefreshPlayerMaterials()
	{
		this.variations.UpdateSkinVariation(base.state.Bloody, base.state.Muddy, base.state.RedPaint, base.state.Cold);
	}

	private void InitClothingMpSync()
	{
		LocalPlayer.Clothing.RefreshVisibleClothing();
		LocalPlayer.Clothing.MpSync();
		this.PlayerVariationSetup();
	}

	public override void Attached()
	{
		base.state.Transform.SetTransforms(base.transform);
		if (base.entity.isOwner)
		{
			base.state.PlayerVariation = LocalPlayer.Stats.PlayerVariation;
			base.state.PlayerVariationHair = LocalPlayer.Stats.PlayerVariationHair;
			base.Invoke("InitClothingMpSync", 0.1f);
			base.Invoke("RefreshPlayerMaterials", 0.1f);
			this.FindHands();
			if (base.entity.isOwner && BoltNetwork.isClient && Scene.TriggerCutScene.clientPlayerOnPlaneGo != null)
			{
				Scene.TriggerCutScene.clientPlayerOnPlaneGo.SendMessage("PlayerVariationSetupClean", base.state.PlayerVariation, SendMessageOptions.DontRequireReceiver);
				Scene.TriggerCutScene.clientCutScenePlayerGo.SendMessage("PlayerVariationSetup", base.state.PlayerVariation, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			base.state.AddCallback("PlayerVariation", new PropertyCallbackSimple(this.PlayerVariationSetup));
			base.state.AddCallback("PlayerVariationHair", new PropertyCallbackSimple(this.PlayerVariationSetup));
			base.state.AddCallback("PlayerClothingIds[]", new PropertyCallbackSimple(this.PlayerVariationSetup));
			base.state.AddCallback("Bloody", new PropertyCallbackSimple(this.RefreshPlayerMaterials));
			base.state.AddCallback("Muddy", new PropertyCallbackSimple(this.RefreshPlayerMaterials));
			base.state.AddCallback("RedPaint", new PropertyCallbackSimple(this.RefreshPlayerMaterials));
			base.state.AddCallback("Cold", new PropertyCallbackSimple(this.RefreshPlayerMaterials));
			this.PlayerVariationSetup();
			this.RefreshPlayerMaterials();
			if (BoltNetwork.isServer && CoopTreeGrid.TodoPlayerSweeps.Contains(base.entity.source))
			{
				CoopTreeGrid.TodoPlayerSweeps.Remove(base.entity.source);
				CoopTreeGrid.SweepGrid();
			}
			Transform playerTr = base.transform;
			PlayerName pn = base.GetComponentInChildren<PlayerName>();
			targetStats ts = base.GetComponent<targetStats>();
			base.state.AddCallback("name", delegate
			{
				pn.Init(this.state.name);
			});
			this.plasticTorch = base.GetComponentsInChildren<BatteryBasedLight>(true).FirstOrDefault<BatteryBasedLight>();
			if (BoltNetwork.isClient && Scene.SceneTracker)
			{
				if (!Scene.SceneTracker.allPlayers.Contains(base.gameObject))
				{
					Scene.SceneTracker.allPlayers.Add(base.entity.gameObject);
				}
				if (!Scene.SceneTracker.allPlayerEntities.Contains(base.entity))
				{
					Scene.SceneTracker.allPlayerEntities.Add(base.entity);
				}
				if (!Scene.SceneTracker.allClients.Contains(base.gameObject))
				{
					Scene.SceneTracker.allClients.Add(base.entity.gameObject);
				}
			}
			base.state.AddCallback("CurrentView", delegate
			{
				if (this.state.CurrentView == 7 != this.RespawnDeadTrigger.activeSelf)
				{
					if (!ts || !ts.inWater)
					{
						this.RespawnDeadTrigger.SetActive(!this.RespawnDeadTrigger.activeSelf);
					}
				}
				pn.OnCurrentViewChanged();
				if (this.state.CurrentView == 8)
				{
					if (Scene.SceneTracker.allPlayers.Contains(this.gameObject))
					{
						Scene.SceneTracker.allPlayers.Remove(this.gameObject);
					}
					if (Scene.SceneTracker.allPlayerEntities.Contains(this.entity))
					{
						Scene.SceneTracker.allPlayerEntities.Remove(this.entity);
					}
					for (int i = playerTr.childCount - 1; i >= 0; i--)
					{
						Transform child = playerTr.GetChild(i);
						if (!child.GetComponent<Animator>())
						{
							UnityEngine.Object.Destroy(child.gameObject);
						}
						else
						{
							for (int j = child.childCount - 1; j >= 0; j--)
							{
								UnityEngine.Object.Destroy(child.GetChild(j).gameObject);
							}
							Component[] components = child.GetComponents(typeof(MonoBehaviour));
							foreach (Component component in components)
							{
								if (!(component is Animator))
								{
									UnityEngine.Object.DestroyImmediate(component);
								}
							}
						}
					}
					Component[] components2 = this.GetComponents(typeof(MonoBehaviour));
					foreach (Component component2 in components2)
					{
						if (!(component2 is BoltEntity))
						{
							UnityEngine.Object.DestroyImmediate(component2);
						}
					}
					StealItemTrigger stealItemTrigger = UnityEngine.Object.Instantiate<StealItemTrigger>(Prefabs.Instance.DeadBackpackPrefab);
					stealItemTrigger._entity = this.entity;
					stealItemTrigger.transform.parent = playerTr;
					stealItemTrigger.transform.localPosition = Vector3.zero;
				}
			});
			base.state.AddCallback("BatteryTorchEnabled", delegate
			{
				this.plasticTorch.SetEnabled(this.state.BatteryTorchEnabled);
			});
			base.state.AddCallback("BatteryTorchColor", delegate
			{
				this.plasticTorch.SetColor(this.state.BatteryTorchColor);
			});
			base.state.AddCallback("BatteryTorchIntensity", delegate
			{
				this.plasticTorch.SetIntensity(this.state.BatteryTorchIntensity);
			});
		}
	}

	private void OnDestroy()
	{
		if (!Scene.SceneTracker)
		{
			return;
		}
		if (Scene.SceneTracker.allPlayers.Contains(base.gameObject))
		{
			Scene.SceneTracker.allPlayers.Remove(base.gameObject);
		}
		if (Scene.SceneTracker.allPlayerEntities.Contains(base.entity))
		{
			Scene.SceneTracker.allPlayerEntities.Remove(base.entity);
		}
	}

	public override void OnEvent(HitPlayer evnt)
	{
		if (base.entity.isOwner)
		{
			int num = 5;
			if (evnt.damage > 0)
			{
				num = evnt.damage;
			}
			base.entity.SendMessage("hitFromEnemy", num);
		}
	}

	private void UpdateLH()
	{
		if (this.lh)
		{
			for (int i = 0; i < this.lh.Available.Length; i++)
			{
				if (this.lh.Available[i].IsNull())
				{
					Debug.LogError("Missing GameObject on Left Hand index : " + i);
				}
				else if (this.lh.Available[i].activeSelf)
				{
					base.state.itemInLeftHand = i;
					return;
				}
			}
			base.state.itemInLeftHand = -1;
		}
	}

	private void UpdateRH()
	{
		if (this.rh)
		{
			for (int i = 0; i < this.rh.Available.Length; i++)
			{
				if (this.rh.Available[i].IsNull())
				{
					Debug.LogError("Missing GameObject on Right Hand index : " + i);
				}
				else if (this.rh.Available[i].activeSelf)
				{
					if (base.state.itemInRightHand != i)
					{
						base.state.itemInRightHand = i;
					}
					return;
				}
			}
			base.state.itemInRightHand = -1;
		}
	}

	private void UpdateFeet()
	{
		if (this.feet)
		{
			for (int i = 0; i < this.feet.Available.Length; i++)
			{
				if (this.feet.Available[i].IsNull())
				{
					Debug.LogError("Missing GameObject on Feet index : " + i);
				}
				else if (this.feet.Available[i].activeSelf)
				{
					if (base.state.itemAtFeet != i)
					{
						base.state.itemAtFeet = i;
					}
					return;
				}
			}
			base.state.itemAtFeet = -1;
		}
	}

	private void UpdateState()
	{
		base.state.Bloody = LocalPlayer.Stats.IsBloody;
		base.state.Muddy = LocalPlayer.Stats.IsMuddy;
		base.state.RedPaint = LocalPlayer.Stats.IsRed;
		base.state.Cold = LocalPlayer.Stats.IsCold;
	}

	private void Update()
	{
		if (base.entity.IsOwner())
		{
			this.UpdateLH();
			this.UpdateRH();
			this.UpdateFeet();
			this.UpdateState();
		}
		else
		{
			base.enabled = false;
		}
	}

	bool IPriorityCalculator.Always
	{
		get
		{
			return true;
		}
	}

	float IPriorityCalculator.CalculateEventPriority(BoltConnection connection, Bolt.Event evnt)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, 1f, 1);
	}

	float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, int skipped)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, 1f, skipped);
	}

	internal void SnapToSpawn()
	{
		base.StartCoroutine(this.SnapToSpawnRoutine());
	}

	private IEnumerator SnapToSpawnRoutine()
	{
		while (!PlayerRespawnMP.Instance)
		{
			yield return null;
		}
		base.transform.position = PlayerRespawnMP.Instance.transform.position;
		yield break;
	}

	private itemConstrainToHand lh;

	private itemConstrainToHand rh;

	private itemConstrainToHand feet;

	private BatteryBasedLight plasticTorch;

	[SerializeField]
	private GameObject RespawnDeadTrigger;

	[SerializeField]
	private GameObject StealItemTrigger;

	private Transform sledLookat;

	private CoopPlayerVariations variations;

	private List<int> ClothingIds = new List<int>(4);

	public coopPlayerRemoveUnusedClothing removeClothing;
}
