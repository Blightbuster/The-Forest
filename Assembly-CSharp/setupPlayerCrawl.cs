using System;
using UnityEngine;

public class setupPlayerCrawl : MonoBehaviour
{
	private void startCrawl()
	{
		if (this.hair && !BoltNetwork.isClient && !ForestVR.Enabled)
		{
			this.hair.SetActive(false);
		}
		base.transform.GetComponent<Animator>().SetBool("begin", true);
	}

	public GameObject hair;
}
