using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class SleepOnAwake : MonoBehaviour
	{
		private void Awake()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.Sleep();
			}
		}
	}
}
