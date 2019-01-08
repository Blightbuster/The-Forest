using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Buildings.World
{
	public class RoofBasic : MonoBehaviour, IRoof
	{
		private void OnEnable()
		{
			this._polygon = this.CalculatePolygon();
			InsideCheck.AddRoof(this);
		}

		private void OnDisable()
		{
			InsideCheck.RemoveRoof(this);
		}

		public float GetLevel()
		{
			return base.transform.TransformPoint(this._center).y;
		}

		public List<Vector3> GetPolygon()
		{
			return this._polygon;
		}

		private List<Vector3> CalculatePolygon()
		{
			List<Vector3> list = new List<Vector3>(this._points.Length);
			for (int i = 0; i < this._points.Length; i++)
			{
				Vector3 position = new Vector3(this._points[i].x, 0f, this._points[i].y) + this._center;
				list.Add(base.transform.TransformPoint(position));
			}
			return list;
		}

		public Vector3 _center;

		public Vector2[] _points;

		private List<Vector3> _polygon;
	}
}
