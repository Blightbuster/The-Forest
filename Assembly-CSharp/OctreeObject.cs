using System;
using UnityEngine;

internal class OctreeObject : MonoBehaviour
{
	private void Awake()
	{
		this.t = Mathf.Clamp(UnityEngine.Random.value * 20f, 1f, 60f);
	}

	private void Update()
	{
		Octree.Instance.Update(base.transform);
	}

	private float t;
}
