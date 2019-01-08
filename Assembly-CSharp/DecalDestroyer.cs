using System;
using System.Collections;
using UnityEngine;

public class DecalDestroyer : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSeconds(this.lifeTime);
		UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	public float lifeTime = 5f;
}
