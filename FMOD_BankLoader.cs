using System;
using System.Collections;
using System.IO;
using UnityEngine;


public class FMOD_BankLoader : MonoBehaviour
{
	
	private void Start()
	{
		FMOD_BankLoader.sPendingLoads++;
		base.StartCoroutine(this.LoadBanks());
	}

	
	private IEnumerator LoadBanks()
	{
		while (!FMOD_Listener.HasLoadedBanks)
		{
			yield return null;
		}
		string fullPath = Application.streamingAssetsPath + "/" + this.folderToLoad;
		string[] banks = Directory.GetFiles(fullPath, "*.bank");
		for (int i = 0; i < banks.Length; i++)
		{
			banks[i] = FMOD_Listener.LoadBank(banks[i]);
		}
		this.loadedBanks = banks;
		FMOD_BankLoader.sPendingLoads--;
		if (FMOD_BankLoader.sPendingLoads == 0)
		{
			FMOD_StudioEventEmitter[] array = FMOD_StudioEventEmitter.sAwaitingBankLoad.ToArray();
			for (int j = array.Length - 1; j >= 0; j--)
			{
				array[j].Activate();
			}
			LinearEmitter[] array2 = LinearEmitter.sAwaitingBankLoad.ToArray();
			for (int k = array2.Length - 1; k >= 0; k--)
			{
				array2[k].Activate();
			}
			FMOD_Listener.ProcessPreloadRequests();
		}
		yield break;
	}

	
	private void OnDisable()
	{
		if (FMOD_Listener.HasLoadedBanks && this.loadedBanks != null)
		{
			for (int i = 0; i < this.loadedBanks.Length; i++)
			{
				FMOD_Listener.UnloadBank(this.loadedBanks[i]);
			}
		}
	}

	
	[Tooltip("The name of a folder in StreamingAssets from which to load banks")]
	public string folderToLoad;

	
	private string[] loadedBanks;

	
	private static int sPendingLoads;
}
