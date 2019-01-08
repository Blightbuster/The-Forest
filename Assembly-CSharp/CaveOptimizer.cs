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
		}
	}

	private void Update()
	{
		if (LocalPlayer.IsInCaves && !LocalPlayer.ActiveAreaInfo.IsLeavingCaves)
		{
			if (this.UseSharedCaveScene && (CaveOptimizer.CanLoadOnRope || !LocalPlayer.AnimControl.onRope) && !this.Shared.LoadedOrLoading)
			{
				base.StartCoroutine(this.Shared.LoadIn());
			}
			if (LocalPlayer.ActiveAreaInfo.CurrentCave == CaveNames.NotInCaves)
			{
				for (int i = 0; i < this.Caves.Length; i++)
				{
					CaveData caveData = this.Caves[i];
					if (!LocalPlayer.IsInEndgame || caveData.AllowInEndgame)
					{
						if (!caveData.LoadedOrLoading)
						{
							base.StartCoroutine(caveData.LoadIn());
						}
					}
					else if (caveData.LoadedOrLoading)
					{
						caveData.Unload();
					}
				}
			}
			else if ((CaveOptimizer.CanLoadOnRope || !LocalPlayer.AnimControl.onRope) && LocalPlayer.ActiveAreaInfo.CurrentCave != this.LastestCave)
			{
				CaveOptimizer.CanLoadOnRope = false;
				if (this.LastestCave != CaveNames.NotInCaves)
				{
					for (int j = 0; j < this.Caves.Length; j++)
					{
						if (j != (int)LocalPlayer.ActiveAreaInfo.CurrentCave)
						{
							this.Caves[j].Unload();
						}
					}
				}
				if ((CaveNames)this.Caves.Length > LocalPlayer.ActiveAreaInfo.CurrentCave)
				{
					CaveData caveData2 = this.Caves[(int)LocalPlayer.ActiveAreaInfo.CurrentCave];
					if (!LocalPlayer.IsInEndgame || caveData2.AllowInEndgame)
					{
						if (!caveData2.LoadedOrLoading)
						{
							base.StartCoroutine(caveData2.LoadIn());
							this.LastestCave = LocalPlayer.ActiveAreaInfo.CurrentCave;
						}
					}
					else if (caveData2.LoadedOrLoading)
					{
						caveData2.Unload();
					}
				}
			}
		}
		else
		{
			bool flag = false;
			if (this.UseSharedCaveScene && this.Shared.LoadedOrLoading)
			{
				this.Shared.Unload();
				flag = true;
			}
			for (int k = 0; k < this.Caves.Length; k++)
			{
				if (this.Caves[k].LoadedOrLoading)
				{
					this.Caves[k].Unload();
					flag = true;
				}
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
		ResourcesHelper.UnloadUnusedAssets();
		yield break;
	}

	public static bool CanLoadOnRope = true;

	[NameFromEnumIndex(typeof(CaveNames))]
	public CaveData[] Caves;

	public CaveData Shared;

	public bool UseSharedCaveScene;

	private CaveNames LastestCave = CaveNames.NotInCaves;
}
