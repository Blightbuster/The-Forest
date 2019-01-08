using System;
using UnityEngine;

[AddComponentMenu("Storage/Rooms/Room")]
[DontStore]
public class Room : MonoBehaviour
{
	private void Awake()
	{
		Room.Current = this;
	}

	public void Save()
	{
		RoomManager.SaveCurrentRoom();
	}

	public static Room Current;
}
