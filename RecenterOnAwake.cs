using System;
using UnityEngine;
using UnityEngine.VR;


public class RecenterOnAwake : MonoBehaviour
{
	
	private void Start()
	{
		InputTracking.Recenter();
	}
}
