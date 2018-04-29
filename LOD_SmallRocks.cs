using System;
using PathologicalGames;
using UnityEngine;


public class LOD_SmallRocks : LOD_Base
{
	
	
	public override LOD_Settings LodSettings
	{
		get
		{
			return LOD_Manager.Instance.SmallRocks;
		}
	}

	
	
	public override SpawnPool Pool
	{
		get
		{
			if (!LOD_SmallRocks._spawnPool)
			{
				LOD_SmallRocks._spawnPool = PoolManager.Pools["Rocks"];
			}
			return LOD_SmallRocks._spawnPool;
		}
	}

	
	public override void SetLOD(int lod)
	{
		bool flag = lod < this.currentLOD;
		base.SetLOD(lod);
		if (this.CurrentLodTransform)
		{
			this.CurrentLodTransform.SendMessage((!flag) ? "SkipStippling" : "ResetStippling", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private static SpawnPool _spawnPool;
}
