using System;
using UnityEngine;

public class disableInOverworld : MonoBehaviour
{
	private void Start()
	{
		if (base.transform.position.y > -200f)
		{
			foreach (GameObject gameObject in this.disableGo)
			{
				gameObject.SetActive(false);
			}
			foreach (GameObject gameObject2 in this.enableGo)
			{
				gameObject2.SetActive(true);
			}
		}
	}

	public GameObject[] disableGo;

	public GameObject[] enableGo;

	public bool netPrefab;
}
