using System;
using UnityEngine;


public class ClearSave : MonoBehaviour
{
	
	private void Awake()
	{
		if (!PlayerPrefs.HasKey("Saved"))
		{
			this.FakeSave();
		}
	}

	
	private void FakeSave()
	{
		PlayerPrefs.SetInt("Saved", 1);
		PlayerPrefs.DeleteKey("__RESUME__");
	}

	
	private int SavedAmount;
}
