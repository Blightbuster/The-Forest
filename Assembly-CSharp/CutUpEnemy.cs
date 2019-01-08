using System;
using UnityEngine;

public class CutUpEnemy : MonoBehaviour
{
	private void Start()
	{
	}

	private void Hit()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.RagDollExploded, base.transform.position, base.transform.rotation);
		UnityEngine.Object.Destroy(this.Top);
	}

	public GameObject RagDollExploded;

	public GameObject Top;
}
