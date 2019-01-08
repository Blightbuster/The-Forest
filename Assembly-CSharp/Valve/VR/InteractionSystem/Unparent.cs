using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class Unparent : MonoBehaviour
	{
		private void Start()
		{
			this.oldParent = base.transform.parent;
			base.transform.parent = null;
			base.gameObject.name = this.oldParent.gameObject.name + "." + base.gameObject.name;
		}

		private void Update()
		{
			if (this.oldParent == null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public Transform GetOldParent()
		{
			return this.oldParent;
		}

		private Transform oldParent;
	}
}
