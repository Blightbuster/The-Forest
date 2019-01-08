using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(CapsuleCollider))]
	public class BodyCollider : MonoBehaviour
	{
		private void Awake()
		{
			this.capsuleCollider = base.GetComponent<CapsuleCollider>();
		}

		private void FixedUpdate()
		{
			float num = Vector3.Dot(this.head.localPosition, Vector3.up);
			this.capsuleCollider.height = Mathf.Max(this.capsuleCollider.radius, num);
			base.transform.localPosition = this.head.localPosition - 0.5f * num * Vector3.up;
		}

		public Transform head;

		private CapsuleCollider capsuleCollider;
	}
}
