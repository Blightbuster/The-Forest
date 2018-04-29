using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class ArrowheadRotation : MonoBehaviour
	{
		
		private void Start()
		{
			float x = UnityEngine.Random.Range(0f, 180f);
			base.transform.localEulerAngles = new Vector3(x, -90f, 90f);
		}
	}
}
