using System;
using UnityEngine;

namespace Ceto.Common.Unity.Utility
{
	
	public class Quit : MonoBehaviour
	{
		
		private void OnGUI()
		{
			if (Input.GetKeyDown(this.quitKey))
			{
				Application.Quit();
			}
		}

		
		public KeyCode quitKey = KeyCode.Escape;
	}
}
