using System;
using System.Collections;
using UnityEngine;


public class fixPlayerCollision : MonoBehaviour
{
	
	private void Start()
	{
		base.gameObject.layer = 31;
	}

	
	private void OnEnable()
	{
		base.StartCoroutine(this.switchCollisionLayerDelayed(21, 2f));
	}

	
	private IEnumerator switchCollisionLayerDelayed(int layer, float t)
	{
		base.gameObject.layer = 31;
		yield return new WaitForSeconds(t);
		if (base.gameObject)
		{
			base.gameObject.layer = layer;
		}
		yield break;
	}
}
