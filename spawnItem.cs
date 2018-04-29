using System;
using System.Collections;
using UnityEngine;


public class spawnItem : MonoBehaviour
{
	
	private void invokePickupSpawn()
	{
		base.StartCoroutine("doPickupSpawn");
	}

	
	public IEnumerator doPickupSpawn()
	{
		yield return YieldPresets.WaitPointFiveSeconds;
		foreach (GameObject go in this.pickupSpawn)
		{
			if (go)
			{
				this.pos = base.transform.position;
				if (UnityEngine.Random.value < 0.08f)
				{
					this.pos.y = this.pos.y + 0.5f;
					GameObject spawnedItem = UnityEngine.Object.Instantiate(go, this.pos, Quaternion.identity) as GameObject;
					spawnedItem.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(-400, 400), (float)UnityEngine.Random.Range(500, 700), (float)UnityEngine.Random.Range(-400, 400));
					spawnedItem.GetComponent<Rigidbody>().AddTorque(1000f, (float)UnityEngine.Random.Range(200, 1200), (float)UnityEngine.Random.Range(200, 1200));
				}
			}
		}
		yield break;
	}

	
	public GameObject[] pickupSpawn;

	
	private Vector3 pos;
}
