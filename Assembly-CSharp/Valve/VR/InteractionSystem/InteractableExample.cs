using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(Interactable))]
	public class InteractableExample : MonoBehaviour
	{
		private void Awake()
		{
			this.textMesh = base.GetComponentInChildren<TextMesh>();
			this.textMesh.text = "No Hand Hovering";
		}

		private void OnHandHoverBegin(Hand hand)
		{
			this.textMesh.text = "Hovering hand: " + hand.name;
		}

		private void OnHandHoverEnd(Hand hand)
		{
			this.textMesh.text = "No Hand Hovering";
		}

		private void HandHoverUpdate(Hand hand)
		{
			if (hand.GetStandardInteractionButtonDown() || (hand.controller != null && hand.controller.GetPressDown(EVRButtonId.k_EButton_Grip)))
			{
				if (hand.currentAttachedObject != base.gameObject)
				{
					this.oldPosition = base.transform.position;
					this.oldRotation = base.transform.rotation;
					hand.HoverLock(base.GetComponent<Interactable>());
					hand.AttachObject(base.gameObject, this.attachmentFlags, string.Empty);
				}
				else
				{
					hand.DetachObject(base.gameObject, true);
					hand.HoverUnlock(base.GetComponent<Interactable>());
					base.transform.position = this.oldPosition;
					base.transform.rotation = this.oldRotation;
				}
			}
		}

		private void OnAttachedToHand(Hand hand)
		{
			this.textMesh.text = "Attached to hand: " + hand.name;
			this.attachTime = Time.time;
		}

		private void OnDetachedFromHand(Hand hand)
		{
			this.textMesh.text = "Detached from hand: " + hand.name;
		}

		private void HandAttachedUpdate(Hand hand)
		{
			this.textMesh.text = "Attached to hand: " + hand.name + "\nAttached time: " + (Time.time - this.attachTime).ToString("F2");
		}

		private void OnHandFocusAcquired(Hand hand)
		{
		}

		private void OnHandFocusLost(Hand hand)
		{
		}

		private TextMesh textMesh;

		private Vector3 oldPosition;

		private Quaternion oldRotation;

		private float attachTime;

		private Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand;
	}
}
