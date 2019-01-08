using System;
using UnityEngine;

public class chanceToEnable : MonoBehaviour
{
	private void Start()
	{
		if (UnityEngine.Random.value < this.chance && this.go)
		{
			this.go.SetActive(true);
		}
	}

	public GameObject go;

	public float chance = 0.1f;
}
