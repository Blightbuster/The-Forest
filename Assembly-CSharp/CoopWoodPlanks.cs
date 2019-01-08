using System;
using UnityEngine;

public class CoopWoodPlanks : MonoBehaviour
{
	private void Awake()
	{
		CoopWoodPlanks.Instance = this;
	}

	public int GetIndex(BreakWoodSimple wood)
	{
		if (wood)
		{
			for (int i = 0; i < this.Planks.Length; i++)
			{
				if (object.ReferenceEquals(wood, this.Planks[i]))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static CoopWoodPlanks Instance;

	public BreakWoodSimple[] Planks;
}
