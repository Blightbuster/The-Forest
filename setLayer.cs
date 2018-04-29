using System;
using UnityEngine;


public class setLayer : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("doSetLayer", this.delay);
	}

	
	private void OnEnable()
	{
		base.Invoke("doSetLayer", this.delay);
	}

	
	private void doSetLayer()
	{
		base.gameObject.layer = this.setToLayer;
	}

	
	public int setToLayer;

	
	public float delay;
}
