using System;
using System.Collections;
using UnityEngine;

public class StreamCaveIn : MonoBehaviour
{
	private IEnumerator LoadIn()
	{
		AsyncOperation async = Application.LoadLevelAdditiveAsync("CaveProps_Streaming");
		yield return async;
		Debug.Log("Loading complete");
		this.MyStreaming = GameObject.FindWithTag("CaveProps");
		yield break;
	}

	private void LoadOut()
	{
		if (this.MyStreaming != null)
		{
			UnityEngine.Object.Destroy(this.MyStreaming);
		}
	}

	private GameObject MyStreaming;
}
