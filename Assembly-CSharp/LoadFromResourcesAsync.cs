using System;
using System.Collections;
using UnityEngine;

public class LoadFromResourcesAsync : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSeconds(this.delay);
		ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(this.path);
		while (!resourceRequest.isDone)
		{
			yield return null;
		}
		GameObject objectToLoad = resourceRequest.asset as GameObject;
		UnityEngine.Object.Instantiate<GameObject>(objectToLoad);
		yield break;
	}

	public string path;

	public float delay;
}
