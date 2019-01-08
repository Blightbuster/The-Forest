using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	public class VirtualCursorSnapNodeGrid : VirtualCursorSnapNode
	{
		public override void Refresh()
		{
			Renderer component = base.GetComponent<Renderer>();
			Camera camera = (this._layer as VirtualCursorSnapLayerGrid)._camera;
			this._vpMin = camera.WorldToViewportPoint(component.bounds.min);
			this._vpMax = camera.WorldToViewportPoint(component.bounds.max);
			VirtualCursorSnapLayerGrid virtualCursorSnapLayerGrid = this._layer as VirtualCursorSnapLayerGrid;
			int num = Screen.height / virtualCursorSnapLayerGrid._hCells;
			int num2 = Screen.width / num;
			int hCells = virtualCursorSnapLayerGrid._hCells;
			int num3 = Mathf.RoundToInt(this._vpMin.x * (float)num2);
			int num4 = Mathf.RoundToInt(this._vpMax.x * (float)num2);
			int num5 = Mathf.RoundToInt(this._vpMin.y * (float)hCells);
			int num6 = Mathf.RoundToInt(this._vpMax.y * (float)hCells);
			Collider thisC = base.GetComponent<Collider>();
			if (thisC is MeshCollider)
			{
				(thisC as MeshCollider).convex = true;
			}
			this._voxels = new List<Vector2>();
			for (int k = num3; k <= num4; k++)
			{
				float x = ((float)k + 0.5f) / (float)num2;
				for (int j = num5; j <= num6; j++)
				{
					float y = ((float)j + 0.5f) / (float)hCells;
					Ray ray = new Ray(camera.transform.position, camera.ViewportToWorldPoint(new Vector3(x, y, virtualCursorSnapLayerGrid._gridDepth)) - camera.transform.position);
					RaycastHit[] source = Physics.SphereCastAll(ray, 0.05f, 6f, this._layers);
					if (source.Any((RaycastHit i) => i.collider == thisC))
					{
						this._voxels.Add(new Vector2(x, y));
						Debug.DrawLine(ray.origin, ray.origin + ray.direction * 6f, Color.green, 1f, true);
					}
					else
					{
						Debug.DrawLine(ray.origin, ray.origin + ray.direction * 6f, Color.red, 1f, true);
					}
				}
			}
			if (thisC is MeshCollider)
			{
				(thisC as MeshCollider).convex = false;
			}
		}

		public Vector3 _vpMin;

		public Vector3 _vpMax;

		public List<Vector2> _voxels;
	}
}
