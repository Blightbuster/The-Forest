using System;
using TheForest.Utils;
using UnityEngine;

public class wormAnimEvents : MonoBehaviour
{
	private void enableFootShake()
	{
		float num = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
		if (num < 40f)
		{
			LocalPlayer.HitReactions.enableFootShake(num, 0.3f);
		}
	}

	private void playWingFlap()
	{
		Debug.Log("play wing flap");
		FMODCommon.PlayOneshotNetworked(this.wingFlapEvent, this.soundSourceTr, FMODCommon.NetworkRole.Server);
	}

	public string wingFlapEvent;

	public Transform soundSourceTr;
}
