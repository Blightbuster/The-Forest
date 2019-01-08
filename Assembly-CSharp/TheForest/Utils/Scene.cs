using System;
using Ceto;
using TheForest.Modding.Bridge;
using TheForest.World;
using UnityEngine;

namespace TheForest.Utils
{
	public class Scene : MonoBehaviour
	{
		private void Awake()
		{
			Scene.ActiveMB = this;
			Scene.GameStats = this._gameStats;
			Scene.SceneTracker = this._sceneTracker;
			Scene.MutantControler = this._mutantControler;
			Scene.MutantSpawnManager = this._mutantSpawnManager;
			Scene.Yacht = this._yacht;
			Scene.PlaneCrash = this._plane;
			Scene.PlaneCrashAnimGO = this._planeCrashAnimGO;
			Scene.TriggerCutScene = this._triggerCutScene;
			Scene.RainFollowGO = this._rainFollowGO;
			Scene.RainTypes = this._rainTypes;
			Scene.WeatherSystem = this._weatherSystem;
			Scene.HudGui = this._hudGui;
			Scene.Cams = this._cams;
			Scene.Clock = this._clock;
			Scene.Atmosphere = this._atmos;
			Scene.WorkScheduler = this._workScheduler;
			Scene.LoadSave = this._loadSave;
			Scene.PlaneGreebles = this._planeGreebles;
			Scene.OceanFlat = this._oceanFlat;
			Scene.OceanCeto = this._oceanCeto;
			Scene.ShoreMask = this._shoreMask;
			Scene.SinkHoleCenter = this._sinkHoleCenter;
			Scene.GreebleZonesManager = this._greebleZonesManager;
			Scene.CaveGrounds = this._caveGrounds;
			Scene.SceneLoaders = this._sceneLoaders;
			Scene.mecanimEvents = this._mecanimEvents;
			Scene.animalSpawner = this._animalSpawner;
			Scene.animHash = this._animHash;
			this.InitModding();
		}

		private void OnDestroy()
		{
			if (Scene.ActiveMB == this)
			{
				Scene.ActiveMB = null;
				Scene.GameStats = null;
				Scene.SceneTracker = null;
				Scene.MutantControler = null;
				Scene.MutantSpawnManager = null;
				Scene.Yacht = null;
				Scene.PlaneCrash = null;
				Scene.PlaneCrashAnimGO = null;
				Scene.TriggerCutScene = null;
				Scene.RainFollowGO = null;
				Scene.RainTypes = null;
				Scene.WeatherSystem = null;
				Scene.HudGui = null;
				Scene.Cams = null;
				Scene.Clock = null;
				Scene.Atmosphere = null;
				Scene.WorkScheduler = null;
				Scene.LoadSave = null;
				Scene.PlaneGreebles = null;
				Scene.OceanFlat = null;
				Scene.OceanCeto = null;
				Scene.ShoreMask = null;
				Scene.SinkHoleCenter = null;
				Scene.GreebleZonesManager = null;
				Scene.CaveGrounds = null;
				Scene.SceneLoaders = null;
				Scene.mecanimEvents = null;
				Scene.animalSpawner = null;
				Scene.animHash = null;
			}
			this.ResetModding();
		}

		public static bool IsInSinkhole(Vector3 position)
		{
			return Vector3.Distance(new Vector3(Scene.SinkHoleCenter.position.x, position.y, Scene.SinkHoleCenter.position.z), position) < 190f;
		}

		public static int ValidateFloorLayers(Vector3 position, int layers)
		{
			return (!Scene.IsInSinkhole(position)) ? layers : (layers ^ 67108864);
		}

		private void InitModding()
		{
			MainSceneBridge.UI.ModTimer = Scene.HudGui.ModTimer;
			MainSceneBridge.PlaneCrashController = Scene.PlaneCrash;
			MainSceneBridge.WorkScheduler = Scene.WorkScheduler;
			MainSceneBridge.MutantController = Scene.MutantControler;
		}

		private void ResetModding()
		{
			MainSceneBridge.UI.ModTimer = null;
			MainSceneBridge.PlaneCrashController = null;
			MainSceneBridge.WorkScheduler = null;
			MainSceneBridge.MutantController = null;
		}

		public GameStats _gameStats;

		public sceneTracker _sceneTracker;

		public mutantController _mutantControler;

		public mutantSpawnManager _mutantSpawnManager;

		public GameObject _yacht;

		public PlaneCrashController _plane;

		public GameObject _planeCrashAnimGO;

		public TriggerCutScene _triggerCutScene;

		public GameObject _rainFollowGO;

		public RainTypes _rainTypes;

		public WeatherSystem _weatherSystem;

		public HudGui _hudGui;

		public CamController _cams;

		public Clock _clock;

		public TheForestAtmosphere _atmos;

		public WorkScheduler _workScheduler;

		public LoadSave _loadSave;

		public GameObject _planeGreebles;

		public GameObject _oceanFlat;

		public GameObject _oceanCeto;

		public AddShoreMask _shoreMask;

		public Transform _sinkHoleCenter;

		public GreebleZonesManager _greebleZonesManager;

		public GameObject[] _caveGrounds;

		public SceneUnloadInCave[] _sceneLoaders;

		public GameObject _mecanimEvents;

		public GameObject _animalSpawner;

		public animatorHashLookup _animHash;

		public static MonoBehaviour ActiveMB;

		public static GameStats GameStats;

		public static sceneTracker SceneTracker;

		public static mutantController MutantControler;

		public static mutantSpawnManager MutantSpawnManager;

		public static GameObject Yacht;

		public static PlaneCrashController PlaneCrash;

		public static GameObject PlaneCrashAnimGO;

		public static TriggerCutScene TriggerCutScene;

		public static GameObject RainFollowGO;

		public static RainTypes RainTypes;

		public static WeatherSystem WeatherSystem;

		public static HudGui HudGui;

		public static CamController Cams;

		public static Clock Clock;

		public static TheForestAtmosphere Atmosphere;

		public static WorkScheduler WorkScheduler;

		public static LoadSave LoadSave;

		public static GameObject PlaneGreebles;

		public static GameObject OceanFlat;

		public static GameObject OceanCeto;

		public static AddShoreMask ShoreMask;

		public static Transform SinkHoleCenter;

		public static GreebleZonesManager GreebleZonesManager;

		public static GameObject[] CaveGrounds;

		public static SceneUnloadInCave[] SceneLoaders;

		public static GameObject mecanimEvents;

		public static GameObject animalSpawner;

		public static bool FinishGameLoad;

		public static animatorHashLookup animHash;

		internal static readonly object MutantSpawnManagerdesiredSkinnyPale;
	}
}
