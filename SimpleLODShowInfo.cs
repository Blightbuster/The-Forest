using System;
using UnityEngine;


public class SimpleLODShowInfo : MonoBehaviour
{
	
	private void OnGUI()
	{
		if (this.lodSwitcher != null)
		{
			Vector3 vector = Camera.main.WorldToScreenPoint(base.transform.position + new Vector3(0f, this.offsetY, 0f));
			if (vector.z > 0f)
			{
				GUIStyle guistyle = new GUIStyle(GUI.skin.label);
				guistyle.normal.textColor = Color.black;
				guistyle.alignment = TextAnchor.LowerCenter;
				GUI.Label(new Rect(vector.x - 50f, (float)Screen.height - vector.y - 20f, 100f, 20f), "LOD " + this.lodSwitcher.GetLODLevel(), guistyle);
			}
		}
	}

	
	public LODSwitcher lodSwitcher;

	
	public float offsetY = 2f;
}
