using System;
using UnityEngine;

public class LoadIcon : MonoBehaviour
{
	private float wm
	{
		get
		{
			return (float)Screen.width * 0.15f;
		}
	}

	private float hm
	{
		get
		{
			return (float)Screen.height * 0.05f;
		}
	}

	private void OnGUI()
	{
		this.DrawLoader();
	}

	private void DrawLoader()
	{
		Matrix4x4 matrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(Time.time * 360f, new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2 + 160)));
		GUI.DrawTexture(new Rect((float)(Screen.width / 2 - 32), (float)(Screen.height / 2 + 128), 64f, 64f), this.texture_Loader);
		GUI.matrix = matrix;
	}

	[SerializeField]
	private Texture2D texture_Loader;
}
