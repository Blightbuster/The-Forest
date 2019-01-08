using System;
using UnityEngine;

public class girlWormSpawner : MonoBehaviour
{
	private void Start()
	{
		if (base.transform.position.y <= -200f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		foreach (GameObject gameObject in this.pickupsGo)
		{
			gameObject.SetActive(true);
			gameObject.transform.parent = null;
		}
		if (BoltNetwork.isClient)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.wormSpawn, base.transform.position, base.transform.rotation);
		wormHiveController component = gameObject2.GetComponent<wormHiveController>();
		component.minWorms = 1;
		component.maxWorms = 34;
	}

	public GameObject wormSpawn;

	public GameObject[] pickupsGo;
}
