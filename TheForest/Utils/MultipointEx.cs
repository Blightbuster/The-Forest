using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	
	public static class MultipointEx
	{
		
		public static Vector3 ClosestPointTo(this List<Vector3> multipoint, Vector3 point)
		{
			Vector3 result = Vector3.zero;
			float num = float.MaxValue;
			bool flag = false;
			for (int i = 1; i < multipoint.Count; i++)
			{
				Vector3 vector;
				if (MathEx.ProjectPointOnLineSegment(multipoint[i - 1], multipoint[i], point, out vector))
				{
					float num2 = Vector3.Distance(point, vector);
					if (num2 < num || flag)
					{
						num = num2;
						result = vector;
						flag = false;
					}
				}
				else if (num == 3.40282347E+38f)
				{
					num = Vector3.Distance(point, vector);
					result = vector;
					flag = true;
				}
			}
			return result;
		}

		
		public static Vector3 GetCenterPosition(this List<Vector3> multipoint)
		{
			Vector3 zero = Vector3.zero;
			foreach (Vector3 vector in multipoint)
			{
				zero.x += vector.x;
				zero.y += vector.y;
				zero.z += vector.z;
			}
			zero.x /= (float)multipoint.Count;
			zero.y /= (float)multipoint.Count;
			zero.z /= (float)multipoint.Count;
			return zero;
		}
	}
}
