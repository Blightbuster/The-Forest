using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	
	[RequireComponent(typeof(Interactable))]
	public class ComplexThrowable : MonoBehaviour
	{
		
		private void Awake()
		{
			base.GetComponentsInChildren<Rigidbody>(this.rigidBodies);
		}

		
		private void Update()
		{
			for (int i = 0; i < this.holdingHands.Count; i++)
			{
				if (!this.holdingHands[i].GetStandardInteractionButton())
				{
					this.PhysicsDetach(this.holdingHands[i]);
				}
			}
		}

		
		private void OnHandHoverBegin(Hand hand)
		{
			if (this.holdingHands.IndexOf(hand) == -1 && hand.controller != null)
			{
				hand.controller.TriggerHapticPulse(800, EVRButtonId.k_EButton_Axis0);
			}
		}

		
		private void OnHandHoverEnd(Hand hand)
		{
			if (this.holdingHands.IndexOf(hand) == -1 && hand.controller != null)
			{
				hand.controller.TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
			}
		}

		
		private void HandHoverUpdate(Hand hand)
		{
			if (hand.GetStandardInteractionButtonDown())
			{
				this.PhysicsAttach(hand);
			}
		}

		
		private void PhysicsAttach(Hand hand)
		{
			this.PhysicsDetach(hand);
			Rigidbody rigidbody = null;
			Vector3 item = Vector3.zero;
			float num = float.MaxValue;
			for (int i = 0; i < this.rigidBodies.Count; i++)
			{
				float num2 = Vector3.Distance(this.rigidBodies[i].worldCenterOfMass, hand.transform.position);
				if (num2 < num)
				{
					rigidbody = this.rigidBodies[i];
					num = num2;
				}
			}
			if (rigidbody == null)
			{
				return;
			}
			if (this.attachMode == ComplexThrowable.AttachMode.FixedJoint)
			{
				Rigidbody rigidbody2 = Util.FindOrAddComponent<Rigidbody>(hand.gameObject);
				rigidbody2.isKinematic = true;
				FixedJoint fixedJoint = hand.gameObject.AddComponent<FixedJoint>();
				fixedJoint.connectedBody = rigidbody;
			}
			hand.HoverLock(null);
			Vector3 b = hand.transform.position - rigidbody.worldCenterOfMass;
			b = Mathf.Min(b.magnitude, 1f) * b.normalized;
			item = rigidbody.transform.InverseTransformPoint(rigidbody.worldCenterOfMass + b);
			hand.AttachObject(base.gameObject, this.attachmentFlags, string.Empty);
			this.holdingHands.Add(hand);
			this.holdingBodies.Add(rigidbody);
			this.holdingPoints.Add(item);
		}

		
		private bool PhysicsDetach(Hand hand)
		{
			int num = this.holdingHands.IndexOf(hand);
			if (num != -1)
			{
				this.holdingHands[num].DetachObject(base.gameObject, false);
				this.holdingHands[num].HoverUnlock(null);
				if (this.attachMode == ComplexThrowable.AttachMode.FixedJoint)
				{
					UnityEngine.Object.Destroy(this.holdingHands[num].GetComponent<FixedJoint>());
				}
				Util.FastRemove<Hand>(this.holdingHands, num);
				Util.FastRemove<Rigidbody>(this.holdingBodies, num);
				Util.FastRemove<Vector3>(this.holdingPoints, num);
				return true;
			}
			return false;
		}

		
		private void FixedUpdate()
		{
			if (this.attachMode == ComplexThrowable.AttachMode.Force)
			{
				for (int i = 0; i < this.holdingHands.Count; i++)
				{
					Vector3 vector = this.holdingBodies[i].transform.TransformPoint(this.holdingPoints[i]);
					Vector3 a = this.holdingHands[i].transform.position - vector;
					this.holdingBodies[i].AddForceAtPosition(this.attachForce * a, vector, ForceMode.Acceleration);
					this.holdingBodies[i].AddForceAtPosition(-this.attachForceDamper * this.holdingBodies[i].GetPointVelocity(vector), vector, ForceMode.Acceleration);
				}
			}
		}

		
		public float attachForce = 800f;

		
		public float attachForceDamper = 25f;

		
		public ComplexThrowable.AttachMode attachMode;

		
		[EnumFlags]
		public Hand.AttachmentFlags attachmentFlags;

		
		private List<Hand> holdingHands = new List<Hand>();

		
		private List<Rigidbody> holdingBodies = new List<Rigidbody>();

		
		private List<Vector3> holdingPoints = new List<Vector3>();

		
		private List<Rigidbody> rigidBodies = new List<Rigidbody>();

		
		public enum AttachMode
		{
			
			FixedJoint,
			
			Force
		}
	}
}
