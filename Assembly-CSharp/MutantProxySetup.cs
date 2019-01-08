using System;
using UnityEngine;

public class MutantProxySetup : MonoBehaviour
{
	private void Start()
	{
		UnityEngine.Object.Destroy(this);
	}

	private void SendPosition()
	{
	}

	public GameObject[] ObjectsToDestroy;

	public MonoBehaviour[] ScriptsToDestroy;

	public GameObject Base;
}
