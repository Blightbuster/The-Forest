using System;
using UnityEngine;


public class LockLocalPosition : MonoBehaviour
{
	
	private void OnEnable()
	{
		this._transform = base.transform;
		this.localPos = this._transform.localPosition;
	}

	
	private void LateUpdate()
	{
		if (this._transform.localPosition != this.localPos)
		{
			this._transform.localPosition = this.localPos;
		}
	}

	
	private Vector3 localPos;

	
	private Transform _transform;
}
