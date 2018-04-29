using System;
using UnityEngine;


public class SaveAndReload : MonoBehaviour
{
	
	private void Awake()
	{
		this.id = SaveAndReload._id++;
	}

	
	private void OnMouseDown()
	{
		JSONLevelSerializer.SaveObjectTreeToServer("ftp:
		{
			Debug.Log("Uploaded!" + error);
		});
		UnityEngine.Object.Destroy(base.gameObject);
		Loom.QueueOnMainThread(delegate
		{
			Debug.Log("Downloading");
			JSONLevelSerializer.LoadObjectTreeFromServer("http:
		}, 6f);
	}

	
	private static int _id;

	
	public int id;
}
