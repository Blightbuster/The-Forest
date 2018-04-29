using System;
using UnityEngine;


public class PlayMakerRPCProxy : MonoBehaviour
{
	
	public void Reset()
	{
		this.fsms = base.GetComponents<PlayMakerFSM>();
	}

	
	[RPC]
	public void ForwardEvent(string eventName)
	{
		foreach (PlayMakerFSM playMakerFSM in this.fsms)
		{
			playMakerFSM.SendEvent(eventName);
		}
	}

	
	public PlayMakerFSM[] fsms;
}
