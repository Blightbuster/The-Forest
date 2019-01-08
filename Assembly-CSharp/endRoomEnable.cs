using System;
using UnityEngine;

public class endRoomEnable : MonoBehaviour
{
	private void Start()
	{
		this.doEnable(false);
		this.col.enabled = false;
		this.col.enabled = true;
	}

	private void doEnable(bool onoff)
	{
		foreach (GameObject gameObject in this.enableGo)
		{
			if (gameObject)
			{
				gameObject.SetActive(onoff);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			this.doEnable(true);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public GameObject[] enableGo;

	public Collider col;
}
