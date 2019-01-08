using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Load Level On Click")]
public class LoadLevelOnClick : MonoBehaviour
{
	public void OnClick()
	{
		if (!string.IsNullOrEmpty(this.levelName))
		{
			Application.LoadLevel(this.levelName);
		}
	}

	public string levelName;
}
