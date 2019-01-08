using System;
using UnityEngine;

public class forceLocalScale : MonoBehaviour
{
	private void Update()
	{
		base.transform.localScale = new Vector3(this.value, this.value, this.value);
	}

	public float value = 1f;
}
