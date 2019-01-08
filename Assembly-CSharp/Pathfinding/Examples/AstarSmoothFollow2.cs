using System;
using UnityEngine;

namespace Pathfinding.Examples
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_astar_smooth_follow2.php")]
	public class AstarSmoothFollow2 : MonoBehaviour
	{
		private void LateUpdate()
		{
			Vector3 b;
			if (this.staticOffset)
			{
				b = this.target.position + new Vector3(0f, this.height, this.distance);
			}
			else if (this.followBehind)
			{
				b = this.target.TransformPoint(0f, this.height, -this.distance);
			}
			else
			{
				b = this.target.TransformPoint(0f, this.height, this.distance);
			}
			base.transform.position = Vector3.Lerp(base.transform.position, b, Time.deltaTime * this.damping);
			if (this.smoothRotation)
			{
				Quaternion b2 = Quaternion.LookRotation(this.target.position - base.transform.position, this.target.up);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b2, Time.deltaTime * this.rotationDamping);
			}
			else
			{
				base.transform.LookAt(this.target, this.target.up);
			}
		}

		public Transform target;

		public float distance = 3f;

		public float height = 3f;

		public float damping = 5f;

		public bool smoothRotation = true;

		public bool followBehind = true;

		public float rotationDamping = 10f;

		public bool staticOffset;
	}
}
