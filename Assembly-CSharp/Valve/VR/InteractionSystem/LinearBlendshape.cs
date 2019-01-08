using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class LinearBlendshape : MonoBehaviour
	{
		private void Awake()
		{
			if (this.skinnedMesh == null)
			{
				this.skinnedMesh = base.GetComponent<SkinnedMeshRenderer>();
			}
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
		}

		private void Update()
		{
			float value = this.linearMapping.value;
			if (value != this.lastValue)
			{
				float value2 = Util.RemapNumberClamped(value, 0f, 1f, 1f, 100f);
				this.skinnedMesh.SetBlendShapeWeight(0, value2);
			}
			this.lastValue = value;
		}

		public LinearMapping linearMapping;

		public SkinnedMeshRenderer skinnedMesh;

		private float lastValue;
	}
}
