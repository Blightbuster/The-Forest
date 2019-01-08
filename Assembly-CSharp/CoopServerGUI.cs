using System;
using UnityEngine;

internal class CoopServerGUI : MonoBehaviour
{
	private void OnGUI()
	{
		if (BoltNetwork.isServer)
		{
			GUILayout.BeginArea(new Rect((float)(Screen.width - 200 - 10), 10f, 200f, (float)(Screen.height - 20)));
			foreach (BoltEntity boltEntity in BoltNetwork.entities)
			{
				if (boltEntity.StateIs<IPlayerState>())
				{
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					GUILayout.Label(boltEntity.GetState<IPlayerState>().name, new GUILayoutOption[0]);
					if (GUILayout.Button("Kick", new GUILayoutOption[]
					{
						GUILayout.Width(50f)
					}))
					{
						boltEntity.source.Disconnect();
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndArea();
		}
	}

	private const int WIDTH = 200;
}
