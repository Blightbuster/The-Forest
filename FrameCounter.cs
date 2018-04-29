using System;
using UnityEngine;


public class FrameCounter : MonoBehaviour
{
	
	private void Start()
	{
		this.timeleft = this.updateInterval;
	}

	
	private void OnGUI()
	{
		this.timeleft -= Time.deltaTime;
		this.accum += Time.timeScale / Time.deltaTime;
		this.frames++;
		if (this.guic == null)
		{
			this.guic = new GUIContent(this.outText);
		}
		GUI.Label(new Rect(10f, 10f, 300f, 50f), this.guic);
		GUI.Label(new Rect(10f, 30f, 300f, 50f), "WASD to move, left shift for speed");
		GUI.Label(new Rect(10f, 50f, 300f, 50f), "Right click rotate");
		GUI.Label(new Rect(10f, 70f, 300f, 50f), "'C' to toggle Scion");
		if ((double)this.timeleft <= 0.0)
		{
			float num = this.accum / (float)this.frames;
			this.outText = string.Format("{0:F2} FPS", num);
			this.guic.text = this.outText;
			this.timeleft = this.updateInterval;
			this.accum = 0f;
			this.frames = 0;
		}
	}

	
	public float updateInterval = 0.5f;

	
	private float accum;

	
	private int frames;

	
	private float timeleft;

	
	private string outText;

	
	private GUIContent guic;
}
