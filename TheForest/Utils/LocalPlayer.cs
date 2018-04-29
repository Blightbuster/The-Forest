using System;
using System.Collections;
using TheForest.Buildings.Creation;
using TheForest.Buildings.World;
using TheForest.Commons.Enums;
using TheForest.Graphics;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.Items.World.Interfaces;
using TheForest.Modding.Bridge;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.Player;
using TheForest.Player.Actions;
using TheForest.Player.Clothing;
using TheForest.Player.Data;
using TheForest.World;
using TheForest.World.Areas;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace TheForest.Utils
{
	
	public class LocalPlayer : MonoBehaviour, ILocalPlayer
	{
		
		
		
		public static IBurnableItem ActiveBurnableItem { get; set; }

		
		
		public static bool IsInCaves
		{
			get
			{
				return LocalPlayer.ActiveAreaInfo && LocalPlayer.ActiveAreaInfo.IsInCaves;
			}
		}

		
		
		
		public static bool IsInEndgame { get; private set; }

		
		
		
		public static bool IsInOverlookArea { get; private set; }

		
		
		public static bool IsInOutsideWorld
		{
			get
			{
				return !LocalPlayer.IsInCaves || LocalPlayer.IsInOverlookArea;
			}
		}

		
		
		public static bool IsInClosedArea
		{
			get
			{
				return LocalPlayer.IsInCaves && !LocalPlayer.IsInOverlookArea;
			}
		}

		
		
		public static bool IsInWorld
		{
			get
			{
				return LocalPlayer.CurrentView == PlayerInventory.PlayerViews.World;
			}
		}

		
		
		public static bool IsInInventory
		{
			get
			{
				return LocalPlayer.CurrentView == PlayerInventory.PlayerViews.Inventory;
			}
		}

		
		
		public static bool IsInBook
		{
			get
			{
				return LocalPlayer.CurrentView == PlayerInventory.PlayerViews.Book;
			}
		}

		
		
		public static PlayerInventory.PlayerViews CurrentView
		{
			get
			{
				return (!LocalPlayer.Inventory) ? PlayerInventory.PlayerViews.Loading : LocalPlayer.Inventory.CurrentView;
			}
		}

		
		private void Awake()
		{
			LocalPlayer.Transform = this._transform;
			LocalPlayer.Rigidbody = this._rigidbody;
			FMOD_StudioEventEmitter.LocalPlayerTransform = LocalPlayer.Transform;
			LocalPlayer.GameObject = this._playerGO;
			LocalPlayer.PlayerBase = this._playerBase;
			LocalPlayer.HeadTr = this._headTr;
			LocalPlayer.HipsTr = this._hipsTr;
			LocalPlayer.Inventory = this._inventory;
			LocalPlayer.ReceipeBook = this._receipeBook;
			LocalPlayer.SpecialActions = this._specialActions;
			LocalPlayer.SpecialItems = this._specialItems;
			LocalPlayer.MainCamTr = ((!ForestVR.Enabled) ? this._mainCamTr : this._mainCamTrVR);
			LocalPlayer.MainCam = ((!ForestVR.Enabled) ? this._mainCam : this._mainCamVR);
			LocalPlayer.InventoryCam = ((!ForestVR.Enabled) ? this._inventoryCam : this._inventoryCamVR);
			LocalPlayer.ImageEffectOptimizer = this._imageEffectOptimizer;
			LocalPlayer.CamFollowHead = this._camFollowHead;
			LocalPlayer.Animator = this._animator;
			LocalPlayer.AnimControl = this._animControl;
			LocalPlayer.Grabber = this._grabber;
			LocalPlayer.Create = this._create;
			LocalPlayer.Tuts = this._tuts;
			LocalPlayer.Sfx = this._sfx;
			LocalPlayer.Stats = this._stats;
			LocalPlayer.Clothing = this._clothing;
			LocalPlayer.FpCharacter = this._fpc;
			LocalPlayer.FpHeadBob = this._fphb;
			LocalPlayer.CamRotator = this._camRotator;
			LocalPlayer.MainRotator = this._mainRotator;
			LocalPlayer.ScriptSetup = this._scriptSetup;
			LocalPlayer.TargetFunctions = this._targetFunctions;
			LocalPlayer.HitReactions = this._hitReactions;
			LocalPlayer.Buoyancy = this._buoyancy;
			LocalPlayer.WaterViz = this._waterViz;
			LocalPlayer.AiInfo = this._aiInfo;
			LocalPlayer.WaterEngine = this._waterEngine;
			LocalPlayer.ItemDecayMachine = this._itemDecayMachine;
			LocalPlayer.AnimatedBook = this._animatedBook;
			LocalPlayer.PassengerManifest = this._passengerManifest;
			LocalPlayer.GreebleRoot = this._greebleRoot;
			LocalPlayer.MudGreeble = this._mudGreeble;
			LocalPlayer.PlayerDeadCam = this._PlayerDeadCam;
			LocalPlayer.PauseMenuBlur = this._pauseMenuBlur;
			LocalPlayer.PauseMenuBlurPsCam = this._pauseMenuBlurPsCam;
			LocalPlayer.LeftHandTrVR = this._leftHandTrVR;
			LocalPlayer.RightHandTrVR = this._rightHandTrVR;
			LocalPlayer.InventoryPositionVR = this._inventoryPositionVR;
			LocalPlayer.HeldItemsData = this._heldItemsData;
			LocalPlayer.Vis = this._vis;
			LocalPlayer.ActiveAreaInfo = this._activeAreaInfo;
			LocalPlayer.PlayerPushSledAction = this._playerPushSledAction;
			LocalPlayer.SurfaceDetector = this._surfaceDetector;
			LocalPlayer.RepairTool = this._repairTool;
			LocalPlayer.Achievements = this._achievements;
			LocalPlayer.QuickSelectViews = this._quickSelectViews;
			LocalPlayer.MapDrawer = this._mapDrawer;
			LocalPlayer.ItemSlots = this._itemSlots;
			LocalPlayer.vrAdapter = this._vrAdapter;
			LocalPlayer.vrPlayerControl = this._vrPlayerControl;
			LocalPlayer.vrVignette = this._vrVignette;
			LocalPlayer.InventoryMouseEventsVR = this._inventoryMouseEventsVR;
			LocalPlayer.SavedData = this._savedData;
			LocalPlayer.IsInEndgame = false;
			LocalPlayer.IsInOverlookArea = false;
			base.StartCoroutine(this.OldSaveCompat());
			this.InitModding();
		}

		
		private void OnDestroy()
		{
			if (LocalPlayer.Transform == base.transform)
			{
				LocalPlayer.Transform = null;
				LocalPlayer.Rigidbody = null;
				FMOD_StudioEventEmitter.LocalPlayerTransform = null;
				LocalPlayer.GameObject = null;
				LocalPlayer.PlayerBase = null;
				LocalPlayer.HeadTr = null;
				LocalPlayer.HipsTr = null;
				LocalPlayer.Inventory = null;
				LocalPlayer.ReceipeBook = null;
				LocalPlayer.SpecialActions = null;
				LocalPlayer.SpecialItems = null;
				LocalPlayer.MainCamTr = null;
				LocalPlayer.MainCam = null;
				LocalPlayer.InventoryCam = null;
				LocalPlayer.ImageEffectOptimizer = null;
				LocalPlayer.CamFollowHead = null;
				LocalPlayer.Animator = null;
				LocalPlayer.AnimControl = null;
				LocalPlayer.Grabber = null;
				LocalPlayer.Create = null;
				LocalPlayer.Tuts = null;
				LocalPlayer.Sfx = null;
				LocalPlayer.Stats = null;
				LocalPlayer.FpCharacter = null;
				LocalPlayer.FpHeadBob = null;
				LocalPlayer.CamRotator = null;
				LocalPlayer.MainRotator = null;
				LocalPlayer.ScriptSetup = null;
				LocalPlayer.TargetFunctions = null;
				LocalPlayer.HitReactions = null;
				LocalPlayer.Buoyancy = null;
				LocalPlayer.WaterViz = null;
				LocalPlayer.AiInfo = null;
				LocalPlayer.WaterEngine = null;
				LocalPlayer.ItemDecayMachine = null;
				LocalPlayer.AnimatedBook = null;
				LocalPlayer.PassengerManifest = null;
				LocalPlayer.GreebleRoot = null;
				LocalPlayer.MudGreeble = null;
				LocalPlayer.PlayerDeadCam = null;
				LocalPlayer.PauseMenuBlur = null;
				LocalPlayer.PauseMenuBlurPsCam = null;
				LocalPlayer.LeftHandTrVR = null;
				LocalPlayer.RightHandTrVR = null;
				LocalPlayer.InventoryPositionVR = null;
				LocalPlayer.HeldItemsData = null;
				LocalPlayer.Vis = null;
				LocalPlayer.ActiveAreaInfo = null;
				LocalPlayer.PlayerPushSledAction = null;
				LocalPlayer.SurfaceDetector = null;
				LocalPlayer.RepairTool = null;
				LocalPlayer.Achievements = null;
				LocalPlayer.QuickSelectViews = null;
				LocalPlayer.MapDrawer = null;
				LocalPlayer.ItemSlots = null;
				LocalPlayer.vrAdapter = null;
				LocalPlayer.vrPlayerControl = null;
				LocalPlayer.vrVignette = null;
				LocalPlayer.InventoryMouseEventsVR = null;
				LocalPlayer.SavedData = null;
				LocalPlayer.ActiveBurnableItem = null;
				LocalPlayer.IsInEndgame = false;
				LocalPlayer.IsInOverlookArea = false;
				this.ResetModding();
			}
		}

		
		public void SetInEndGame(bool value)
		{
			LocalPlayer.IsInEndgame = value;
		}

		
		public void SetInOverlookArea(bool value)
		{
			LocalPlayer.IsInOverlookArea = value;
		}

		
		private IEnumerator OldSaveCompat()
		{
			if (!CoopPeerStarter.DedicatedHost)
			{
				yield return null;
				yield return null;
				if (LocalPlayer.PassengerManifest)
				{
					if (!LocalPlayer.PassengerManifest.gameObject.activeSelf)
					{
						LocalPlayer.PassengerManifest.gameObject.SetActive(true);
					}
					if (GameSetup.IsSavedGame)
					{
						LocalPlayer.PassengerManifest.SendMessage("OnDeserialized");
					}
				}
				if (!LocalPlayer.GameObject.GetComponent<targetStats>())
				{
					targetStats targetStats = LocalPlayer.GameObject.AddComponent<targetStats>();
					targetStats.setPlayerType = true;
				}
				if (!LocalPlayer.GameObject.GetComponent<visRangeSetup>())
				{
					visRangeSetup visRangeSetup = LocalPlayer.GameObject.AddComponent<visRangeSetup>();
					visRangeSetup.host = true;
					visRangeSetup.testDist = 32f;
					visRangeSetup.offsetFactor = 1.05f;
					this._vis = visRangeSetup;
					LocalPlayer.Vis = visRangeSetup;
				}
				while (!LocalPlayer.Inventory.enabled)
				{
					yield return null;
				}
				if (!LocalPlayer.GameObject.GetComponent<CoopVoice>())
				{
					CoopVoice coopVoice = LocalPlayer.GameObject.AddComponent<CoopVoice>();
					int id = ItemDatabase.ItemByName("WalkyTalky")._id;
					coopVoice.WalkieTalkie = LocalPlayer.Inventory.InventoryItemViewsCache[id][0]._held.GetComponent<BatteryBasedTalkyWalky>();
				}
			}
			yield break;
		}

		
		private void InitModding()
		{
			MainSceneBridge.LocalPlayer = this;
		}

		
		private void ResetModding()
		{
			MainSceneBridge.LocalPlayer = null;
		}

		
		public void UnlimitedItems(bool onoff)
		{
			LocalPlayer.Inventory.ItemFilter = ((!onoff) ? null : new InventoryItemFilter_Unlimited());
		}

		
		public bool AddItem(int itemId, int amount = 1000, bool preventAutoEquip = true)
		{
			return LocalPlayer.Inventory.AddItem(itemId, amount, preventAutoEquip, false, null);
		}

		
		public void AddItemsByType(ItemTypes typeMask, int amount = 1000, bool preventAutoEquip = true)
		{
			foreach (Item item in ItemDatabase.Items)
			{
				try
				{
					if (item._maxAmount >= 0 && item.MatchType((Item.Types)typeMask) && LocalPlayer.Inventory.InventoryItemViewsCache.ContainsKey(item._id))
					{
						LocalPlayer.Inventory.AddItem(item._id, amount, preventAutoEquip, false, null);
					}
				}
				catch (Exception ex)
				{
				}
			}
		}

		
		public void Goto(Transform target)
		{
			bool flag = Terrain.activeTerrain.SampleHeight(target.position) - target.position.y > (float)((!LocalPlayer.IsInCaves) ? 6 : 3);
			Area componentInParent = target.GetComponentInParent<Area>();
			if (!flag && !LocalPlayer.IsInCaves)
			{
				RaycastHit raycastHit;
				if (Physics.SphereCast(target.transform.position + Vector3.up * 25f + Vector3.forward * 2f, 2f, Vector3.down, out raycastHit, 60f))
				{
					this.GotoCave(flag);
					this.GotoArea(componentInParent);
					LocalPlayer.Rigidbody.velocity = Vector3.zero;
					LocalPlayer.Transform.position = raycastHit.point + Vector3.up * 2.5f;
					Debug.Log("$> going to " + target);
				}
				else
				{
					Debug.Log("# didn't find a suitable landing spot raycasting down on '" + target + "', cancelling goto");
				}
			}
			else
			{
				this.GotoCave(flag);
				this.GotoArea(componentInParent);
				LocalPlayer.Rigidbody.velocity = Vector3.zero;
				LocalPlayer.Transform.position = target.transform.position + Vector3.up;
				GameObject gameObject = GameObject.FindWithTag("EndgameLoader");
				if (gameObject)
				{
					SceneLoadTrigger component = gameObject.GetComponent<SceneLoadTrigger>();
					Vector3 rhs = LocalPlayer.Transform.position - component.transform.position;
					if (Vector3.Dot(component.transform.forward, rhs) > 0f && rhs.magnitude < 150f)
					{
						component._onCrossingForwards.Invoke();
					}
				}
				Debug.Log("# going to " + target);
			}
		}

		
		public void Goto(Vector3 position)
		{
			bool inCave = Terrain.activeTerrain.SampleHeight(position) - position.y > (float)((!LocalPlayer.IsInCaves) ? 6 : 3);
			this.GotoCave(inCave);
			LocalPlayer.Rigidbody.velocity = Vector3.zero;
			LocalPlayer.Transform.position = position;
			Debug.Log("# going to " + position);
		}

		
		private void GotoCave(bool inCave)
		{
			if (inCave && !LocalPlayer.IsInCaves)
			{
				Debug.Log("# goto -> entering cave");
				LocalPlayer.GameObject.SendMessage("InACave");
			}
			else if (!inCave && LocalPlayer.IsInCaves)
			{
				Debug.Log("# goto -> exiting cave");
				LocalPlayer.GameObject.SendMessage("NotInACave");
			}
		}

		
		private void GotoArea(Area area)
		{
			if (area)
			{
				area.OnEnter(null);
			}
		}

		
		[Header("Core")]
		public Transform _transform;

		
		public Rigidbody _rigidbody;

		
		public GameObject _playerGO;

		
		public GameObject _playerBase;

		
		public Transform _headTr;

		
		public Transform _hipsTr;

		
		public PlayerInventory _inventory;

		
		public ReceipeBook _receipeBook;

		
		public Grabber _grabber;

		
		public Create _create;

		
		public PlayerTuts _tuts;

		
		public PlayerSfx _sfx;

		
		public PlayerStats _stats;

		
		public PlayerClothing _clothing;

		
		public GameObject _specialActions;

		
		public GameObject _specialItems;

		
		public LocalPlayer.SavedDataFields _savedData;

		
		[Header("Cameras / Visuals / VR")]
		public Transform _mainCamTr;

		
		public Transform _mainCamTrVR;

		
		public Camera _mainCam;

		
		public Camera _mainCamVR;

		
		public Camera _inventoryCam;

		
		public Camera _inventoryCamVR;

		
		public ImageEffectOptimizer _imageEffectOptimizer;

		
		public camFollowHead _camFollowHead;

		
		public SimpleMouseRotator _camRotator;

		
		public GameObject _PlayerDeadCam;

		
		public Blur _pauseMenuBlur;

		
		public Blur _pauseMenuBlurPsCam;

		
		public Transform _leftHandTrVR;

		
		public Transform _rightHandTrVR;

		
		public Transform _inventoryPositionVR;

		
		public VRPlayerAdapter _vrAdapter;

		
		public VRPlayerControl _vrPlayerControl;

		
		public VRVignetteController _vrVignette;

		
		public CameraMouseEvents _inventoryMouseEventsVR;

		
		[Header("Animation / AI Targeting")]
		public Animator _animator;

		
		public playerAnimatorControl _animControl;

		
		public FirstPersonCharacter _fpc;

		
		public FirstPersonHeadBob _fphb;

		
		public playerScriptSetup _scriptSetup;

		
		public playerTargetFunctions _targetFunctions;

		
		public playerHitReactions _hitReactions;

		
		public SimpleMouseRotator _mainRotator;

		
		public SkinnedMeshRenderer _animatedBook;

		
		public visRangeSetup _vis;

		
		public playerAiInfo _aiInfo;

		
		public PlayerPushSledAction _playerPushSledAction;

		
		[Header("Unsorted")]
		public Buoyancy _buoyancy;

		
		public WaterViz _waterViz;

		
		public WaterEngine _waterEngine;

		
		public ItemDecayMachine _itemDecayMachine;

		
		public PassengerManifest _passengerManifest;

		
		public GameObject _greebleRoot;

		
		public GreebleLayer _mudGreeble;

		
		public HeldItemsData _heldItemsData;

		
		public ActiveAreaInfo _activeAreaInfo;

		
		public UnderfootSurfaceDetector _surfaceDetector;

		
		public RepairTool _repairTool;

		
		public AchievementsManager _achievements;

		
		public QuickSelectViews[] _quickSelectViews;

		
		public CaveMapDrawer _mapDrawer;

		
		public itemConstrainToHand[] _itemSlots;

		
		public static Transform Transform;

		
		public static Rigidbody Rigidbody;

		
		public static GameObject GameObject;

		
		public static GameObject PlayerBase;

		
		public static Transform HeadTr;

		
		public static Transform HipsTr;

		
		public static PlayerInventory Inventory;

		
		public static ReceipeBook ReceipeBook;

		
		public static GameObject SpecialActions;

		
		public static GameObject SpecialItems;

		
		public static Transform MainCamTr;

		
		public static Camera MainCam;

		
		public static Camera InventoryCam;

		
		public static ImageEffectOptimizer ImageEffectOptimizer;

		
		public static camFollowHead CamFollowHead;

		
		public static Animator Animator;

		
		public static playerAnimatorControl AnimControl;

		
		public static Grabber Grabber;

		
		public static Create Create;

		
		public static PlayerTuts Tuts;

		
		public static PlayerSfx Sfx;

		
		public static PlayerStats Stats;

		
		public static PlayerClothing Clothing;

		
		public static FirstPersonCharacter FpCharacter;

		
		public static FirstPersonHeadBob FpHeadBob;

		
		public static SimpleMouseRotator CamRotator;

		
		public static SimpleMouseRotator MainRotator;

		
		public static playerScriptSetup ScriptSetup;

		
		public static playerTargetFunctions TargetFunctions;

		
		public static playerHitReactions HitReactions;

		
		public static Buoyancy Buoyancy;

		
		public static BoltEntity Entity;

		
		public static IPlayerState State;

		
		public static WaterViz WaterViz;

		
		public static playerAiInfo AiInfo;

		
		public static WaterEngine WaterEngine;

		
		public static ItemDecayMachine ItemDecayMachine;

		
		public static SkinnedMeshRenderer AnimatedBook;

		
		public static PassengerManifest PassengerManifest;

		
		public static GameObject GreebleRoot;

		
		public static GreebleLayer MudGreeble;

		
		public static GameObject PlayerDeadCam;

		
		public static Blur PauseMenuBlur;

		
		public static Blur PauseMenuBlurPsCam;

		
		public static Transform LeftHandTrVR;

		
		public static Transform RightHandTrVR;

		
		public static Transform InventoryPositionVR;

		
		public static HeldItemsData HeldItemsData;

		
		public static visRangeSetup Vis;

		
		public static ActiveAreaInfo ActiveAreaInfo;

		
		public static PlayerPushSledAction PlayerPushSledAction;

		
		public static UnderfootSurfaceDetector SurfaceDetector;

		
		public static RepairTool RepairTool;

		
		public static AchievementsManager Achievements;

		
		public static QuickSelectViews[] QuickSelectViews;

		
		public static CaveMapDrawer MapDrawer;

		
		public static itemConstrainToHand[] ItemSlots;

		
		public static VRPlayerAdapter vrAdapter;

		
		public static VRPlayerControl vrPlayerControl;

		
		public static VRVignetteController vrVignette;

		
		public static CameraMouseEvents InventoryMouseEventsVR;

		
		public static LocalPlayer.SavedDataFields SavedData;

		
		[Serializable]
		public class SavedDataFields
		{
			
			public BoolData ReachedLowSanityThreshold;

			
			public BoolData ExitedEndgame;
		}
	}
}
