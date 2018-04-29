using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class LinearDisplacement : MonoBehaviour
	{
		
		private void Start()
		{
			this.initialPosition = base.transform.localPosition;
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
		}

		
		private void Update()
		{
			if (this.linearMapping)
			{
				base.transform.localPosition = this.initialPosition + this.linearMapping.value * this.displacement;
			}
		}

		
		public Vector3 displacement;

		
		public LinearMapping linearMapping;

		
		private Vector3 initialPosition;
	}
}
