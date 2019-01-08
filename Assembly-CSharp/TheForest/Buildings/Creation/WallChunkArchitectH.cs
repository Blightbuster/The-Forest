using System;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[AddComponentMenu("Buildings/Creation/Wall Chunk Architect (Always Horizontal)")]
	[DoNotSerializePublic]
	public class WallChunkArchitectH : WallChunkArchitect
	{
		public override bool UseHorizontalLogs
		{
			get
			{
				return true;
			}
		}
	}
}
