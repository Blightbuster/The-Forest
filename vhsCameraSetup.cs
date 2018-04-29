using System;
using UnityEngine;


public class vhsCameraSetup : MonoBehaviour
{
	
	private void Start()
	{
		if (this.useCustomFrameRate)
		{
			Application.targetFrameRate = this.frameRateLimit;
		}
	}

	
	private void Update()
	{
		if (this._sourceLenseJoint && this._targetCamera)
		{
			this._targetCamera.fieldOfView = this._startFieldOfViewVal - this._sourceLenseJoint.transform.localPosition.y * 100f * this._fovConversionFactor;
		}
	}

	
	public Transform _sourceLenseJoint;

	
	public Camera _targetCamera;

	
	public float _startFieldOfViewVal = 50f;

	
	public float _fovConversionFactor = 0.8f;

	
	public bool useCustomFrameRate;

	
	public int frameRateLimit = 30;
}
