using System;
using ModAPI;
using TheForest.Utils;
using TheForest.World;
using UltimateCheatmenu;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class BuildingHealthHitRelay : MonoBehaviour
	{
		
		public void __LocalizedHit__Original(LocalizedHitData data)
		{
			BuildingHealth buildingHealth = this.GetBuildingHealth();
			if (buildingHealth)
			{
				buildingHealth.LocalizedHit(data);
			}
		}

		
		public void OnExplode(Explode.Data explodeData)
		{
			BuildingExplosion componentInParent = base.transform.GetComponentInParent<BuildingExplosion>();
			if (componentInParent && !componentInParent.Exploding)
			{
				Scene.ActiveMB.StartCoroutine(componentInParent.OnExplode(explodeData));
			}
		}

		
		public BuildingHealth GetBuildingHealth()
		{
			PrefabIdentifier componentInParent = base.transform.GetComponentInParent<PrefabIdentifier>();
			return (!componentInParent) ? null : componentInParent.GetComponent<BuildingHealth>();
		}

		
		public void LocalizedHit(LocalizedHitData data)
		{
			try
			{
				this.__LocalizedHit__Original(DestroyBuildings.GetLocalizedHitData(data));
			}
			catch (Exception ex)
			{
				Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
				this.__LocalizedHit__Original(data);
			}
		}
	}
}
