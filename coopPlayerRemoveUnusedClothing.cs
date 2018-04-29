using System;
using System.Collections;
using UnityEngine;


public class coopPlayerRemoveUnusedClothing : MonoBehaviour
{
	
	private void Start()
	{
		if (!BoltNetwork.isRunning)
		{
			base.StartCoroutine("PerformClothingCleanup");
		}
	}

	
	public IEnumerator PerformClothingCleanup()
	{
		yield return YieldPresets.WaitFourSeconds;
		for (int i = 0; i < this.clothingVariations.Length; i++)
		{
			if (this.clothingVariations[i] && !this.clothingVariations[i].activeSelf)
			{
				UnityEngine.Object.Destroy(this.clothingVariations[i]);
			}
		}
		yield return null;
		yield break;
	}

	
	public GameObject[] clothingVariations;

	
	public bool alreadyClean;
}
