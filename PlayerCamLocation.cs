using System;
using UnityEngine;


public class PlayerCamLocation : MonoBehaviour
{
	
	private void Awake()
	{
		this.Update();
	}

	
	private void Update()
	{
		PlayerCamLocation.PlayerLoc = base.transform.position;
	}

	
	public static Vector3 PlayerLoc;
}
