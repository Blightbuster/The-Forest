using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class SpawnAndAttachAfterControllerIsTracking : MonoBehaviour
	{
		private void Start()
		{
			this.hand = base.GetComponentInParent<Hand>();
		}

		private void Update()
		{
			if (this.itemPrefab != null && this.hand.controller != null && this.hand.controller.hasTracking)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.itemPrefab);
				gameObject.SetActive(true);
				this.hand.AttachObject(gameObject, Hand.AttachmentFlags.SnapOnAttach | Hand.AttachmentFlags.DetachOthers | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand, string.Empty);
				this.hand.controller.TriggerHapticPulse(800, EVRButtonId.k_EButton_Axis0);
				UnityEngine.Object.Destroy(base.gameObject);
				gameObject.transform.localScale = this.itemPrefab.transform.localScale;
			}
		}

		private Hand hand;

		public GameObject itemPrefab;
	}
}
