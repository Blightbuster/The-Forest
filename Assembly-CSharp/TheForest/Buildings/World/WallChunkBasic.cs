using System;
using UnityEngine;

namespace TheForest.Buildings.World
{
	public class WallChunkBasic : MonoBehaviour
	{
		private void OnEnable()
		{
			Vector3 start;
			Vector3 end;
			Vector3 vector;
			this.CalculatePoints(out start, out end, out vector);
			this._gridToken = InsideCheck.AddWallChunk(start, end, vector.y - start.y);
		}

		private void OnDisable()
		{
			if (this._gridToken >= 0)
			{
				InsideCheck.RemoveWallChunk(this._gridToken);
			}
		}

		private void CalculatePoints(out Vector3 start, out Vector3 end, out Vector3 top)
		{
			WallChunkBasic.Axis axis = this._axis;
			if (axis != WallChunkBasic.Axis.X)
			{
				if (axis != WallChunkBasic.Axis.Z)
				{
					start = Vector3.zero;
					end = Vector3.zero;
				}
				else
				{
					start = this._center - new Vector3(0f, this._height / 2f, this._length / 2f);
					end = start;
					end.z = start.z + this._length;
				}
			}
			else
			{
				start = this._center - new Vector3(this._length / 2f, this._height / 2f, 0f);
				end = start;
				end.x = start.x + this._length;
			}
			top = start;
			top.y = start.y + this._height;
			start = base.transform.TransformPoint(start);
			end = base.transform.TransformPoint(end);
			top = base.transform.TransformPoint(top);
		}

		public WallChunkBasic.Axis _axis;

		public Vector3 _center;

		public float _length;

		public float _height;

		private int _gridToken;

		public enum Axis
		{
			X,
			Z
		}
	}
}
