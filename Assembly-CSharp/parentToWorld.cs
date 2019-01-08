using System;
using UnityEngine;

public class parentToWorld : MonoBehaviour
{
	private void Start()
	{
		if (this.doImmediate)
		{
			this.setToWorld();
		}
		else
		{
			base.Invoke("setToWorld", 0.5f);
		}
	}

	private void setToWorld()
	{
		base.transform.parent = null;
	}

	public bool doImmediate;
}
