using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class DontDestroyOnLoad : MonoBehaviour
	{
		private void Awake()
		{
			if (base.enabled)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		private void Update()
		{
		}
	}
}
