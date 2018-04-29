﻿using System;
using UnityEngine;


[AddComponentMenu("Storage/Rooms/Examples/Player Spawn Point")]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(StoreInformation))]
public class PlayerSpawnPoint : MonoBehaviour
{
	
	
	
	public bool current
	{
		get
		{
			return PlayerSpawnPoint.currentSpawnPoint == this;
		}
		set
		{
			if (value)
			{
				PlayerSpawnPoint.currentSpawnPoint = this;
			}
			else if (PlayerSpawnPoint.currentSpawnPoint == this)
			{
				PlayerSpawnPoint.currentSpawnPoint = null;
			}
		}
	}

	
	private void Awake()
	{
		base.GetComponent<Collider>().isTrigger = true;
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == PlayerLocator.PlayerGameObject)
		{
			this.current = true;
		}
	}

	
	private void OnRoomWasLoaded()
	{
		if (this.current)
		{
			PlayerLocator.Current.transform.position = base.transform.position;
			PlayerLocator.Current.transform.rotation = base.transform.rotation;
		}
	}

	
	public static PlayerSpawnPoint currentSpawnPoint;
}
