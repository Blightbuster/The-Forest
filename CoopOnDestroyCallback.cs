using System;
using UnityEngine;


public class CoopOnDestroyCallback : MonoBehaviour
{
	
	private void OnDestroy()
	{
		if (this.Callback != null)
		{
			this.Callback();
			this.Callback = null;
		}
	}

	
	public Action Callback;
}
