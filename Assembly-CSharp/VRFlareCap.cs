using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class VRFlareCap : MonoBehaviour
{
	private void OnDisable()
	{
		this.flarePopped = false;
	}

	private void OnHandHoverBegin(Hand hand)
	{
		if (!this.attached && hand.GetStandardInteractionButton())
		{
			hand.AttachObject(base.gameObject, this.attachmentFlags, this.attachmentPoint);
		}
	}

	private void HandHoverUpdate(Hand hand)
	{
		if (hand.GetStandardInteractionButtonDown())
		{
			hand.AttachObject(base.gameObject, this.attachmentFlags, this.attachmentPoint);
		}
	}

	private void OnAttachedToHand(Hand hand)
	{
		this.attached = true;
		hand.HoverLock(null);
		Rigidbody component = base.GetComponent<Rigidbody>();
		component.isKinematic = true;
		component.interpolation = RigidbodyInterpolation.None;
		this.attachTime = Time.time;
		this.attachPosition = base.transform.position;
		this.attachRotation = base.transform.rotation;
	}

	private void HandAttachedUpdate(Hand hand)
	{
		float num = Vector3.Distance(this.restTransform.position, base.transform.position);
		if (num > this.minPullDistance && !this.flarePopped)
		{
			FMODCommon.PlayOneshot("event:/combat/flare/flare_ignite", base.transform);
			this.SpawnDynamicCap();
			hand.DetachObject(base.gameObject, true);
			this.attached = false;
			hand.HoverUnlock(null);
			this.flarePopped = true;
			this.controller.setLit();
		}
		if (!hand.GetStandardInteractionButton() && !this.flarePopped && num < this.minPullDistance)
		{
			hand.DetachObject(base.gameObject, true);
			this.attached = false;
			hand.HoverUnlock(null);
			this.controller.setUnlit();
		}
	}

	private void SpawnDynamicCap()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.dynamicCap, base.transform.position, base.transform.rotation);
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		component.AddForce(gameObject.transform.up * this.popForce, ForceMode.VelocityChange);
		component.AddTorque(gameObject.transform.right * this.popTorque, ForceMode.VelocityChange);
	}

	[EnumFlags]
	[Tooltip("The flags used to attach this object to the hand.")]
	public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand;

	[Tooltip("Name of the attachment transform under in the hand's hierarchy which the object should should snap to.")]
	public string attachmentPoint;

	private bool attached;

	private float attachTime;

	private Vector3 attachPosition;

	private Quaternion attachRotation;

	public GameObject dynamicCap;

	public VRFlareController controller;

	public Transform restTransform;

	public float minPullDistance = 0.3f;

	public float popForce = 5f;

	public float popTorque = 10f;

	public bool flarePopped;
}
