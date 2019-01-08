using System;
using UnityEngine;

public class setupClientPlaneCrash : MonoBehaviour
{
	private void Start()
	{
		if (ForestVR.Enabled && BoltNetwork.isClient)
		{
			if (this.Head)
			{
				this.Head.SetActive(true);
			}
			if (this.Hair)
			{
				this.Hair.SetActive(true);
			}
		}
	}

	public GameObject Head;

	public GameObject Hair;
}
