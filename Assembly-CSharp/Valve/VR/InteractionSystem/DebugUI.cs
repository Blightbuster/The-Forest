using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class DebugUI : MonoBehaviour
	{
		public static DebugUI instance
		{
			get
			{
				if (DebugUI._instance == null)
				{
					DebugUI._instance = UnityEngine.Object.FindObjectOfType<DebugUI>();
				}
				return DebugUI._instance;
			}
		}

		private void Start()
		{
			this.player = Player.instance;
		}

		private void OnGUI()
		{
			this.player.Draw2DDebug();
		}

		private Player player;

		private static DebugUI _instance;
	}
}
