using System;
using UnityEngine;

public class mutantDirtyObjectCleaner : MonoBehaviour
{
	private void Start()
	{
		this.aiNet = base.transform.GetComponent<mutantAI_net>();
		base.Invoke("cleanUpDirty", 2f);
	}

	private void cleanUpDirty()
	{
		if (!this.aiNet.leader)
		{
			foreach (GameObject gameObject in this.leaderProps)
			{
				if (gameObject)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}
		if (this.aiNet.femaleSkinny || this.aiNet.maleSkinny || this.aiNet.pale)
		{
			foreach (GameObject gameObject2 in this.weapons)
			{
				if (gameObject2)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
			}
		}
	}

	public GameObject[] leaderProps;

	public GameObject[] weapons;

	public GameObject tennisBelt;

	public GameObject fireStick;

	public GameObject fireBomb;

	private mutantAI_net aiNet;
}
