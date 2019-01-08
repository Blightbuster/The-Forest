using System;
using UnityEngine;

public class TripWireMolotov : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("enemyRoot") || other.gameObject.CompareTag("enemyRoot"))
		{
			this.MolotovReal.SetActive(true);
			this.MolotovFake.SetActive(false);
		}
	}

	public GameObject MolotovReal;

	public GameObject MolotovFake;
}
