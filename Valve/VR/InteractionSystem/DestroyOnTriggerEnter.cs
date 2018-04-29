using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	public class DestroyOnTriggerEnter : MonoBehaviour
	{
		
		private void Start()
		{
			if (!string.IsNullOrEmpty(this.tagFilter))
			{
				this.useTag = true;
			}
		}

		
		private void OnTriggerEnter(Collider collider)
		{
			if (!this.useTag || (this.useTag && collider.gameObject.tag == this.tagFilter))
			{
				UnityEngine.Object.Destroy(collider.gameObject.transform.root.gameObject);
			}
		}

		
		public string tagFilter;

		
		private bool useTag;
	}
}
