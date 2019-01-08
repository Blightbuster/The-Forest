using System;
using UnityEngine;

namespace Pathfinding
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_local_space_graph.php")]
	public class LocalSpaceGraph : MonoBehaviour
	{
		private void Start()
		{
			this.originalMatrix = base.transform.localToWorldMatrix;
		}

		public Matrix4x4 GetMatrix()
		{
			return base.transform.worldToLocalMatrix * this.originalMatrix;
		}

		protected Matrix4x4 originalMatrix;
	}
}
