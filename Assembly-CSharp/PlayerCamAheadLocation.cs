using System;
using UnityEngine;

public class PlayerCamAheadLocation : MonoBehaviour
{
	private void Awake()
	{
		this.Update();
	}

	private void Update()
	{
		PlayerCamAheadLocation.PlayerLoc = base.transform.position;
	}

	public static Vector3 PlayerLoc;
}
