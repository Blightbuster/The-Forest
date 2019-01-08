using System;
using System.Collections;
using UnityEngine;

public class spinRagdoll : MonoBehaviour
{
	private void Start()
	{
		if (base.transform.GetComponent<Rigidbody>())
		{
			base.Invoke("spin", 0.05f);
		}
	}

	private void spin()
	{
		base.transform.GetComponent<Rigidbody>().AddTorque(20000f, 20000f, 20000f);
	}

	private IEnumerator spinForce()
	{
		float t = 0f;
		float max = 0.5f;
		float dropoff = 1f;
		while (t < max)
		{
			dropoff *= 1f - t * 2f;
			base.transform.GetComponent<Rigidbody>().AddTorque(100000f * dropoff, 100000f * dropoff, 100000f * dropoff);
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}
}
