using System;
using TheForest.Buildings.Creation;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class HoleCutterNode : MonoBehaviour
	{
		
		private void Start()
		{
			FloorHoleArchitect.Nodes.Add(this);
		}

		
		private void OnDestroy()
		{
			FloorHoleArchitect.Nodes.Remove(this);
		}

		
		
		
		public FloorHoleArchitect TargetedBy { get; set; }
	}
}
