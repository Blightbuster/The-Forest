using System;
using UnityEngine;


public class SaveAndLoadToServer : MonoBehaviour
{
	
	private void OnGUI()
	{
		using (new VerticalCentered())
		{
			if (this.targetGameObject && GUILayout.Button("Save to server JSON", new GUILayoutOption[0]))
			{
				JSONLevelSerializer.SaveObjectTreeToServer("ftp:
				UnityEngine.Object.Destroy(this.targetGameObject);
			}
			if (!this.targetGameObject && GUILayout.Button("Load from server JSON", new GUILayoutOption[0]))
			{
				JSONLevelSerializer.LoadObjectTreeFromServer("http:
			}
			if (this.targetGameObject && GUILayout.Button("Save to server Binary", new GUILayoutOption[0]))
			{
				LevelSerializer.SaveObjectTreeToServer("ftp:
				UnityEngine.Object.Destroy(this.targetGameObject);
			}
			if (!this.targetGameObject && GUILayout.Button("Load from server Binary", new GUILayoutOption[0]))
			{
				LevelSerializer.LoadObjectTreeFromServer("http:
			}
			if (GUILayout.Button("Save scene to server JSON", new GUILayoutOption[0]))
			{
				JSONLevelSerializer.SerializeLevelToServer("ftp:
			}
			if (GUILayout.Button("Load scene from server JSON", new GUILayoutOption[0]))
			{
				JSONLevelSerializer.LoadSavedLevelFromServer("http:
			}
			if (GUILayout.Button("Save scene to server Binary", new GUILayoutOption[0]))
			{
				JSONLevelSerializer.SerializeLevelToServer("ftp:
			}
			if (GUILayout.Button("Load scene from server Binary", new GUILayoutOption[0]))
			{
				JSONLevelSerializer.LoadSavedLevelFromServer("http:
			}
		}
	}

	
	private void CompletedLoad(LevelLoader loader)
	{
		this.targetGameObject = loader.Last;
	}

	
	private void CompletedJSONLoad(JSONLevelLoader loader)
	{
		this.targetGameObject = loader.Last;
	}

	
	private void Completed(Exception e)
	{
		if (e != null)
		{
			Debug.Log("Error");
			Debug.Log(e.ToString());
			Debug.Log(base.transform.position.ToString());
		}
		else
		{
			Debug.Log("Succeeded");
		}
	}

	
	public GameObject targetGameObject;
}
