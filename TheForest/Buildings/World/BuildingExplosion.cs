using System;
using System.Collections;
using Bolt;
using TheForest.Utils.Enums;
using TheForest.World;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class BuildingExplosion : EntityBehaviour
	{
		
		public void UnlocalizedExplode()
		{
			base.SendMessage("lookAtExplosion", base.transform.position + Vector3.down);
		}

		
		public IEnumerator OnExplode(Explode.Data explodeData)
		{
			if (!this._exploding && !PlayerPreferences.NoDestruction)
			{
				this._exploding = true;
				if (!BoltNetwork.isClient)
				{
					BuildingHealth component = base.GetComponent<BuildingHealth>();
					if (component)
					{
						component.LocalizedHit(new LocalizedHitData
						{
							_damage = explodeData.explode.damage * this.GetDamageRatio(explodeData.distance, explodeData.explode.radius),
							_position = explodeData.explode.transform.position,
							_distortRatio = 1.35f
						});
					}
				}
				else
				{
					FoundationHealth component2 = base.GetComponent<FoundationHealth>();
					if (component2)
					{
						component2.Distort(new LocalizedHitData
						{
							_damage = explodeData.explode.damage * this.GetDamageRatio(explodeData.distance, explodeData.explode.radius),
							_position = explodeData.explode.transform.position,
							_distortRatio = 2.5f
						});
					}
				}
				yield return null;
				this._exploding = false;
			}
			yield break;
		}

		
		public void OnExplodeFoundationTier(Explode.Data explodeData, FoundationChunkTier tier)
		{
			FoundationHealth component = base.GetComponent<FoundationHealth>();
			if (component)
			{
				component.LocalizedTierHit(new LocalizedHitData
				{
					_damage = explodeData.explode.damage * this.GetDamageRatio(explodeData.distance, explodeData.explode.radius),
					_position = explodeData.explode.transform.position,
					_distortRatio = 2.5f
				}, tier);
			}
		}

		
		private float GetDamageRatio(float distanceWithExplosion, float radius)
		{
			return 1f - Mathf.Clamp01((distanceWithExplosion - 2f) / radius);
		}

		
		private Transform[] GetTransforms()
		{
			if (this._selectionMode == BuildingExplosion.SelectionModes.StructureTag || this._selectionMode == BuildingExplosion.SelectionModes.Both)
			{
				return base.transform.GetComponentsInChildren<Transform>();
			}
			return (from r in base.transform.GetComponentsInChildren<Renderer>()
			select r.transform).ToArray<Transform>();
		}

		
		
		public bool Exploding
		{
			get
			{
				return this._exploding;
			}
		}

		
		public BuildingExplosion.SelectionModes _selectionMode;

		
		public bool _destroyThis;

		
		public CapsuleDirections _capsuleDirection = CapsuleDirections.Z;

		
		private bool _exploding;

		
		public enum SelectionModes
		{
			
			Renderer,
			
			StructureTag,
			
			Both
		}
	}
}
