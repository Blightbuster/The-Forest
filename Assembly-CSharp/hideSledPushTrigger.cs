using System;
using TheForest.Utils;
using UnityEngine;

public class hideSledPushTrigger : MonoBehaviour
{
	private void Start()
	{
		this.asp = base.transform.GetComponent<activateSledPush>();
	}

	private void LateUpdate()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return;
		}
		if (this.asp.onSled && LocalPlayer.AnimControl.overallSpeed > 0.2f)
		{
			this.asp.MyPickUp.SetActive(false);
			this.asp.Sheen.SetActive(false);
		}
	}

	private activateSledPush asp;
}
