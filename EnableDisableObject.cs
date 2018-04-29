using System;
using UnityEngine;


[ExecuteInEditMode]
public class EnableDisableObject : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void Update()
	{
		Behaviour light = this.Light1;
		bool enabled = this.Enabled;
		this.Light2.enabled = enabled;
		light.enabled = enabled;
	}

	
	private void OnGUI()
	{
		GUI.Label(new Rect(185f, 145f, 150f, 30f), "Turn lights on-off");
		this.Enabled = GUI.Toggle(new Rect(295f, 145f, 100f, 20f), this.Enabled, string.Empty);
	}

	
	private bool Enabled = true;

	
	public Light Light1;

	
	public Light Light2;
}
