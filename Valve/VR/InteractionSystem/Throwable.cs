using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	
	[RequireComponent(typeof(Interactable))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(VelocityEstimator))]
	public class Throwable : MonoBehaviour
	{
		
		private void Awake()
		{
			this.velocityEstimator = base.GetComponent<VelocityEstimator>();
			if (this.attachEaseIn)
			{
				this.attachmentFlags &= ~Hand.AttachmentFlags.SnapOnAttach;
			}
			Rigidbody component = base.GetComponent<Rigidbody>();
			component.maxAngularVelocity = 50f;
		}

		
		private void OnHandHoverBegin(Hand hand)
		{
			bool flag = false;
			if (!this.attached && hand.GetStandardInteractionButton())
			{
				Rigidbody component = base.GetComponent<Rigidbody>();
				if (component.velocity.magnitude >= this.catchSpeedThreshold)
				{
					hand.AttachObject(base.gameObject, this.attachmentFlags, this.attachmentPoint);
					flag = false;
				}
			}
			if (flag)
			{
				ControllerButtonHints.ShowButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis1
				});
			}
		}

		
		private void OnHandHoverEnd(Hand hand)
		{
			ControllerButtonHints.HideButtonHint(hand, new EVRButtonId[]
			{
				EVRButtonId.k_EButton_Axis1
			});
		}

		
		private void HandHoverUpdate(Hand hand)
		{
			if (hand.GetStandardInteractionButtonDown())
			{
				hand.AttachObject(base.gameObject, this.attachmentFlags, this.attachmentPoint);
				ControllerButtonHints.HideButtonHint(hand, new EVRButtonId[]
				{
					EVRButtonId.k_EButton_Axis1
				});
			}
		}

		
		private void OnAttachedToHand(Hand hand)
		{
			this.attached = true;
			this.onPickUp.Invoke();
			hand.HoverLock(null);
			Rigidbody component = base.GetComponent<Rigidbody>();
			component.isKinematic = true;
			component.interpolation = RigidbodyInterpolation.None;
			if (hand.controller == null)
			{
				this.velocityEstimator.BeginEstimatingVelocity();
			}
			this.attachTime = Time.time;
			this.attachPosition = base.transform.position;
			this.attachRotation = base.transform.rotation;
			if (this.attachEaseIn)
			{
				this.attachEaseInTransform = hand.transform;
				if (!Util.IsNullOrEmpty<string>(this.attachEaseInAttachmentNames))
				{
					float num = float.MaxValue;
					for (int i = 0; i < this.attachEaseInAttachmentNames.Length; i++)
					{
						Transform attachmentTransform = hand.GetAttachmentTransform(this.attachEaseInAttachmentNames[i]);
						float num2 = Quaternion.Angle(attachmentTransform.rotation, this.attachRotation);
						if (num2 < num)
						{
							this.attachEaseInTransform = attachmentTransform;
							num = num2;
						}
					}
				}
			}
			this.snapAttachEaseInCompleted = false;
		}

		
		private void OnDetachedFromHand(Hand hand)
		{
			this.attached = false;
			this.onDetachFromHand.Invoke();
			hand.HoverUnlock(null);
			Rigidbody component = base.GetComponent<Rigidbody>();
			component.isKinematic = false;
			component.interpolation = RigidbodyInterpolation.Interpolate;
			Vector3 b = Vector3.zero;
			Vector3 a = Vector3.zero;
			Vector3 vector = Vector3.zero;
			if (hand.controller == null)
			{
				this.velocityEstimator.FinishEstimatingVelocity();
				a = this.velocityEstimator.GetVelocityEstimate();
				vector = this.velocityEstimator.GetAngularVelocityEstimate();
				b = this.velocityEstimator.transform.position;
			}
			else
			{
				a = Player.instance.trackingOriginTransform.TransformVector(hand.controller.velocity);
				vector = Player.instance.trackingOriginTransform.TransformVector(hand.controller.angularVelocity);
				b = hand.transform.position;
			}
			Vector3 rhs = base.transform.TransformPoint(component.centerOfMass) - b;
			component.velocity = a + Vector3.Cross(vector, rhs);
			component.angularVelocity = vector;
			float num = Time.fixedDeltaTime + Time.fixedTime - Time.time;
			base.transform.position += num * a;
			float num2 = 57.29578f * vector.magnitude;
			Vector3 normalized = vector.normalized;
			base.transform.rotation *= Quaternion.AngleAxis(num2 * num, normalized);
		}

		
		private void HandAttachedUpdate(Hand hand)
		{
			if (!hand.GetStandardInteractionButton())
			{
				base.StartCoroutine(this.LateDetach(hand));
			}
			if (this.attachEaseIn)
			{
				float num = Util.RemapNumberClamped(Time.time, this.attachTime, this.attachTime + this.snapAttachEaseInTime, 0f, 1f);
				if (num < 1f)
				{
					num = this.snapAttachEaseInCurve.Evaluate(num);
					base.transform.position = Vector3.Lerp(this.attachPosition, this.attachEaseInTransform.position, num);
					base.transform.rotation = Quaternion.Lerp(this.attachRotation, this.attachEaseInTransform.rotation, num);
				}
				else if (!this.snapAttachEaseInCompleted)
				{
					base.gameObject.SendMessage("OnThrowableAttachEaseInCompleted", hand, SendMessageOptions.DontRequireReceiver);
					this.snapAttachEaseInCompleted = true;
				}
			}
		}

		
		private IEnumerator LateDetach(Hand hand)
		{
			yield return new WaitForEndOfFrame();
			hand.DetachObject(base.gameObject, this.restoreOriginalParent);
			yield break;
		}

		
		private void OnHandFocusAcquired(Hand hand)
		{
			base.gameObject.SetActive(true);
			this.velocityEstimator.BeginEstimatingVelocity();
		}

		
		private void OnHandFocusLost(Hand hand)
		{
			base.gameObject.SetActive(false);
			this.velocityEstimator.FinishEstimatingVelocity();
		}

		
		[EnumFlags]
		[Tooltip("The flags used to attach this object to the hand.")]
		public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.ParentToHand;

		
		[Tooltip("Name of the attachment transform under in the hand's hierarchy which the object should should snap to.")]
		public string attachmentPoint;

		
		[Tooltip("How fast must this object be moving to attach due to a trigger hold instead of a trigger press?")]
		public float catchSpeedThreshold;

		
		[Tooltip("When detaching the object, should it return to its original parent?")]
		public bool restoreOriginalParent;

		
		public bool attachEaseIn;

		
		public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		
		public float snapAttachEaseInTime = 0.15f;

		
		public string[] attachEaseInAttachmentNames;

		
		private VelocityEstimator velocityEstimator;

		
		private bool attached;

		
		private float attachTime;

		
		private Vector3 attachPosition;

		
		private Quaternion attachRotation;

		
		private Transform attachEaseInTransform;

		
		public UnityEvent onPickUp;

		
		public UnityEvent onDetachFromHand;

		
		public bool snapAttachEaseInCompleted;
	}
}
