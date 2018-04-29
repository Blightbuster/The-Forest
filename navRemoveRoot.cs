using System;
using Pathfinding;
using UnityEngine;


public class navRemoveRoot : MonoBehaviour
{
	
	private void doRootNavRemove(Bounds b)
	{
		this.getBounds = b;
		base.Invoke("startRemove", 0.5f);
	}

	
	private void startRemove()
	{
		if (!AstarPath.active)
		{
			return;
		}
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain)
		{
			GraphUpdateObject ob = new GraphUpdateObject(this.getBounds);
			Debug.DrawRay(this.getBounds.center, Vector3.up * this.getBounds.extents.y, Color.red, 10f);
			Debug.DrawRay(this.getBounds.center, Vector3.right * this.getBounds.extents.x, Color.red, 10f);
			Debug.DrawRay(this.getBounds.center, Vector3.forward * this.getBounds.extents.z, Color.red, 10f);
			AstarPath.active.UpdateGraphs(ob, 0f);
			UnityEngine.Object.Destroy(base.gameObject, 2f);
		}
	}

	
	public Bounds getBounds;

	
	public bool isAboveTerrain = true;
}
