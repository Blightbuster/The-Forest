using System;
using PathologicalGames;


public class LOD_Plant : LOD_Base
{
	
	
	public override LOD_Settings LodSettings
	{
		get
		{
			return LOD_Manager.Instance.Plant;
		}
	}

	
	
	public override SpawnPool Pool
	{
		get
		{
			if (!LOD_Plant._spawnPool)
			{
				LOD_Plant._spawnPool = PoolManager.Pools["Bushes"];
			}
			return LOD_Plant._spawnPool;
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
