using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class CaveOptimizer : MonoBehaviour
{
	
	private void Awake()
	{
		if (base.transform.root == base.transform)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			CaveOptimizer.CanLoadOnRope = true;
			this.Common = new CaveData
			{
				SceneName = "CaveProps_Streaming",
				RootName = "CaveProps"
			};
		}
	}

	
	private void Update()
	{
		if (LocalPlayer.IsInCaves && !LocalPlayer.ActiveAreaInfo.IsLeavingCaves)
		{
			if ((CaveOptimizer.CanLoadOnRope || !LocalPlayer.AnimControl.onRope) && !LocalPlayer.IsInEndgame && !this.Common.LoadedOrLoading)
			{
				base.StartCoroutine(this.Common.LoadIn());
			}
		}
		else
		{
			bool flag = false;
			if (this.Common.LoadedOrLoading)
			{
				this.Common.Unload();
				flag = true;
			}
			if (flag)
			{
				this.LastestCave = LocalPlayer.ActiveAreaInfo.CurrentCave;
				CaveOptimizer.CanLoadOnRope = true;
				base.Invoke("CleanUp", 0.1f);
			}
		}
	}

	
	private IEnumerator CleanUp()
	{
		yield return null;
		yield return null;
		Resources.UnloadUnusedAssets();
		yield break;
	}

	
	public static bool CanLoadOnRope = true;

	
	[NameFromEnumIndex(typeof(CaveNames))]
	public CaveData[] Caves;

	
	private CaveData Common;

	
	private CaveNames LastestCave = CaveNames.NotInCaves;
}
