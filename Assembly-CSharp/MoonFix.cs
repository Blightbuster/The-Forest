using System;
using UnityEngine;

public class MoonFix : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Light>().color = this.MyColor;
	}

	private void Update()
	{
		base.GetComponent<Light>().color = this.MyColor;
	}

	public Color MyColor;
}
