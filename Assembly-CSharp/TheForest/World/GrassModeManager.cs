using System;
using AdvancedTerrainGrass;
using UnityEngine;

namespace TheForest.World
{
	public class GrassModeManager : MonoBehaviour
	{
		public static GrassModeManager GetInstance()
		{
			return GrassModeManager._instance;
		}

		private void Awake()
		{
			if (GrassModeManager._instance != null && GrassModeManager._instance != this)
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			GrassModeManager._instance = this;
			this.ActiveMode = TheForestQualitySettings.GrassModes.CPU;
			TheForestQualitySettings.GrassModes activeMode = this.ActiveMode;
			if (activeMode != TheForestQualitySettings.GrassModes.CPU)
			{
				if (activeMode == TheForestQualitySettings.GrassModes.GPU)
				{
					this.InitializeGpu();
				}
			}
			else
			{
				this.InitializeCpu();
			}
			this._initFrame = Time.renderedFrameCount;
		}

		private void Update()
		{
			this.CheckFirstRefresh();
		}

		private void CheckFirstRefresh()
		{
			if (this._completedFirstRefresh || Time.renderedFrameCount < this._initFrame + this.InitRefreshDelayFrames)
			{
				return;
			}
			this._completedFirstRefresh = true;
			this.RefreshSettings();
		}

		private void OnDestroy()
		{
			if (GrassModeManager._instance == this)
			{
				GrassModeManager._instance = null;
			}
		}

		private void InitializeCpu()
		{
			this.TerrainSource.drawTreesAndFoliage = true;
		}

		private void InitializeGpu()
		{
			this.ATGrassManager.SetInitializationDensityValue(TheForestQualitySettings.UserSettings.GrassDensity);
			this.ATGrassManager.SetInitializationDistanceValue(210f);
			this.ATGrassManager.enabled = true;
		}

		public void RefreshSettings()
		{
			this.RefreshSettings(TheForestQualitySettings.UserSettings.GrassDensity, TheForestQualitySettings.UserSettings.GrassDistance);
		}

		public void RefreshSettings(float density, float distance)
		{
			if (!this.SettingsChanged(density, distance))
			{
				return;
			}
			this._currentDensity = density;
			this._currentDistance = distance;
			TheForestQualitySettings.GrassModes activeMode = this.ActiveMode;
			if (activeMode != TheForestQualitySettings.GrassModes.CPU)
			{
				if (activeMode == TheForestQualitySettings.GrassModes.GPU)
				{
					this.RefreshGpuSettings();
				}
			}
			else
			{
				this.RefreshCpuSettings();
			}
		}

		private void RefreshCpuSettings()
		{
			this.TerrainSource.detailObjectDistance = this._currentDistance;
			this.TerrainSource.detailObjectDensity = this._currentDensity;
		}

		private void RefreshGpuSettings()
		{
			this.ATGrassManager.CullDistance = this._currentDistance * this.GPUDistanceMultiplier;
			this.ATGrassManager.DetailDensity = this._currentDensity;
			this.ATGrassManager.RefreshGrassRenderingSettings(this._currentDensity, this._currentDistance * this.GPUDistanceMultiplier);
		}

		private bool SettingsChanged(float density, float distance)
		{
			return !Mathf.Approximately(this._currentDensity, density) || !Mathf.Approximately(this._currentDistance, distance);
		}

		public Terrain TerrainSource;

		public GrassManager ATGrassManager;

		public TheForestQualitySettings.GrassModes ActiveMode;

		public float GPUDistanceMultiplier = 1.3f;

		private static GrassModeManager _instance;

		private float _currentDensity = -1f;

		private float _currentDistance = -1f;

		private int _initFrame;

		private bool _completedFirstRefresh;

		private int InitRefreshDelayFrames = 10;
	}
}
