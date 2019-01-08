using System;
using UnityEngine;

namespace Pathfinding
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_local_space_rich_a_i.php")]
	public class LocalSpaceRichAI : RichAI
	{
		public override void UpdatePath()
		{
			this.canSearchPath = true;
			this.waitingForPathCalc = false;
			Path currentPath = this.seeker.GetCurrentPath();
			if (currentPath != null && !this.seeker.IsDone())
			{
				currentPath.Error();
				currentPath.Claim(this);
				currentPath.Release(this, false);
			}
			this.waitingForPathCalc = true;
			this.lastRepath = Time.time;
			Matrix4x4 matrix = this.graph.GetMatrix();
			this.seeker.StartPath(matrix.MultiplyPoint3x4(this.tr.position), matrix.MultiplyPoint3x4(this.target.position));
		}

		protected override Vector3 UpdateTarget(RichFunnel fn)
		{
			Matrix4x4 matrix = this.graph.GetMatrix();
			Matrix4x4 inverse = matrix.inverse;
			Debug.DrawRay(matrix.MultiplyPoint3x4(this.tr.position), Vector3.up * 2f, Color.red);
			Debug.DrawRay(inverse.MultiplyPoint3x4(this.tr.position), Vector3.up * 2f, Color.green);
			this.nextCorners.Clear();
			Vector3 vector = this.tr.position;
			Vector3 vector2 = matrix.MultiplyPoint3x4(vector);
			bool flag;
			vector2 = fn.Update(vector2, this.nextCorners, 2, out this.lastCorner, out flag);
			vector = inverse.MultiplyPoint3x4(vector2);
			Debug.DrawRay(vector, Vector3.up * 3f, Color.black);
			for (int i = 0; i < this.nextCorners.Count; i++)
			{
				this.nextCorners[i] = inverse.MultiplyPoint3x4(this.nextCorners[i]);
				Debug.DrawRay(this.nextCorners[i], Vector3.up * 3f, Color.yellow);
			}
			if (flag && !this.waitingForPathCalc)
			{
				this.UpdatePath();
			}
			return vector;
		}

		public LocalSpaceGraph graph;
	}
}
