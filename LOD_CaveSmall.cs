using System;
using PathologicalGames;


public class LOD_CaveSmall : LOD_Base
{
	
	
	public override LOD_Settings LodSettings
	{
		get
		{
			return LOD_Manager.Instance.SmallCave;
		}
	}

	
	
	public override SpawnPool Pool
	{
		get
		{
			if (!LOD_CaveSmall._spawnPool)
			{
				LOD_CaveSmall._spawnPool = PoolManager.Pools["Caves"];
			}
			return LOD_CaveSmall._spawnPool;
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
