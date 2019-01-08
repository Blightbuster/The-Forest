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
		foreach (GameObject gameObject in this.pickupSpawn)
		{
			if (gameObject)
			{
				this.pos = base.transform.position;
				if (UnityEngine.Random.value < 0.08f)
				{
					this.pos.y = this.pos.y + 0.5f;
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, this.pos, Quaternion.identity);
					gameObject2.GetComponent<Rigidbody>().AddForce((float)UnityEngine.Random.Range(-400, 400), (float)UnityEngine.Random.Range(500, 700), (float)UnityEngine.Random.Range(-400, 400));
					gameObject2.GetComponent<Rigidbody>().AddTorque(1000f, (float)UnityEngine.Random.Range(200, 1200), (float)UnityEngine.Random.Range(200, 1200));
				}
			}
		}
		yield break;
	}

	public GameObject[] pickupSpawn;

	private Vector3 pos;
}
