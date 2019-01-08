using System;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[DoNotSerializePublic]
	public class SmallWallArchitect : WallArchitect
	{
		protected override Transform SpawnEdge(WallArchitect.Edge edge)
		{
			Transform transform = new GameObject("SmallWallEdge").transform;
			transform.transform.position = edge._p1;
			Vector3 vector = edge._p2 - edge._p1;
			if (this._autofillmode || Vector3.Distance(edge._p1, edge._p2) > this._logWidth * 3.5f)
			{
				Vector3 b = new Vector3(0f, this._logWidth * 0.95f, 0f);
				for (int i = 0; i < edge._segments.Length; i++)
				{
					WallArchitect.HorizontalSegment horizontalSegment = edge._segments[i];
					Quaternion rotation = Quaternion.LookRotation(horizontalSegment._axis);
					Vector3 vector2 = horizontalSegment._p1;
					Transform transform2 = new GameObject("Segment" + i).transform;
					transform2.parent = transform;
					transform2.LookAt(horizontalSegment._axis);
					horizontalSegment._root = transform2;
					transform2.position = horizontalSegment._p1;
					Vector3 localScale = new Vector3(1f, 1f, horizontalSegment._length / this._logLength);
					Vector3 vector3 = new Vector3(1f, 1f, 0.31f + (localScale.z - 1f) / 2f);
					float num = 1f - vector3.z / localScale.z;
					for (int j = 0; j < 2; j++)
					{
						Transform transform3 = base.NewLog(vector2, rotation);
						transform3.parent = transform2;
						this._newPool.Enqueue(transform3);
						transform3.localScale = localScale;
						vector2 += b;
					}
				}
			}
			else
			{
				Vector3 normalized = Vector3.Scale(vector, new Vector3(1f, 0f, 1f)).normalized;
				float y = Mathf.Tan(Vector3.Angle(vector, normalized) * 0.0174532924f) * this._logWidth;
				Quaternion rotation2 = Quaternion.LookRotation(Vector3.up);
				Vector3 localScale2 = new Vector3(1f, 1f, 1.9f * this._logWidth / this._logLength);
				float num2 = this._logWidth / 2f * 0.98f;
				for (int k = 0; k < edge._segments.Length; k++)
				{
					WallArchitect.HorizontalSegment horizontalSegment2 = edge._segments[k];
					Vector3 vector4 = horizontalSegment2._p1;
					vector4.y -= num2;
					Transform transform4 = new GameObject("Segment" + k).transform;
					transform4.parent = transform;
					horizontalSegment2._root = transform4;
					transform4.position = horizontalSegment2._p1;
					float num3 = Vector3.Distance(horizontalSegment2._p1, horizontalSegment2._p2);
					int num4 = Mathf.Max(Mathf.RoundToInt((num3 - this._logWidth * 0.96f / 2f) / this._logWidth), 1);
					Vector3 vector5 = normalized * this._logWidth * 0.98f;
					vector5.y = y;
					vector4 += vector5 / 2f;
					if (vector.y < 0f)
					{
						vector5.y *= -1f;
					}
					for (int l = 0; l < num4; l++)
					{
						Transform transform5 = base.NewLog(vector4, rotation2);
						transform5.parent = transform4;
						this._newPool.Enqueue(transform5);
						vector4 += vector5;
						transform5.localScale = localScale2;
					}
				}
			}
			return transform;
		}
	}
}
