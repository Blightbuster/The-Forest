using System;
using TheForest.World;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class HitRelayOnContactWithTag : MonoBehaviour
	{
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag(this._tag))
			{
				BuildingHealth componentInParent = base.transform.GetComponentInParent<BuildingHealth>();
				if (componentInParent)
				{
					componentInParent.LocalizedHit(new LocalizedHitData
					{
						_position = other.transform.position,
						_damage = (float)this._damage
					});
				}
			}
		}

		
		public string _tag = "enemyCollide";

		
		public int _damage;
	}
}
