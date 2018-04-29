using System;
using TheForest.Utils;
using UnityEngine;


public class enableWithDelay : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("enableThisGo", this.delay);
	}

	
	private void enableThisGo()
	{
		if (this.graphUpdateCheck && Scene.SceneTracker.graphsBeingUpdated)
		{
			return;
		}
		if (this.go)
		{
			this.go.SetActive(true);
		}
	}

	
	public GameObject go;

	
	public float delay;

	
	public bool graphUpdateCheck;
}
