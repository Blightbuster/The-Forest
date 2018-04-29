using System;
using UnityEngine;


public class lizardAnimEvents : MonoBehaviour
{
	
	private void Start()
	{
		PlayMakerFSM[] components = base.transform.parent.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "aiBaseFSM")
			{
				this.pmBase = playMakerFSM;
			}
		}
	}

	
	private void toMatchPos()
	{
		this.pmBase.SendEvent("toMatchPos");
	}

	
	private PlayMakerFSM pmBase;
}
