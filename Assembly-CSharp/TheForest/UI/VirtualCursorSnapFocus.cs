using System;
using UnityEngine;

namespace TheForest.UI
{
	public class VirtualCursorSnapFocus : MonoBehaviour
	{
		private void OnEnable()
		{
			if (ForestVR.Prototype)
			{
				base.enabled = false;
				return;
			}
			VirtualCursorSnapNode component = base.GetComponent<VirtualCursorSnapNode>();
			component._layer.SetCurrentNode(component);
		}
	}
}
