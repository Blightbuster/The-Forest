using System;
using UnityEngine;


public class TerrainRenderQueue : MonoBehaviour
{
	
	private void Start()
	{
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain)
		{
			activeTerrain.materialTemplate.renderQueue = this.queue;
		}
	}

	
	public int queue = 1900;
}
