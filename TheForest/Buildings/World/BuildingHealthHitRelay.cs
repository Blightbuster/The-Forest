using System;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class BuildingHealthHitRelay : MonoBehaviour
	{
		
		public void LocalizedHit(LocalizedHitData data)
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
	}
}
