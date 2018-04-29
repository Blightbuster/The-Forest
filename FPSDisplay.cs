using System;
using UnityEngine;


public class FPSDisplay : MonoBehaviour
{
	
	private void Start()
	{
		this.updateDelay = Time.time + 0.5f;
	}

	
	private void Update()
	{
		this.deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
	}

	
	private void OnGUI()
	{
		int width = Screen.width;
		int height = Screen.height;
		GUIStyle guistyle = new GUIStyle();
		Rect position = new Rect(0f, 0f, (float)width, (float)(height * 2 / 100));
		guistyle.alignment = TextAnchor.UpperLeft;
		guistyle.fontSize = height * 2 / 100;
		guistyle.normal.textColor = new Color(0f, 0f, 0.5f, 1f);
		if (Time.time > this.updateDelay)
		{
			this.msec = this.deltaTime * 1000f;
			this.fps = 1f / this.deltaTime;
			this.text = string.Format("{0:0.0} ms ({1:0.} fps)", this.msec, this.fps);
			this.updateDelay = Time.time + 0.5f;
		}
		GUI.Label(position, this.text, guistyle);
	}

	
	private float deltaTime;

	
	private float updateDelay;

	
	private float msec;

	
	private float fps;

	
	private string text;
}
