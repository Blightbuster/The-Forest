using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class DontDestroyOnLoad : MonoBehaviour
	{
		
		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}
}
