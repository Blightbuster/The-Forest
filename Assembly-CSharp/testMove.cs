using System;
using System.Collections;
using UnityEngine;

public class testMove : MonoBehaviour
{
	private void Start()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.col = base.GetComponent<BoxCollider>();
		base.StartCoroutine("doMove");
		this.val = UnityEngine.Random.Range(-50f, 50f);
	}

	private IEnumerator doMove()
	{
		for (;;)
		{
			Vector3 pos = new Vector3((float)UnityEngine.Random.Range(-50, 50), (float)UnityEngine.Random.Range(-50, 50), (float)UnityEngine.Random.Range(-50, 50));
			base.transform.position = pos;
			yield return null;
		}
		yield break;
	}

	private Rigidbody rb;

	private BoxCollider col;

	private float val;
}
