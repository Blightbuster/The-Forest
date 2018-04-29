using System;
using TheForest.Utils;
using UnityEngine;


public class InventoryFakeGround : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (LocalPlayer.IsInCaves)
		{
			bool flag = LocalPlayer.IsInEndgame && LocalPlayer.SurfaceDetector.Surface != UnderfootSurfaceDetector.SurfaceType.Rock;
			this.ToggleGrounds(false, false, !flag, flag);
		}
		else
		{
			bool flag2 = LocalPlayer.Stats.IsInNorthColdArea();
			this.ToggleGrounds(!flag2, flag2, false, false);
		}
	}

	
	private void ToggleGrounds(bool outside, bool outsideSnow, bool cave, bool endgame)
	{
		if (this.OutsideGroundGo.activeSelf != outside)
		{
			this.OutsideGroundGo.SetActive(outside);
		}
		if (this.OutsideGroundSnowGo.activeSelf != outsideSnow)
		{
			this.OutsideGroundSnowGo.SetActive(outsideSnow);
		}
		if (this.CaveGroundGo.activeSelf != cave)
		{
			this.CaveGroundGo.SetActive(cave);
		}
		if (this.EndgameGroundGo.activeSelf != endgame)
		{
			this.EndgameGroundGo.SetActive(endgame);
		}
	}

	
	public GameObject OutsideGroundGo;

	
	public GameObject OutsideGroundSnowGo;

	
	public GameObject CaveGroundGo;

	
	public GameObject EndgameGroundGo;
}
