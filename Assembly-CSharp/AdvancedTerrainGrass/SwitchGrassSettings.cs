using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTerrainGrass
{
	public class SwitchGrassSettings : MonoBehaviour
	{
		private void OnLevelWasLoaded()
		{
			this.ReloadTerrain();
		}

		private void ReloadTerrain()
		{
			this.ters = Terrain.activeTerrains;
			int num = this.ters.Length;
			for (int i = 0; i < num; i++)
			{
				GrassManager component = this.ters[i].GetComponent<GrassManager>();
				if (component != null)
				{
					this.GrassManagers.Add(component);
					this.ActiveGrassManagers++;
				}
			}
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.Alpha1))
			{
				for (int i = 0; i < this.ActiveGrassManagers; i++)
				{
					this.GrassManagers[i].RefreshGrassRenderingSettings(this.GrassManagers[i].DetailDensity, this.GrassManagers[i].CullDistance, this.GrassManagers[i].FadeLength, this.GrassManagers[i].CacheDistance, this.GrassManagers[i].DetailFadeStart, this.GrassManagers[i].DetailFadeLength, this.GrassManagers[i].ShadowStart, this.GrassManagers[i].ShadowFadeLength, this.GrassManagers[i].ShadowStartFoliage, this.GrassManagers[i].ShadowStartFoliage);
				}
			}
			if (Input.GetKey(KeyCode.Alpha2))
			{
				for (int j = 0; j < this.ActiveGrassManagers; j++)
				{
					this.GrassManagers[j].RefreshGrassRenderingSettings((!this.doForceDetailDensity) ? this.GrassManagers[j].DetailDensity : this.forcedDetailDensity, (!this.doForceCullDistance) ? this.GrassManagers[j].CullDistance : this.forcedCullDistance, (!this.doForceFadeLength) ? this.GrassManagers[j].FadeLength : this.forcedFadeLength, (!this.doForceCacheDistance) ? this.GrassManagers[j].CacheDistance : this.forcedCacheDistance, (!this.doForceDetailFadeStart) ? this.GrassManagers[j].DetailFadeStart : this.forcedDetailFadeStart, (!this.doForceDetailFadeLength) ? this.GrassManagers[j].DetailFadeLength : this.forcedDetailFadeLength, this.GrassManagers[j].ShadowStart, this.GrassManagers[j].ShadowFadeLength, this.GrassManagers[j].ShadowStartFoliage, this.GrassManagers[j].ShadowStartFoliage);
				}
			}
		}

		public List<GrassManager> GrassManagers;

		public int ActiveGrassManagers;

		public Terrain[] ters;

		public bool doForceDetailDensity;

		public float forcedDetailDensity = 0.2f;

		public bool doForceCullDistance;

		public float forcedCullDistance = 200f;

		public bool doForceFadeLength;

		public float forcedFadeLength = 80f;

		public bool doForceCacheDistance;

		public float forcedCacheDistance = 300f;

		public bool doForceDetailFadeStart;

		public float forcedDetailFadeStart = 40f;

		public bool doForceDetailFadeLength;

		public float forcedDetailFadeLength = 40f;
	}
}
