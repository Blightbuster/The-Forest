using System;
using TheForest.Utils;
using UnityEngine;

public class UnParent : MonoBehaviour
{
	private void Start()
	{
		if (this.delay > 0f)
		{
			base.Invoke("doUnParent", this.delay);
		}
		else
		{
			base.transform.parent = null;
		}
	}

	private void doUnParent()
	{
		if (base.transform)
		{
			if (Scene.ActiveMB)
			{
				base.transform.parent = Scene.ActiveMB.transform;
			}
			base.transform.parent = null;
		}
	}

	public float delay;
}
