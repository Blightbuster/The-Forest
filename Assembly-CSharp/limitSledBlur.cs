using System;
using TheForest.Buildings.World;
using UnityEngine;

public class limitSledBlur : MonoBehaviour
{
	private void Start()
	{
		this.mh = base.transform.GetComponentInChildren<MultiHolder>();
	}

	public MultiHolder mh;
}
