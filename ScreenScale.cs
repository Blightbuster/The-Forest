using System;
using UnityEngine;


public class ScreenScale : MonoBehaviour
{
	
	private void Awake()
	{
		this.camera = base.gameObject.GetComponent<Camera>();
		if (this.camera)
		{
			this.currentWidth = (float)Screen.width;
			float value = this.baseWidth / this.currentWidth;
			this.camera.orthographicSize = Mathf.Clamp(value, 0.25f, 2f);
		}
		else
		{
			Debug.LogError("No camera attached to ScreenScale component");
		}
	}

	
	private void Start()
	{
	}

	
	private void Update()
	{
	}

	
	public float baseWidth = 1024f;

	
	private float currentWidth = 1024f;

	
	private Camera camera;
}
