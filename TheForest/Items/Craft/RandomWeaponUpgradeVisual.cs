using System;
using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;

namespace TheForest.Items.Craft
{
	
	public class RandomWeaponUpgradeVisual : MonoBehaviour
	{
		
		public void OnEnable()
		{
			if (!this._hasAutoVisual)
			{
				this._hasAutoVisual = true;
				int num = 0;
				int num2 = this._autoVisualCount * 5;
				int num3 = 0;
				while (num3 < num2 && num < this._autoVisualCount)
				{
					if (this.PlaceVisual())
					{
						num++;
					}
					num3++;
				}
			}
		}

		
		private void OnDisable()
		{
			if (!base.enabled && this._hasAutoVisual && !MenuMain.exitingToMenu)
			{
				this.ClearVisuals();
			}
		}

		
		public bool PlaceVisual()
		{
			if (this._visuals == null)
			{
				this._visuals = new List<Transform>();
			}
			Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
			Vector3 origin = this._castReferal.transform.TransformPoint(this._castReferal.center) + onUnitSphere;
			Vector3 direction = -onUnitSphere;
			direction.x *= UnityEngine.Random.Range(0.9f, 1.1f);
			direction.y *= UnityEngine.Random.Range(0.9f, 1.1f);
			direction.z *= UnityEngine.Random.Range(0.9f, 1.1f);
			RaycastHit raycastHit;
			if (this._target.Raycast(new Ray(origin, direction), out raycastHit, 100f))
			{
				Transform transform = PoolManager.Pools["misc"].Spawn(this._prefab, raycastHit.point, Quaternion.LookRotation(raycastHit.normal), this._target.transform);
				this._visuals.Add(transform);
				foreach (Transform parent in this._proxies)
				{
					Transform transform2 = PoolManager.Pools["misc"].Spawn(this._prefab);
					transform2.parent = parent;
					transform2.localPosition = transform.localPosition;
					transform2.localRotation = transform.localRotation;
					this._visuals.Add(transform2);
				}
				return true;
			}
			return false;
		}

		
		public void ClearVisuals()
		{
			if (this._visuals != null)
			{
				for (int i = 0; i < this._visuals.Count; i++)
				{
					if (this._visuals[i])
					{
						PoolManager.Pools["misc"].Despawn(this._visuals[i]);
					}
				}
				this._visuals.Clear();
			}
			this._hasAutoVisual = false;
		}

		
		public Transform _prefab;

		
		public MeshCollider _target;

		
		public SphereCollider _castReferal;

		
		public Transform[] _proxies;

		
		public int _autoVisualCount = 6;

		
		private List<Transform> _visuals;

		
		private bool _hasAutoVisual;
	}
}
