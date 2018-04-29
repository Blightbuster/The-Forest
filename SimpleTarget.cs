using System;
using UnityEngine;


public class SimpleTarget : MonoBehaviour
{
	
	private void Start()
	{
		this.player = GameObject.Find("player");
		this.MyTransform = base.transform;
	}

	
	private void Update()
	{
		this.MyTransform.LookAt(this.player.transform);
	}

	
	private GameObject player;

	
	private Transform MyTransform;
}
