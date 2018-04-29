using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public interface IRoof
	{
		
		float GetLevel();

		
		List<Vector3> GetPolygon();
	}
}
