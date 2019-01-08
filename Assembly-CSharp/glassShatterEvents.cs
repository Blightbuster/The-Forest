using System;
using UnityEngine;

public class glassShatterEvents : MonoBehaviour
{
	private void enableGlass()
	{
		if (this.shatteredGlassGo)
		{
			this.shatteredGlassGo.SetActive(true);
		}
		if (this.originalGlassGo)
		{
			this.originalGlassGo.SetActive(false);
		}
	}

	public GameObject shatteredGlassGo;

	public GameObject originalGlassGo;
}
