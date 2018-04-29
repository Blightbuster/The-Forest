using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Tools;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/Creation/Craft Structure")]
	public class Craft_Structure : EntityBehaviour
	{
		
		private void OnSerializing()
		{
			this._presentIngredientsCount = this._presentIngredients.Length;
		}

		
		private void OnDeserialized()
		{
			base.enabled = false;
			if (this._presentIngredients != null && this._presentIngredients.Length > this._presentIngredientsCount)
			{
				ReceipeIngredient[] array = new ReceipeIngredient[this._presentIngredientsCount];
				for (int i = 0; i < this._presentIngredientsCount; i++)
				{
					array[i] = this._presentIngredients[i];
				}
				this._presentIngredients = array;
			}
			if (this.manualLoading)
			{
				this.WasLoaded = true;
			}
			else
			{
				this.Initialize();
			}
		}

		
		public void Initialize()
		{
			if (!this._initialized)
			{
				if (this._presentIngredients == null)
				{
					this._presentIngredients = new ReceipeIngredient[this._requiredIngredients.Count];
				}
				if (this._presentIngredients.Length != this._requiredIngredients.Count)
				{
					this._presentIngredients = new ReceipeIngredient[this._requiredIngredients.Count];
				}
				for (int i = 0; i < this._requiredIngredients.Count; i++)
				{
					Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[i];
					if (this._presentIngredients[i] == null)
					{
						this._presentIngredients[i] = new ReceipeIngredient
						{
							_itemID = buildIngredients._itemID
						};
					}
					ReceipeIngredient receipeIngredient = this._presentIngredients[i];
					int amount = buildIngredients._amount - receipeIngredient._amount;
					BuildMission.AddNeededToBuildMission(buildIngredients._itemID, amount, true);
					for (int j = 0; j < receipeIngredient._amount; j++)
					{
						if (j >= buildIngredients._renderers.Length)
						{
							break;
						}
						buildIngredients._renderers[j].SetActive(true);
					}
				}
				this._initialized = true;
				if (BoltNetwork.isRunning)
				{
					base.gameObject.AddComponent<CoopConstruction>();
					if (BoltNetwork.isServer && this.entity.isAttached)
					{
						this.UpdateNetworkIngredients();
					}
				}
				if (!BoltNetwork.isClient)
				{
					this.CheckNeeded();
				}
			}
		}

		
		private void Start()
		{
			if (this._presentIngredients == null)
			{
				this._presentIngredients = new ReceipeIngredient[this._requiredIngredients.Count];
			}
			base.enabled = (this._initialized || this._grabbed);
			if (!this.manualLoading && base.transform.root != LocalPlayer.Transform)
			{
				if (!LevelSerializer.IsDeserializing)
				{
					this.Initialize();
				}
				else if (this._requiredIngredients == null)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			this._ghost = base.transform.parent.gameObject;
		}

		
		private void Update()
		{
			if (this._initialized)
			{
				this.CheckText();
				this.CheckNeeded();
				Scene.HudGui.DestroyIcon.gameObject.SetActive(true);
				if (this._swapTo)
				{
					if (!Scene.HudGui.ToggleVariationIcon.activeSelf)
					{
						Scene.HudGui.ToggleVariationIcon.SetActive(true);
					}
					if (TheForest.Utils.Input.GetButtonDown("Rotate"))
					{
						this.SwapToNextGhost();
						return;
					}
				}
				if (TheForest.Utils.Input.GetButtonAfterDelay("Craft", 0.5f, false))
				{
					this.CancelBlueprint();
					return;
				}
				if (this._lockBuild || (this.CustomLockCheck != null && this.CustomLockCheck()))
				{
					int num = 0;
					for (int i = 0; i < this._requiredIngredients.Count; i++)
					{
						num += this._requiredIngredients[i]._amount - this._presentIngredients[i]._amount;
					}
					if (num == 1)
					{
						this.AllOff(false);
						Scene.HudGui.CantPlaceIcon.SetActive(true);
						return;
					}
				}
				Scene.HudGui.CantPlaceIcon.SetActive(false);
				bool flag = false;
				for (int j = 0; j < this._requiredIngredients.Count; j++)
				{
					HudGui.BuildingIngredient icons = this.GetIcons(this._requiredIngredients[j]._itemID);
					if (this._requiredIngredients[j]._amount != this._presentIngredients[j]._amount)
					{
						Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[j];
						ReceipeIngredient receipeIngredient = this._presentIngredients[j];
						if (buildIngredients._amount > receipeIngredient._amount)
						{
							if (!LocalPlayer.Inventory.Owns(this._requiredIngredients[j]._itemID, true) && !Cheats.Creative)
							{
								icons._iconSprite.color = Scene.HudGui.BuildingIngredientMissing;
								icons._border.color = Scene.HudGui.BuildingIngredientMissing;
							}
							else
							{
								icons._iconSprite.color = Scene.HudGui.BuildingIngredientOwned;
								icons._border.color = Scene.HudGui.BuildingIngredientOwned;
								if (this._nextAddItem < Time.time && !flag)
								{
									flag = true;
									if (!Scene.HudGui.AddBuildingIngredientIcon.activeSelf)
									{
										Scene.HudGui.AddBuildingIngredientIcon.SetActive(true);
									}
									Scene.HudGui.AddBuildingIngredientIcon.transform.localPosition = icons._iconGo.transform.localPosition;
									Scene.HudGui.AddBuildingIngredientIcon.transform.rotation = icons._iconGo.transform.rotation;
									if (TheForest.Utils.Input.GetButtonDown("Build") || (Cheats.Creative && TheForest.Utils.Input.GetButton("Build")))
									{
										if (Cheats.Creative)
										{
											this._nextAddItem = Time.time + 0.065f;
										}
										this.AddIngredient(j);
										break;
									}
								}
							}
						}
					}
				}
				if (!flag && Scene.HudGui.AddBuildingIngredientIcon.activeSelf)
				{
					Scene.HudGui.AddBuildingIngredientIcon.SetActive(false);
				}
				if (Vector3.Dot(LocalPlayer.Transform.forward, this._uiFollowTarget.forward) < 0.75f || LocalPlayer.Transform.InverseTransformPoint(this._uiFollowTarget.position).z < 0f)
				{
					this.SetUpIcons();
				}
				else if (SteamClientDSConfig.isDedicatedClient)
				{
					bool flag2 = LocalPlayer.Transform.InverseTransformPoint(this._uiFollowTarget.position).z >= 0f;
					float num2 = Vector3.Dot(LocalPlayer.Transform.forward, this._uiFollowTarget.forward);
					bool flag3 = num2 > 0.35f && num2 < 0.8f;
					if (flag2 && flag3)
					{
						this.SetUpIcons();
					}
				}
			}
		}

		
		private void OnTriggerEnter(Collider c)
		{
			if (this.CustomLockCheck == null && this._useProximityLock && (c.CompareTag("Player") || c.CompareTag("PlayerNet")))
			{
				this._lockBuild = true;
			}
		}

		
		private void OnTriggerExit(Collider c)
		{
			if (this._useProximityLock && (c.CompareTag("Player") || c.CompareTag("PlayerNet")))
			{
				this._lockBuild = false;
				Scene.HudGui.CantPlaceIcon.SetActive(false);
			}
		}

		
		private void OnDestroy()
		{
			this.AllOff(this._initialized);
		}

		
		public void GrabEnter()
		{
			this.SetUpIcons();
			LocalPlayer.Inventory.DontShowDrop = true;
			base.enabled = true;
			this._grabbed = true;
		}

		
		public void GrabExit()
		{
			this.AllOff(false);
			base.enabled = false;
			this._grabbed = false;
		}

		
		public override void Attached()
		{
			if (BoltNetwork.isServer && this._initialized)
			{
				this.UpdateNetworkIngredients();
			}
		}

		
		public override void Detached()
		{
			this.AllOff(false);
		}

		
		public void SwapToNextGhost()
		{
			if (LocalPlayer.Sfx)
			{
				LocalPlayer.Sfx.PlayWhoosh();
			}
			if (BoltNetwork.isRunning && !this.entity.isOwner)
			{
				SwapGhost swapGhost = SwapGhost.Create(this.entity.source);
				swapGhost.GhostEntity = this.entity;
				swapGhost.Send();
			}
			else
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(this._swapTo, this._ghost.transform.position, this._ghost.transform.rotation);
				if (this._ghost.transform.parent)
				{
					gameObject.transform.parent = this._ghost.transform.parent;
				}
				Craft_Structure componentInChildren = gameObject.GetComponentInChildren<Craft_Structure>();
				if (componentInChildren)
				{
					componentInChildren._presentIngredients = new ReceipeIngredient[componentInChildren._requiredIngredients.Count];
					for (int i = 0; i < this._presentIngredients.Length; i++)
					{
						bool flag = false;
						for (int j = 0; j < componentInChildren._presentIngredients.Length; j++)
						{
							if (componentInChildren._presentIngredients[j] == null)
							{
								componentInChildren._presentIngredients[j] = new ReceipeIngredient
								{
									_itemID = this._presentIngredients[i]._itemID
								};
							}
							if (this._presentIngredients[i]._itemID == componentInChildren._presentIngredients[j]._itemID)
							{
								int num = this._requiredIngredients[i]._amount - this._presentIngredients[i]._amount;
								BuildMission.AddNeededToBuildMission(this._presentIngredients[i]._itemID, -num, true);
								componentInChildren._presentIngredients[j]._amount = this._presentIngredients[i]._amount;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							this.SpawnBackIngredients(i);
						}
					}
					componentInChildren.manualLoading = false;
					TreeStructure component = this._ghost.GetComponent<TreeStructure>();
					if (component)
					{
						TreeStructure treeStructure = gameObject.GetComponent<TreeStructure>();
						if (!treeStructure)
						{
							treeStructure = gameObject.AddComponent<TreeStructure>();
						}
						treeStructure.TreeId = component.TreeId;
					}
					if (BoltNetwork.isRunning && !gameObject.GetComponent<BoltEntity>().isAttached)
					{
						BoltNetwork.Attach(gameObject);
					}
					this.AllOff(false);
					if (BoltNetwork.isRunning)
					{
						BoltNetwork.Destroy(this._ghost);
					}
					else
					{
						UnityEngine.Object.Destroy(this._ghost);
					}
				}
				else
				{
					Debug.Log("Swap target isn't a ghost, aborting");
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}

		
		public ReceipeIngredient[] GetPresentIngredients()
		{
			return this._presentIngredients;
		}

		
		private HudGui.BuildingIngredient GetIcons(int itemId)
		{
			foreach (HudGui.BuildingIngredient buildingIngredient in Scene.HudGui._buildingIngredients)
			{
				if (itemId == buildingIngredient._itemId)
				{
					return buildingIngredient;
				}
			}
			return null;
		}

		
		private void AllOff(bool destroy = false)
		{
			try
			{
				if (Scene.HudGui)
				{
					Scene.HudGui.AddBuildingIngredientIcon.SetActive(false);
					Scene.HudGui.DestroyIcon.SetActive(false);
					Scene.HudGui.ToggleVariationIcon.SetActive(false);
					Scene.HudGui.CantPlaceIcon.SetActive(false);
					if (Scene.HudGui.BuildingIngredientsFollow.gameObject.activeSelf)
					{
						Scene.HudGui.BuildingIngredientsFollow.gameObject.SetActive(false);
					}
				}
				if (Application.isPlaying)
				{
					for (int i = 0; i < this._requiredIngredients.Count; i++)
					{
						Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[i];
						HudGui.BuildingIngredient icons = this.GetIcons(buildIngredients._itemID);
						if (icons._iconGo.activeSelf || icons._label.gameObject.activeSelf)
						{
							icons._iconGo.SetActive(false);
							icons._label.gameObject.SetActive(false);
						}
					}
				}
				if (LocalPlayer.Inventory)
				{
					LocalPlayer.Inventory.DontShowDrop = false;
				}
			}
			catch
			{
			}
		}

		
		private void CheckNeeded()
		{
			for (int i = 0; i < this._requiredIngredients.Count; i++)
			{
				if (this._presentIngredients[i]._amount < this._requiredIngredients[i]._amount)
				{
					return;
				}
			}
			this.Build();
		}

		
		private void SetUpIcons()
		{
			if (Application.isPlaying)
			{
				if (this._requiredIngredients != null)
				{
					if (!this._uiFollowTarget)
					{
						this._uiFollowTarget = new GameObject("UiFollowTarget").transform;
						this._uiFollowTarget.parent = base.transform;
					}
					if (this._bounds.size.sqrMagnitude == 0f)
					{
						this._bounds.center = base.transform.position;
						this._requiredIngredients.ForEach(delegate(Craft_Structure.BuildIngredients ri)
						{
							ri._renderers.ForEach(delegate(GameObject go)
							{
								Renderer component = go.GetComponent<Renderer>();
								if (component)
								{
									this._bounds.Encapsulate(component.bounds.center);
								}
								else if (go.transform.childCount > 0)
								{
									Transform child = go.transform.GetChild(0);
									component = child.GetComponent<Renderer>();
									if (component)
									{
										this._bounds.Encapsulate(component.bounds.center);
									}
									else
									{
										this._bounds.Encapsulate(child.position);
									}
								}
								this._bounds.Encapsulate(go.transform.position);
							});
						});
					}
					Vector3 position = LocalPlayer.MainCamTr.position;
					position.y = this._bounds.center.y;
					Vector3 forward = this._bounds.center + this.TriggerOffset - LocalPlayer.MainCamTr.position;
					float num = forward.magnitude * 0.9f;
					Vector3 vector = LocalPlayer.MainCamTr.position + forward.normalized * Mathf.Clamp(10f, 1f, num);
					vector.y = Mathf.Lerp(vector.y, LocalPlayer.MainCamTr.position.y, 0.5f);
					this._uiFollowTarget.position = vector;
					if (!this._bounds.Contains(position))
					{
						Ray ray = new Ray(LocalPlayer.MainCamTr.position, (this._uiFollowTarget.position - LocalPlayer.MainCamTr.position).normalized);
						float a;
						if (this._bounds.IntersectRay(ray, out a))
						{
							Vector3 point = ray.GetPoint(Mathf.Clamp(10f, 1f, Mathf.Min(a, num)));
							if (Vector3.Dot(LocalPlayer.Transform.forward, point - LocalPlayer.MainCamTr.position) > 0.35f)
							{
								this._uiFollowTarget.position = point;
							}
						}
						forward = this._uiFollowTarget.position - LocalPlayer.MainCamTr.position;
					}
					else if (Vector3.Dot(LocalPlayer.Transform.forward, vector - LocalPlayer.MainCamTr.position) < 0.1f)
					{
						this._uiFollowTarget.position = LocalPlayer.MainCamTr.position + LocalPlayer.MainCamTr.forward * Mathf.Clamp(10f, 1f, num);
						forward = this._uiFollowTarget.position - LocalPlayer.MainCamTr.position;
					}
					forward.y = 0f;
					this._uiFollowTarget.rotation = Quaternion.LookRotation(forward);
					Scene.HudGui.BuildingIngredientsFollow._target = this._uiFollowTarget;
					for (int i = this._requiredIngredients.Count - 1; i >= 0; i--)
					{
						Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[i];
						HudGui.BuildingIngredient icons = this.GetIcons(buildIngredients._itemID);
						icons._iconGo.transform.SetAsFirstSibling();
					}
					Scene.HudGui.BuildingIngredientsGrid.Reposition();
				}
				else
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		
		private void CheckText()
		{
			if (Application.isPlaying)
			{
				for (int i = 0; i < this._requiredIngredients.Count; i++)
				{
					Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[i];
					ReceipeIngredient receipeIngredient = this._presentIngredients[i];
					HudGui.BuildingIngredient icons = this.GetIcons(buildIngredients._itemID);
					if (buildIngredients != null && receipeIngredient != null && receipeIngredient._amount != buildIngredients._amount)
					{
						if (!icons._iconGo.activeSelf || !icons._label.gameObject.activeSelf)
						{
							icons._iconGo.SetActive(true);
							icons._label.gameObject.SetActive(true);
							Scene.HudGui.BuildingIngredientsGrid.repositionNow = true;
							if (!Scene.HudGui.BuildingIngredientsFollow.gameObject.activeSelf)
							{
								Scene.HudGui.BuildingIngredientsFollow.gameObject.SetActive(true);
							}
						}
						switch (buildIngredients._textDisplayMode)
						{
						case Craft_Structure.BuildIngredients.TextOptions.PresentOverTotal:
							if (icons.DisplayedPresent != receipeIngredient._amount || icons.DisplayedNeeded != buildIngredients._amount)
							{
								icons.DisplayedPresent = receipeIngredient._amount;
								icons.DisplayedNeeded = buildIngredients._amount;
								icons._label.text = receipeIngredient._amount + "/" + buildIngredients._amount;
							}
							break;
						case Craft_Structure.BuildIngredients.TextOptions.Present:
							icons._label.text = receipeIngredient._amount.ToString();
							break;
						case Craft_Structure.BuildIngredients.TextOptions.Total:
							icons._label.text = buildIngredients._amount.ToString();
							break;
						}
					}
					else if (icons._iconGo.activeSelf || icons._label.gameObject.activeSelf)
					{
						icons._iconGo.SetActive(false);
						icons._label.gameObject.SetActive(false);
					}
				}
			}
		}

		
		private void AddIngredient(int ingredientNum)
		{
			Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[ingredientNum];
			if (LocalPlayer.Inventory.RemoveItem(buildIngredients._itemID, 1, false, true) || Cheats.Creative)
			{
				LocalPlayer.Sfx.PlayHammer();
				if (!Cheats.Creative || Time.frameCount % 3 == 0)
				{
					Scene.HudGui.AddBuildingIngredientIcon.SetActive(false);
				}
				if (BoltNetwork.isRunning)
				{
					if (BoltNetwork.isClient)
					{
						bool flag = true;
						for (int i = 0; i < this._requiredIngredients.Count; i++)
						{
							if (this._presentIngredients[i]._amount < this._requiredIngredients[i]._amount && (i != ingredientNum || this._presentIngredients[i]._amount + 1 < this._requiredIngredients[i]._amount))
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							this.Build();
						}
					}
					AddIngredient addIngredient = global::AddIngredient.Create(GlobalTargets.OnlyServer);
					addIngredient.IngredientNum = ingredientNum;
					addIngredient.ItemId = buildIngredients._itemID;
					addIngredient.Construction = base.GetComponentInParent<BoltEntity>();
					addIngredient.Send();
				}
				else
				{
					this.AddIngrendient_Actual(ingredientNum, true, null);
				}
			}
		}

		
		public void UpdateNeededRenderers()
		{
			for (int i = 0; i < this._requiredIngredients.Count; i++)
			{
				Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[i];
				ReceipeIngredient receipeIngredient = this._presentIngredients[i];
				int num = 0;
				while (num < receipeIngredient._amount && num < buildIngredients._renderers.Length)
				{
					if (!buildIngredients._renderers[num].activeSelf)
					{
						buildIngredients._renderers[num].SetActive(true);
					}
					num++;
				}
			}
		}

		
		public void AddIngrendient_Actual(int ingredientNum, bool local, BoltConnection from = null)
		{
			Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[ingredientNum];
			ReceipeIngredient receipeIngredient = this._presentIngredients[ingredientNum];
			int num = 0;
			for (int i = 0; i < this._requiredIngredients.Count; i++)
			{
				num += this._requiredIngredients[i]._amount - this._presentIngredients[i]._amount;
			}
			if (receipeIngredient._amount >= buildIngredients._amount || (num == 1 && (this._lockBuild || (this.CustomLockCheck != null && this.CustomLockCheck()))))
			{
				if (BoltNetwork.isRunning && from && !Cheats.Creative)
				{
					PlayerAddItem playerAddItem = PlayerAddItem.Create(from);
					playerAddItem.ItemId = buildIngredients._itemID;
					playerAddItem.Amount = 1;
					playerAddItem.Send();
				}
				this.UpdateNetworkIngredients();
				return;
			}
			receipeIngredient._amount++;
			this.UpdateNeededRenderers();
			BuildMission.AddNeededToBuildMission(receipeIngredient._itemID, -1, false);
			if (BoltNetwork.isRunning)
			{
				this.UpdateNetworkIngredients();
			}
			this.CheckNeeded();
		}

		
		public void UpdateNetworkIngredients()
		{
			if (BoltNetwork.isRunning)
			{
				BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
				if (componentInParent.isOwner && this._presentIngredients != null)
				{
					IConstructionState state = componentInParent.GetState<IConstructionState>();
					for (int i = 0; i < this._presentIngredients.Length; i++)
					{
						state.Ingredients[i].Count = 0;
						state.Ingredients[i].Count = this._presentIngredients[i]._amount;
					}
				}
				componentInParent.Freeze(false);
			}
		}

		
		public void CancelBlueprintSafe()
		{
			GameStats.CancelledStructure.Invoke();
			for (int i = 0; i < this._requiredIngredients.Count; i++)
			{
				this.SpawnBackIngredients(i);
			}
			this.CheckText();
			this.AllOff(false);
			if (BoltNetwork.isRunning && this.entity.isAttached)
			{
				BoltNetwork.Destroy(this.entity);
			}
			else
			{
				UnityEngine.Object.Destroy(this._ghost);
			}
			base.enabled = false;
		}

		
		public void CancelBlueprint()
		{
			if (BoltNetwork.isRunning)
			{
				CancelBluePrint cancelBluePrint = CancelBluePrint.Create(GlobalTargets.OnlyServer);
				cancelBluePrint.BluePrint = this.entity;
				cancelBluePrint.Send();
			}
			else
			{
				this.CancelBlueprintSafe();
			}
			if (LocalPlayer.Sfx)
			{
				LocalPlayer.Sfx.PlayRemove();
			}
		}

		
		public void SpawnBackIngredients(int ingredientId)
		{
			Craft_Structure.BuildIngredients buildIngredients = this._requiredIngredients[ingredientId];
			ReceipeIngredient receipeIngredient = this._presentIngredients[ingredientId];
			if (buildIngredients != null && receipeIngredient != null)
			{
				int num = buildIngredients._amount - receipeIngredient._amount;
				BuildMission.AddNeededToBuildMission(receipeIngredient._itemID, -num, true);
				int amount = this._presentIngredients[ingredientId]._amount;
				if (amount > 0)
				{
					Transform transform = BoltNetwork.isRunning ? ItemDatabase.ItemById(this._presentIngredients[ingredientId]._itemID)._pickupPrefabMP : ItemDatabase.ItemById(this._presentIngredients[ingredientId]._itemID)._pickupPrefab;
					if (transform)
					{
						Craft_Structure.PickupAxis pickupAxis = this._requiredIngredients[ingredientId]._pickupAxis;
						float f = (float)amount * 0.428571433f + 1f;
						int num2 = Mathf.Min(Mathf.RoundToInt(f), 10);
						for (int i = 0; i < num2; i++)
						{
							int num3 = Mathf.RoundToInt((float)i / (float)num2 * (float)amount);
							if (this._requiredIngredients[ingredientId]._renderers.Length <= num3)
							{
								break;
							}
							Transform transform2 = this._requiredIngredients[ingredientId]._renderers[num3].transform;
							Transform transform3 = BoltNetwork.isRunning ? BoltNetwork.Instantiate(transform.gameObject).transform : UnityEngine.Object.Instantiate<Transform>(transform);
							transform3.position = transform2.position;
							switch (pickupAxis)
							{
							case Craft_Structure.PickupAxis.Z:
								transform3.rotation = transform2.rotation;
								break;
							case Craft_Structure.PickupAxis.X:
								transform3.rotation = Quaternion.LookRotation(transform2.right);
								break;
							case Craft_Structure.PickupAxis.Y:
								transform3.rotation = Quaternion.LookRotation(transform2.up);
								break;
							}
						}
					}
				}
			}
		}

		
		private void BuildFx()
		{
		}

		
		private void Build()
		{
			if (this._type != BuildingTypes.None)
			{
				EventRegistry.Player.Publish(TfEvent.BuiltStructure, this._type);
				this._type = BuildingTypes.None;
			}
			if (BoltNetwork.isClient)
			{
				if (base.enabled)
				{
					base.enabled = false;
					this.AllOff(false);
				}
				return;
			}
			if (!this._ghost)
			{
				this._ghost = base.transform.parent.gameObject;
			}
			GameObject gameObject;
			if (BoltNetwork.isServer)
			{
				if (this.entity.attachToken != null)
				{
					if (this.entity.attachToken is CoopWallChunkToken)
					{
						(this.entity.attachToken as CoopWallChunkToken).Additions = this.entity.GetComponent<WallChunkArchitect>().Addition;
					}
					gameObject = BoltNetwork.Instantiate(this.Built, this.entity.attachToken, this._ghost.transform.position, this._ghost.transform.rotation).gameObject;
				}
				else
				{
					gameObject = BoltNetwork.Instantiate(this.Built, this.entity.attachToken, this._ghost.transform.position, this._ghost.transform.rotation).gameObject;
					BoltEntity component = gameObject.GetComponent<BoltEntity>();
					if (component && component.isAttached && component.StateIs<IMultiHolderState>())
					{
						component.GetState<IMultiHolderState>().IsReal = true;
					}
					BoltEntity component2 = gameObject.GetComponent<BoltEntity>();
					if (component2 && component.isAttached && component2.StateIs<IRaftState>())
					{
						component2.GetState<IRaftState>().IsReal = true;
					}
				}
			}
			else
			{
				gameObject = (GameObject)UnityEngine.Object.Instantiate(this.Built, this._ghost.transform.position, this._ghost.transform.rotation);
			}
			TreeStructure component3 = this._ghost.GetComponent<TreeStructure>();
			if (component3)
			{
				TreeStructure treeStructure = gameObject.GetComponent<TreeStructure>();
				if (!treeStructure)
				{
					treeStructure = gameObject.AddComponent<TreeStructure>();
				}
				treeStructure.TreeId = component3.TreeId;
			}
			ropeSetGroundHeight component4 = gameObject.GetComponent<ropeSetGroundHeight>();
			if (component4)
			{
				gameObject.SendMessage("setGroundTriggerHeight", SendMessageOptions.DontRequireReceiver);
			}
			if (this._ghost.transform.parent != null)
			{
				gameObject.transform.parent = this._ghost.transform.parent;
			}
			this.OnBuilt(gameObject);
			this.OnBuilt = null;
			base.enabled = false;
			this._initialized = false;
			if (this._ghost)
			{
				base.StartCoroutine(this.DelayedDestroy());
			}
			else
			{
				this.AllOff(true);
			}
			if (this._playTwinkle && LocalPlayer.Sfx)
			{
				LocalPlayer.Sfx.PlayBuildingComplete(gameObject, true);
			}
			else if (SteamDSConfig.isDedicatedServer)
			{
				FmodOneShot fmodOneShot = FmodOneShot.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
				fmodOneShot.EventPath = CoopAudioEventDb.FindId("event:/ui/ingame/ui_complete");
				fmodOneShot.Position = gameObject.transform.position;
				fmodOneShot.Send();
			}
		}

		
		private IEnumerator DelayedDestroy()
		{
			yield return null;
			this.AllOff(true);
			UnityEngine.Object.Destroy(this._ghost);
			yield break;
		}

		
		
		
		public bool WasLoaded { get; set; }

		
		
		
		public bool IsInitialized
		{
			get
			{
				return this._initialized;
			}
			set
			{
				this._initialized = value;
			}
		}

		
		
		
		public Vector3 TriggerOffset { get; set; }

		
		public bool manualLoading;

		
		public Color ColorGrey;

		
		public Color ColorRed;

		
		public GameObject Built;

		
		public BuildingTypes _type;

		
		public List<Craft_Structure.BuildIngredients> _requiredIngredients;

		
		public bool _playTwinkle = true;

		
		public bool _useProximityLock = true;

		
		public GameObject _swapTo;

		
		[SerializeThis]
		private ReceipeIngredient[] _presentIngredients;

		
		[SerializeThis]
		private int _presentIngredientsCount;

		
		private bool _initialized;

		
		private bool _grabbed;

		
		private bool _lockBuild;

		
		private GameObject _ghost;

		
		private float _nextAddItem;

		
		private Transform _uiFollowTarget;

		
		private Bounds _bounds;

		
		public Func<bool> CustomLockCheck;

		
		public Action<GameObject> OnBuilt = delegate
		{
		};

		
		[Serializable]
		public class BuildIngredients : ReceipeIngredient
		{
			
			public GameObject[] _renderers;

			
			public Craft_Structure.BuildIngredients.TextOptions _textDisplayMode;

			
			public Craft_Structure.PickupAxis _pickupAxis;

			
			public enum TextOptions
			{
				
				PresentOverTotal,
				
				Present,
				
				Total
			}
		}

		
		public enum PickupAxis
		{
			
			X = 1,
			
			Y,
			
			Z = 0
		}
	}
}
