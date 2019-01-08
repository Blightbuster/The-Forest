using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class SpawnParticles : MonoBehaviour
	{
		public void SetTargetPosition(Vector3 position)
		{
			this._targetPosition = position;
		}

		public void DoSpawn()
		{
			Prefabs.Instance.SpawnHitPS(this._particleType, this._targetPosition, Quaternion.LookRotation(this._targetPosition - base.transform.position));
		}

		public HitParticles _particleType;

		private Vector3 _targetPosition;
	}
}
