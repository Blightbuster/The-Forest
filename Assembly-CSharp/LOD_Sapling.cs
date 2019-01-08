using System;
using PathologicalGames;

public class LOD_Sapling : LOD_Trees
{
	public override LOD_Settings LodSettings
	{
		get
		{
			return LOD_Manager.Instance.Bush;
		}
	}

	public override SpawnPool Pool
	{
		get
		{
			if (!LOD_Sapling._spawnPool)
			{
				LOD_Sapling._spawnPool = PoolManager.Pools["Trees"];
			}
			return LOD_Sapling._spawnPool;
		}
	}

	public override bool DestroyInsteadOfDisable
	{
		get
		{
			return true;
		}
	}

	private static SpawnPool _spawnPool;
}
