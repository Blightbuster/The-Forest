using System;
using UnityEngine;


public class setLightTrackerTag : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("setTrackerTag", 0.2f);
	}

	
	private void setTrackerTag()
	{
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			transform.gameObject.tag = "Player";
		}
	}
}
