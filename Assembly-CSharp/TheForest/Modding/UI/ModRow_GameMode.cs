using System;
using TheForest.Modding.Interfaces;
using UnityEngine;

namespace TheForest.Modding.UI
{
	public class ModRow_GameMode : ModRow
	{
		public GameObject _launchSpButton;

		public GameObject _launchMpButton;

		private IGameModeLauncher _launcher;
	}
}
