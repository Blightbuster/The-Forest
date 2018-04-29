using System;
using System.Collections.Generic;


public static class weightsRandomizer
{
	
	public static weightsRandomizer<R> From<R>(Dictionary<R, int> spawnRate)
	{
		return new weightsRandomizer<R>(spawnRate);
	}
}
