using System;
using UnityEngine;

[AddComponentMenu("Storage/Rooms/Dont Store Object In Room")]
public class DontStoreObjectInRoom : MonoBehaviour, IControlSerializationEx, IControlSerialization
{
	private void Awake()
	{
		LevelLoader.OnDestroyObject += this.HandleLevelLoaderOnDestroyObject;
	}

	private void HandleLevelLoaderOnDestroyObject(GameObject toBeDestroyed, ref bool cancel)
	{
		if (toBeDestroyed == base.gameObject)
		{
			cancel = this.preserveThisObjectWhenLoading;
		}
	}

	private void OnDestroy()
	{
		LevelLoader.OnDestroyObject -= this.HandleLevelLoaderOnDestroyObject;
	}

	public bool ShouldSaveWholeObject()
	{
		return !RoomManager.savingRoom;
	}

	public bool ShouldSave()
	{
		return !RoomManager.savingRoom;
	}

	public bool preserveThisObjectWhenLoading = true;
}
