using System;
using ModAPI;
using TheForest.Utils;
using TheForest.World;
using UltimateCheatmenu;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class BuildingHealthChunkHitRelay : MonoBehaviour
	{
		
		public void __LocalizedHit__Original(LocalizedHitData data)
		{
			BuildingHealthChunk componentInParent = base.transform.GetComponentInParent<BuildingHealthChunk>();
			if (componentInParent)
			{
				componentInParent.LocalizedHit(data);
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

		
		
		public BuildingHealth BuildingHealth
		{
			get
			{
				BuildingHealthChunk componentInParent = base.transform.GetComponentInParent<BuildingHealthChunk>();
				return (!componentInParent) ? null : componentInParent.BuildingHealth;
			}
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
