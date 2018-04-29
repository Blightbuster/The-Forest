using System;
using PathologicalGames;


public class LOD_CaveMedium : LOD_Base
{
	
	
	public override LOD_Settings LodSettings
	{
		get
		{
			return LOD_Manager.Instance.MediumCave;
		}
	}

	
	
	public override SpawnPool Pool
	{
		get
		{
			if (!LOD_CaveMedium._spawnPool)
			{
				LOD_CaveMedium._spawnPool = PoolManager.Pools["Caves"];
			}
			return LOD_CaveMedium._spawnPool;
		}
	}

	
	public override void SetLOD(int lod)
	{
		base.SetLOD(lod);
		if (this.CurrentLodTransform != null)
		{
			this.CurrentLodTransform.localScale = base.transform.lossyScale;
		}
	}

	
	private static SpawnPool _spawnPool;
}
