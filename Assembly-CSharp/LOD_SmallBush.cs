using System;
using PathologicalGames;

public class LOD_SmallBush : LOD_Base
{
	public override LOD_Settings LodSettings
	{
		get
		{
			return LOD_Manager.Instance.SmallBush;
		}
	}

	public override SpawnPool Pool
	{
		get
		{
			if (!LOD_SmallBush._spawnPool)
			{
				LOD_SmallBush._spawnPool = PoolManager.Pools["Bushes"];
			}
			return LOD_SmallBush._spawnPool;
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
