using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[Serializable]
	public class SpherecastAnchoring
	{
		
		public void CastForAnchors<T>(Action<T> onAnchorFound)
		{
			float maxDistance = Vector3.Distance(LocalPlayer.MainCamTr.position, LocalPlayer.Create.BuildingPlacer.transform.position) * this._castDistanceRatio - this._anchorCastRadius * 2f;
			if (Physics.SphereCast(LocalPlayer.MainCamTr.position, this._anchorCastRadius, LocalPlayer.MainCamTr.forward, out this._hit, maxDistance, this._anchorLayers))
			{
				T t = (!this._searchInParent) ? this._hit.collider.GetComponent<T>() : this._hit.collider.GetComponentInParent<T>();
				if (t != null)
				{
					onAnchorFound(t);
					return;
				}
			}
			onAnchorFound(default(T));
		}

		
		
		public RaycastHit LastHit
		{
			get
			{
				return this._hit;
			}
		}

		
		public LayerMask _anchorLayers;

		
		public float _anchorCastRadius = 1.25f;

		
		public bool _searchInParent;

		
		public float _castDistanceRatio = 1.2f;

		
		private RaycastHit _hit;
	}
}
