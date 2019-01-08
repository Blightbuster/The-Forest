using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.Interfaces;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[DoNotSerializePublic]
	public class Create : MonoBehaviour
	{
		private void Awake()
		{
			this.CraftStructures = new List<Craft_Structure>();
		}

		private void Update()
		{
			if ((LocalPlayer.AnimControl.onRope || LocalPlayer.AnimControl.useRootMotion || LocalPlayer.AnimControl.onRockThrower || LocalPlayer.FpCharacter.drinking || LocalPlayer.AnimControl.holdingGirl) && this.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				this.ShouldOpenBook = false;
				this.CloseTheBook(false);
				return;
			}
			if (this.IsClosing)
			{
				if (!PlayerPreferences.CanUpdateFov)
				{
					if (!ForestVR.Enabled)
					{
						LocalPlayer.MainCam.fieldOfView = Mathf.SmoothDamp(LocalPlayer.MainCam.fieldOfView, PlayerPreferences.Fov, ref this._fovChangeSpeed, 0.15f);
					}
					if (Mathf.Approximately(LocalPlayer.MainCam.fieldOfView, PlayerPreferences.Fov))
					{
						this.IsClosing = false;
						PlayerPreferences.CanUpdateFov = true;
					}
				}
			}
			else if (this.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				if ((!TheForest.Utils.Input.IsGamePad) ? TheForest.Utils.Input.GetButtonDown("Esc") : TheForest.Utils.Input.GetButtonDown("Back"))
				{
					this.CloseTheBook(false);
				}
				if (!PlayerPreferences.CanUpdateFov && !Mathf.Approximately(LocalPlayer.MainCam.fieldOfView, 65f) && !ForestVR.Enabled)
				{
					LocalPlayer.MainCam.fieldOfView = Mathf.SmoothDamp(LocalPlayer.MainCam.fieldOfView, 70f, ref this._fovChangeSpeed, 0.15f);
				}
				VirtualCursor.Instance.SetCursorType((!TheForest.Utils.Input.IsGamePad) ? VirtualCursor.CursorTypes.Arrow : VirtualCursor.CursorTypes.None);
			}
			if (this.ShouldOpenBook)
			{
				this.OpenBook();
			}
			if (this.LockPlace)
			{
				this.LockPlace = false;
			}
			else if (this.CreateMode)
			{
				if (!this.ShownPlace)
				{
					this.Grabber.ShowPlace();
					this.ShownPlace = true;
				}
				bool button = TheForest.Utils.Input.GetButton("Batch");
				if (Scene.HudGui.BatchPlaceIcon.activeSelf != button)
				{
					Scene.HudGui.BatchPlaceIcon.SetActive(button);
				}
				if ((!TheForest.Utils.Input.IsGamePad) ? TheForest.Utils.Input.GetButtonDown("Esc") : FirstPersonCharacter.GetDropInput())
				{
					this.CancelPlace();
				}
				else if (this._buildingPlacer.Clear && TheForest.Utils.Input.GetButtonDown("Build"))
				{
					this.PlaceGhost(button);
				}
			}
		}

		public void CloseBuildMode()
		{
			this.CreateMode = false;
		}

		public void CancelPlace()
		{
			if (this._currentGhost)
			{
				LocalPlayer.Sfx.PlayWhoosh();
				UnityEngine.Object.Destroy(this._currentGhost);
			}
			this.ClearReferences(true);
			this.CreateMode = false;
		}

		public void PlaceGhost(bool chain = false)
		{
			base.StartCoroutine(this.PlaceGhostRoutine(chain));
		}

		private IEnumerator PlaceGhostRoutine(bool chain)
		{
			BuildingTypes bType = this.CurrentBlueprint._type;
			yield return null;
			if (this.CreateMode)
			{
				this.ShownPlace = false;
				this.CreateMode = false;
				if (!this.ToolsShown)
				{
					base.SendMessage("BuildToolsTut");
					this.ToolsShown = true;
				}
				TreeStructure ts = this._currentGhost.GetComponentInChildren<TreeStructure>();
				if (ts && this.TargetTree)
				{
					if (this.TargetTree.CompareTag("conTree"))
					{
						ts.TreeId = this.TargetTree.parent.GetComponent<TreeHealth>().LodTree.GetComponentInChildren<CoopTreeId>().Id;
					}
					else
					{
						ts.TreeId = this.TargetTree.GetComponent<TreeHealth>().LodTree.GetComponentInChildren<CoopTreeId>().Id;
					}
					ts.enabled = true;
					if (!BoltNetwork.isRunning)
					{
						Scene.ActiveMB.StartCoroutine(ts.OnDeserialized());
					}
				}
				CoopConstructionEx coopEx = this._currentGhost.GetComponent<CoopConstructionEx>();
				WallArchitect wallArch = this.CurrentGhost.GetComponent<WallArchitect>();
				ICoopTokenConstruction tokenConstruction = this._currentGhost.GetComponent<ICoopTokenConstruction>();
				this.ParentEntity = this.GetParentEntity(this._currentGhost);
				if (BoltNetwork.isRunning && !wallArch)
				{
					BoltEntity component = this._currentGhost.GetComponent<BoltEntity>();
					if (tokenConstruction != null)
					{
						PlaceFoundationEx placeFoundationEx = PlaceFoundationEx.Create(GlobalTargets.OnlyServer);
						placeFoundationEx.Position = this._currentGhost.transform.position;
						placeFoundationEx.Rotation = this._currentGhost.transform.rotation;
						placeFoundationEx.Prefab = component.prefabId;
						placeFoundationEx.Token = tokenConstruction.CustomToken;
						placeFoundationEx.Parent = this.ParentEntity;
						placeFoundationEx.Send();
						UnityEngine.Object.Destroy(this._currentGhost);
					}
					else if (coopEx)
					{
						coopEx.SendMessage("OnSerializing");
						CoopConstructionExToken coopConstructionExToken = this.GetCoopConstructionExToken(coopEx, this.ParentEntity);
						PlaceFoundationEx placeFoundationEx2 = PlaceFoundationEx.Create(GlobalTargets.OnlyServer);
						placeFoundationEx2.Parent = this.ParentEntity;
						placeFoundationEx2.Position = this._currentGhost.transform.position;
						placeFoundationEx2.Rotation = this._currentGhost.transform.rotation;
						placeFoundationEx2.Prefab = component.prefabId;
						placeFoundationEx2.Token = coopConstructionExToken;
						placeFoundationEx2.Send();
						UnityEngine.Object.Destroy(this._currentGhost);
					}
					else if (!this._currentGhost.GetComponent(typeof(IAnchorableStructure)) && component)
					{
						this._currentGhost.AddComponent<CoopDestroyPredictedGhost>();
						this._currentGhost.AddComponent<destroyAfter>().destroyTime = 2f;
						PlaceConstruction placeConstruction = PlaceConstruction.Create(GlobalTargets.OnlyServer);
						placeConstruction.Parent = this.ParentEntity;
						placeConstruction.PrefabId = component.prefabId;
						placeConstruction.Position = this._currentGhost.transform.position;
						placeConstruction.Rotation = this._currentGhost.transform.rotation;
						if (ts)
						{
							placeConstruction.TreeIndex = ts.TreeId;
						}
						placeConstruction.Send();
						this._currentGhost.SendMessage("OnPlacingRemotely", SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						this._currentGhost.SendMessage("OnPlaced", false, SendMessageOptions.DontRequireReceiver);
					}
				}
				else
				{
					this._currentGhost.SendMessage("OnPlaced", false, SendMessageOptions.DontRequireReceiver);
					this._currentGhost.transform.Find("Trigger").gameObject.SetActive(true);
					Transform transform = this._currentGhost.transform.Find("LastBuiltLocation");
					if (transform)
					{
						transform.gameObject.SetActive(true);
					}
					if (this.ParentEntity)
					{
						DynamicBuilding component2 = this.ParentEntity.GetComponent<DynamicBuilding>();
						this._currentGhost.transform.parent = ((!component2 || !component2._parentOverride) ? this.ParentEntity.transform : component2._parentOverride);
					}
				}
				this.ClearReferences(!chain);
				LocalPlayer.Sfx.PlayPlaceGhost();
				this.RefreshGrabber();
				yield return YieldPresets.WaitPointFiveSeconds;
				if (chain)
				{
					this.MultiPlaceAction(bType);
				}
			}
			yield break;
		}

		private void MultiPlaceAction(BuildingTypes type)
		{
			if (this.Inventory.CurrentView != PlayerInventory.PlayerViews.World || LocalPlayer.AnimControl.onRope || LocalPlayer.AnimControl.useRootMotion || LocalPlayer.AnimControl.onRockThrower || LocalPlayer.Animator.GetBool("logHeld"))
			{
				return;
			}
			LocalPlayer.Inventory.BlockTogglingInventory = false;
			this.CreateBuilding(type);
			LocalPlayer.Create.CloseTheBook(false);
		}

		public BoltEntity GetParentEntity(GameObject ghost)
		{
			BoltEntity boltEntity = null;
			SingleAnchorStructure component = ghost.GetComponent<SingleAnchorStructure>();
			if (component && component.Anchor1)
			{
				component.enabled = false;
				boltEntity = component.Anchor1.GetComponentInParent<BoltEntity>();
			}
			if (this._buildingPlacer.ForcedParent)
			{
				boltEntity = this._buildingPlacer.ForcedParent.GetComponentInParent<BoltEntity>();
				this._buildingPlacer.ForcedParent = null;
			}
			else if (this._buildingPlacer.LastHit != null && !this._currentBlueprint._isDynamic && !boltEntity)
			{
				boltEntity = this._buildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>();
			}
			if (!boltEntity)
			{
				boltEntity = null;
			}
			else
			{
				DynamicBuilding component2 = boltEntity.GetComponent<DynamicBuilding>();
				if (component2 && (!this._currentBlueprint._allowParentingToDynamic || !component2._allowParenting))
				{
					boltEntity = null;
				}
			}
			return boltEntity;
		}

		public CoopConstructionExToken GetCoopConstructionExToken(CoopConstructionEx coopEx, BoltEntity parentEntity)
		{
			CoopConstructionExToken coopConstructionExToken = new CoopConstructionExToken();
			coopConstructionExToken.Architects = new CoopConstructionExToken.ArchitectData[coopEx.Architects.Length];
			for (int i = 0; i < coopEx.Architects.Length; i++)
			{
				coopConstructionExToken.Parent = parentEntity;
				coopConstructionExToken.Architects[i].PointsCount = (coopEx.Architects[i] as ICoopStructure).MultiPointsCount;
				coopConstructionExToken.Architects[i].PointsPositions = (coopEx.Architects[i] as ICoopStructure).MultiPointsPositions.ToArray();
				coopConstructionExToken.Architects[i].CustomToken = (coopEx.Architects[i] as ICoopStructure).CustomToken;
				if (coopEx.Architects[i] is FoundationArchitect)
				{
					coopConstructionExToken.Architects[i].AboveGround = ((FoundationArchitect)coopEx.Architects[i])._aboveGround;
				}
				else if (coopEx.Architects[i] is RoofArchitect)
				{
					if ((coopEx.Architects[i] as RoofArchitect).CurrentSupport != null)
					{
						coopConstructionExToken.Architects[i].Support = ((coopEx.Architects[i] as RoofArchitect).CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>();
					}
				}
				else if (coopEx.Architects[i] is FloorArchitect && (coopEx.Architects[i] as FloorArchitect).CurrentSupport != null)
				{
					coopConstructionExToken.Architects[i].Support = ((coopEx.Architects[i] as FloorArchitect).CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>();
				}
			}
			return coopConstructionExToken;
		}

		public void RefreshGrabber()
		{
			base.StartCoroutine(this.RefreshGrabberRoutine());
		}

		private IEnumerator RefreshGrabberRoutine()
		{
			yield return YieldPresets.WaitPointOneSeconds;
			this.Grabber.GetComponent<Collider>().enabled = false;
			this.Grabber.GetComponent<Collider>().enabled = true;
			yield break;
		}

		private void ClearReferences(bool equipPrevious)
		{
			if (this._currentGhost)
			{
				if (this._currentGhost.transform.parent == this._buildingPlacer.transform)
				{
					this._currentGhost.transform.parent = null;
				}
				this._currentGhost = null;
			}
			this._currentBlueprint = null;
			this.CraftStructures.Clear();
			if (this._buildingPlacer)
			{
				this._buildingPlacer.ValidateAnchor = null;
				this._buildingPlacer.SetRenderer(null);
				this._buildingPlacer.gameObject.SetActive(false);
			}
			this.Grabber.ClosePlace();
			Scene.HudGui.WallConstructionIcons.Shutdown();
			Scene.HudGui.DefensiveWallConstructionIcons.Shutdown();
			Scene.HudGui.RoofConstructionIcons.Shutdown();
			Scene.HudGui.FoundationConstructionIcons.Shutdown();
			Scene.HudGui.CantPlaceIcon.SetActive(false);
			Scene.HudGui.BatchPlaceIcon.SetActive(false);
			Scene.HudGui.SupportPlacementGizmo.Hide();
			Scene.HudGui.MultipointShapeGizmo.Shutdown();
			if (equipPrevious)
			{
				this.RestoreEquipement();
			}
			LocalPlayer.FpCharacter.CanJump = true;
		}

		private void RestoreEquipement()
		{
			if (this.ShouldEquipLeftHandAfter)
			{
				this.Inventory.EquipPreviousUtility(false);
			}
			if (this.ShouldEquipRightHandAfter)
			{
				this.Inventory.EquipPreviousWeaponDelayed();
			}
		}

		public void OpenBook()
		{
			if (this.enterCreateModeCoolDown > Time.time || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				return;
			}
			if (LocalPlayer.AnimControl.onRope || LocalPlayer.AnimControl.useRootMotion || LocalPlayer.AnimControl.onRockThrower || LocalPlayer.Animator.GetBool("logHeld") || LocalPlayer.AnimControl.carry || LocalPlayer.vrAdapter.TheatreController.theatreOn || LocalPlayer.AnimControl.fullBodyState2.tagHash == LocalPlayer.AnimControl.getupHash || LocalPlayer.AnimControl.holdingGlider)
			{
				this.ShouldOpenBook = false;
				this.CloseTheBook(false);
				return;
			}
			if (LocalPlayer.FpCharacter.Grounded || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory)
			{
				Scene.HudGui.ClearMpPlayerList();
				base.StartCoroutine(this.OpenBookSequence());
			}
			else
			{
				this.ShouldOpenBook = true;
			}
		}

		private IEnumerator OpenBookSequence()
		{
			this.enterCreateModeCoolDown = Time.time + 0.25f;
			if (LocalPlayer.Inventory.IsSlotLocked(Item.EquipmentSlot.RightHand))
			{
				LocalPlayer.Inventory.StopPendingEquip();
				yield return null;
				yield return null;
			}
			if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				yield break;
			}
			LocalPlayer.Tuts.CloseCraftingTut();
			LocalPlayer.Inventory.BlockTogglingInventory = true;
			if (LocalPlayer.AnimControl.USE_NEW_BOOK && (!LocalPlayer.AnimControl.bookHeldGo.GetComponent<survivalBookController>().realBookOpen || (LocalPlayer.AnimControl.currLayerState1.shortNameHash == LocalPlayer.AnimControl.bookIdleToIdleHash && LocalPlayer.AnimControl.currLayerState1.normalizedTime > 0.5f)))
			{
				base.CancelInvoke("showEquipped");
				this.ShouldOpenBook = false;
				Scene.HudGui.ShowHud(false);
				this.CreateMode = false;
				if (this._currentGhost != null)
				{
					UnityEngine.Object.Destroy(this._currentGhost);
					this.ClearReferences(false);
				}
				this.ShouldEquipLeftHandAfter = !LocalPlayer.Inventory.IsLeftHandEmpty();
				this.ShouldEquipRightHandAfter = !LocalPlayer.Inventory.IsRightHandEmpty();
				if (LocalPlayer.Animator.GetBool("lookAtPhoto") || LocalPlayer.Animator.GetBool("lookAtItemRight") || LocalPlayer.Animator.GetBool("lookAtItem"))
				{
					this.Inventory.HideAllEquiped(true, true);
					LocalPlayer.Stats.cancelCheckItem();
				}
				else
				{
					this.Inventory.HideAllEquiped(true, false);
				}
				this.ShownPlace = false;
				LocalPlayer.AnimControl.cancelAnimatorActions();
				this.Inventory.Close();
				this.Inventory.CurrentView = PlayerInventory.PlayerViews.Book;
				LocalPlayer.Animator.SetBoolReflected("bookHeld", true);
				LocalPlayer.Tuts.HideStoryClueTut();
				LocalPlayer.FpCharacter.CanJump = false;
				this.IsClosing = false;
				PlayerPreferences.CanUpdateFov = true;
				base.Invoke("LockFoV", 0.45f);
			}
			yield break;
		}

		private void LockFoV()
		{
			PlayerPreferences.CanUpdateFov = false;
		}

		public void CloseBookForInventory()
		{
			this.CloseTheBook(true);
			LocalPlayer.AnimControl.bookHeldGo.SendMessage("fastCloseBook", SendMessageOptions.DontRequireReceiver);
		}

		public void CloseTheBook(bool toInventory = false)
		{
			if (this.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				if (!this.CreateMode && !LocalPlayer.AnimControl.upsideDown)
				{
					if (!ForestVR.Enabled)
					{
						LocalPlayer.Inventory.enabled = false;
					}
					if (toInventory)
					{
						this.showEquipped();
					}
					else
					{
						VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.Arrow);
						base.Invoke("showEquipped", 0.65f);
					}
				}
				else
				{
					this.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
				}
				if (LocalPlayer.AnimControl.USE_NEW_BOOK)
				{
					base.CancelInvoke("LockFoV");
					Scene.HudGui.ShowHud(true);
					LocalPlayer.FpCharacter.UnLockView();
					LocalPlayer.FpCharacter.CanJump = true;
					if (this.CreateMode)
					{
						LocalPlayer.Animator.SetInteger("bookCloseInt", 1);
					}
					else
					{
						LocalPlayer.Animator.SetInteger("bookCloseInt", 0);
					}
					LocalPlayer.Animator.SetBoolReflected("bookHeld", false);
					this.IsClosing = true;
				}
				this.ShouldOpenBook = false;
			}
		}

		private void showEquipped()
		{
			if (this.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				LocalPlayer.Inventory.BlockTogglingInventory = false;
				this.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
				this.RestoreEquipement();
			}
			LocalPlayer.Inventory.enabled = true;
		}

		private Bounds GetGhostBounds(GameObject ghost)
		{
			Bounds result = default(Bounds);
			GhostRendererSelector[] componentsInChildren = ghost.GetComponentsInChildren<GhostRendererSelector>();
			if (componentsInChildren.Length > 0)
			{
				ghost.transform.localRotation = Quaternion.Inverse(componentsInChildren[0].transform.rotation);
				this._buildingPlacer.SetRenderer(componentsInChildren[0].GetComponent<Renderer>());
				result = componentsInChildren[0].GetComponent<Renderer>().bounds;
				for (int i = 1; i < componentsInChildren.Length; i++)
				{
					result.Encapsulate(componentsInChildren[i].GetComponent<Renderer>().bounds);
				}
			}
			else
			{
				this._buildingPlacer.SetRenderer(ghost.GetComponent<Renderer>());
				GhostColliderSelector[] componentsInChildren2 = ghost.GetComponentsInChildren<GhostColliderSelector>();
				if (componentsInChildren2.Length > 0)
				{
					ghost.transform.localRotation = Quaternion.Inverse(componentsInChildren2[0].transform.rotation);
					result = componentsInChildren2[0].GetComponent<Collider>().bounds;
					for (int j = 1; j < componentsInChildren2.Length; j++)
					{
						result.Encapsulate(componentsInChildren2[j].GetComponent<Collider>().bounds);
					}
				}
				else if (ghost.GetComponent<Renderer>())
				{
					result = ghost.GetComponent<Renderer>().bounds;
				}
			}
			return result;
		}

		public Vector3 GetGhostOffsetWithPlacer(GameObject ghost)
		{
			BoxCollider boxCollider = (BoxCollider)this._buildingPlacer.GetComponent<Collider>();
			Bounds ghostBounds = this.GetGhostBounds(ghost);
			boxCollider.size = ghostBounds.size;
			boxCollider.center = new Vector3(0f, ghostBounds.extents.y * 1.05f, 0f);
			return ghost.transform.position - ghostBounds.center + boxCollider.center;
		}

		public void InitPlacer(BuildingBlueprint bp)
		{
			if (this._buildingPlacer && this._buildingPlacer.gameObject.activeSelf)
			{
				this._buildingPlacer.gameObject.SetActive(false);
			}
			this._buildingPlacer = this.BuildingPlacerCloseMedFar[(int)((!ForestVR.Enabled) ? bp._placerDistance : PlacerDistance.Far)];
			this._buildingPlacer.gameObject.SetActive(true);
			this._buildingPlacer.transform.rotation = Quaternion.identity;
		}

		public void CreateBuilding(BuildingTypes type)
		{
			this._currentBlueprint = Prefabs.Instance.Constructions._blueprints.Find((BuildingBlueprint bp) => bp._type == type);
			if (this._currentBlueprint == null)
			{
				Debug.LogError("Building blueprint not found on Create script for " + type);
			}
			else
			{
				this.InitPlacer(this._currentBlueprint);
				if (!BoltNetwork.isRunning || !this._currentBlueprint._ghostPrefabMP)
				{
					this._currentGhost = UnityEngine.Object.Instantiate<GameObject>(this._currentBlueprint._ghostPrefab);
				}
				else
				{
					this._currentGhost = UnityEngine.Object.Instantiate<GameObject>(this._currentBlueprint._ghostPrefabMP);
				}
				this._currentGhost.transform.parent = this._buildingPlacer.transform;
				this._currentGhost.transform.localRotation = Quaternion.identity;
				Transform transform = this._currentGhost.transform.Find("Trigger");
				Craft_Structure component = transform.GetComponent<Craft_Structure>();
				if (component)
				{
					this.CraftStructures.Add(component);
				}
				transform.gameObject.SetActive(false);
				Renderer component2 = this._currentGhost.GetComponent<Renderer>();
				if (component2)
				{
					component2.enabled = false;
					base.StartCoroutine(this.EnableRendererAfterDelay(component2));
				}
				Transform transform2 = this._currentGhost.transform.Find("LastBuiltLocation");
				if (transform2)
				{
					transform2.gameObject.SetActive(false);
				}
				this._currentGhost.transform.localPosition = this.GetGhostOffsetWithPlacer(this._currentGhost);
				this._buildingPlacer.TreeStructure = this._currentBlueprint._allowInTree;
				this._buildingPlacer.IgnoreLookAtCollision = this._currentBlueprint._ignoreLookAtCollision;
				this._buildingPlacer.IgnoreBlock = this._currentBlueprint._ignoreBlock;
				this._buildingPlacer.AllowFoundation = this._currentBlueprint._allowFoundation;
				this._buildingPlacer.Airborne = this._currentBlueprint._airBorne;
				IAnchorValidation component3 = this._currentGhost.GetComponent<IAnchorValidation>();
				if (component3 != null)
				{
					this._buildingPlacer.ValidateAnchor = new Func<Transform, bool>(component3.ValidateAnchor);
				}
				this._buildingPlacer.ShowAnchorArea.SetActive(this._currentBlueprint._showAnchors);
				this._buildingPlacer.ShowSupportAnchorArea.SetActive(this._currentBlueprint._showSupportAnchor);
				if (this._currentBlueprint._waterborne)
				{
					this._buildingPlacer.SetWaterborne(this._currentBlueprint._waterborneExclusive);
				}
				if (this._currentBlueprint._hydrophobic)
				{
					this._buildingPlacer.SetHydrophobic();
				}
				this.BeginCoolDown();
				this.CreateMode = true;
				this.LockPlace = true;
				this.ShouldOpenBook = false;
				this.Inventory.EquipPreviousUtility(true);
			}
		}

		private IEnumerator EnableRendererAfterDelay(Renderer r)
		{
			yield return YieldPresets.WaitPointZeroFiveSeconds;
			r.enabled = true;
			yield break;
		}

		public void BeginCoolDown()
		{
			this.enterCreateModeCoolDown = Time.time + 0.7f;
		}

		private void ClosedCreate()
		{
			LocalPlayer.FpCharacter.UnLockView();
		}

		public static void ApplyGhostMaterial(Transform targetTransform, bool recursive = true)
		{
			if (targetTransform == null)
			{
				return;
			}
			if (!recursive)
			{
				Create.ApplyGhostMaterial(targetTransform.GetComponentInChildren<Renderer>());
				return;
			}
			foreach (Renderer targetRenderer in targetTransform.GetComponentsInChildren<Renderer>())
			{
				Create.ApplyGhostMaterial(targetRenderer);
			}
		}

		public static void ApplyGhostMaterial(Renderer targetRenderer)
		{
			if (targetRenderer == null)
			{
				return;
			}
			targetRenderer.sharedMaterial = Create.CurrentGhostMat;
		}

		public BuildingBlueprint CurrentBlueprint
		{
			get
			{
				return this._currentBlueprint;
			}
		}

		public GameObject CurrentGhost
		{
			get
			{
				return this._currentGhost;
			}
		}

		public KeepAboveTerrain BuildingPlacer
		{
			get
			{
				return this._buildingPlacer;
			}
		}

		public List<Craft_Structure> CraftStructures { get; private set; }

		public BoltEntity ParentEntity { get; private set; }

		public static bool CanLock { get; set; }

		public static Material CurrentGhostMat
		{
			get
			{
				return (!Create.CanLock) ? Prefabs.Instance.GhostBlocked : Prefabs.Instance.GhostClear;
			}
		}

		public KeepAboveTerrain[] BuildingPlacerCloseMedFar;

		public WallClick Grabber;

		public Transform TargetTree;

		public GameObject SurvivalBook;

		public PlayerInventory Inventory;

		[HideInInspector]
		public bool CreateMode;

		private KeepAboveTerrain _buildingPlacer;

		private bool ShouldEquipLeftHandAfter;

		private bool ShouldEquipRightHandAfter;

		private bool ShouldOpenBook;

		private bool ToolsShown;

		private bool ShownPlace;

		private bool LockPlace;

		private bool IsClosing;

		private float enterCreateModeCoolDown;

		private BuildingBlueprint _currentBlueprint;

		private GameObject _currentGhost;

		private float _fovChangeSpeed;
	}
}
