using System;
using UnityEngine;

public class molotovSpearFollow : MonoBehaviour
{
	private void Start()
	{
		this._targetLocalPos = base.transform.localPosition;
		this._rb = base.transform.GetComponent<Rigidbody>();
	}

	private void LateUpdate()
	{
		if (base.transform.parent == null)
		{
			return;
		}
		if (this._rb)
		{
			this._rb.useGravity = false;
			this._rb.isKinematic = false;
			base.transform.localPosition = this._targetLocalPos;
		}
	}

	private Vector3 _targetLocalPos;

	private Rigidbody _rb;
}
