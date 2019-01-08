using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class ArrowHand : MonoBehaviour
	{
		private void Awake()
		{
			this.allowTeleport = base.GetComponent<AllowTeleportWhileAttachedToHand>();
			this.allowTeleport.teleportAllowed = true;
			this.allowTeleport.overrideHoverLock = false;
			this.arrowList = new List<GameObject>();
		}

		private void OnEnable()
		{
			if (this.useCustomScale)
			{
				base.transform.localScale = new Vector3(2f, 2f, 2f);
			}
		}

		private void OnAttachedToHand(Hand attachedHand)
		{
			this.hand = attachedHand;
			this.FindBow();
		}

		private GameObject InstantiateArrow()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.arrowPrefab, this.arrowNockTransform.position, this.arrowNockTransform.rotation);
			gameObject.name = "Bow Arrow";
			gameObject.transform.parent = this.arrowNockTransform;
			Util.ResetTransform(gameObject.transform, true);
			this.arrowList.Add(gameObject);
			while (this.arrowList.Count > this.maxArrowCount)
			{
				GameObject gameObject2 = this.arrowList[0];
				this.arrowList.RemoveAt(0);
				if (gameObject2)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
			}
			return gameObject;
		}

		private void HandAttachedUpdate(Hand hand)
		{
			if (this.bow == null)
			{
				this.FindBow();
			}
			if (this.bow == null)
			{
				return;
			}
			if (this.allowArrowSpawn && this.currentArrow == null)
			{
				this.currentArrow = this.InstantiateArrow();
				this.arrowSpawnSound.Play();
			}
			float num = Vector3.Distance(this.nockPositionTransform.position, this.bow.nockTransform.position);
			if (!this.nocked)
			{
				if (num < this.rotationLerpThreshold)
				{
					float t = Util.RemapNumber(num, this.rotationLerpThreshold, this.lerpCompleteDistance, 0f, 1f);
					this.arrowNockTransform.rotation = Quaternion.Lerp(this.arrowNockTransform.parent.rotation, this.bow.nockRestTransform.rotation, t);
				}
				else
				{
					this.arrowNockTransform.localRotation = Quaternion.identity;
				}
				if (num < this.positionLerpThreshold)
				{
					float num2 = Util.RemapNumber(num, this.positionLerpThreshold, this.lerpCompleteDistance, 0f, 1f);
					num2 = Mathf.Clamp(num2, 0f, 1f);
					this.arrowNockTransform.position = Vector3.Lerp(this.arrowNockTransform.parent.position, this.bow.nockRestTransform.position, num2);
				}
				else
				{
					this.arrowNockTransform.position = this.arrowNockTransform.parent.position;
				}
				if (num < this.lerpCompleteDistance)
				{
					if (!this.arrowLerpComplete)
					{
						this.arrowLerpComplete = true;
						hand.controller.TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
					}
				}
				else if (this.arrowLerpComplete)
				{
					this.arrowLerpComplete = false;
				}
				if (num < this.nockDistance)
				{
					if (!this.inNockRange)
					{
						this.inNockRange = true;
						this.bow.ArrowInPosition();
					}
				}
				else if (this.inNockRange)
				{
					this.inNockRange = false;
				}
				if (num < this.nockDistance && hand.controller.GetPress(8589934592UL) && !this.nocked)
				{
					if (this.currentArrow == null)
					{
						this.currentArrow = this.InstantiateArrow();
					}
					this.nocked = true;
					this.bow.StartNock(this);
					hand.HoverLock(base.GetComponent<Interactable>());
					this.allowTeleport.teleportAllowed = false;
					this.currentArrow.transform.parent = this.bow.nockTransform;
					Util.ResetTransform(this.currentArrow.transform, true);
					Util.ResetTransform(this.arrowNockTransform, true);
				}
			}
			if (this.nocked && (!hand.controller.GetPress(8589934592UL) || hand.controller.GetPressUp(8589934592UL)))
			{
				if (this.bow.pulled)
				{
					this.FireArrow();
				}
				else
				{
					this.arrowNockTransform.rotation = this.currentArrow.transform.rotation;
					this.currentArrow.transform.parent = this.arrowNockTransform;
					Util.ResetTransform(this.currentArrow.transform, true);
					this.nocked = false;
					this.bow.ReleaseNock();
					hand.HoverUnlock(base.GetComponent<Interactable>());
					this.allowTeleport.teleportAllowed = true;
				}
				this.bow.StartRotationLerp();
			}
		}

		private void OnDetachedFromHand(Hand hand)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void FireArrow()
		{
			if (LocalPlayer.Inventory)
			{
				LocalPlayer.Inventory.ReleaseAttack();
			}
			this.currentArrow.transform.parent = null;
			if (ForestVR.Enabled)
			{
				if (this.currentArrow != null)
				{
					UnityEngine.Object.Destroy(this.currentArrow);
				}
				this.nocked = false;
				this.bow.ArrowReleased();
				this.currentArrow = null;
				this.allowArrowSpawn = false;
				base.Invoke("EnableArrowSpawn", 0.5f);
				base.StartCoroutine(this.ArrowReleaseHaptics());
				return;
			}
			Arrow component = this.currentArrow.GetComponent<Arrow>();
			component.shaftRB.isKinematic = false;
			component.shaftRB.useGravity = true;
			component.shaftRB.transform.GetComponent<BoxCollider>().enabled = true;
			component.arrowHeadRB.isKinematic = false;
			component.arrowHeadRB.useGravity = true;
			component.arrowHeadRB.transform.GetComponent<BoxCollider>().enabled = true;
			component.arrowHeadRB.AddForce(this.currentArrow.transform.forward * this.bow.GetArrowVelocity(), ForceMode.VelocityChange);
			component.arrowHeadRB.AddTorque(this.currentArrow.transform.forward * 10f);
			this.nocked = false;
			this.currentArrow.GetComponent<Arrow>().ArrowReleased(this.bow.GetArrowVelocity());
			this.bow.ArrowReleased();
			this.allowArrowSpawn = false;
			base.Invoke("EnableArrowSpawn", 0.5f);
			base.StartCoroutine(this.ArrowReleaseHaptics());
			this.currentArrow = null;
			this.allowTeleport.teleportAllowed = true;
		}

		private void EnableArrowSpawn()
		{
			this.allowArrowSpawn = true;
		}

		private IEnumerator ArrowReleaseHaptics()
		{
			yield return new WaitForSeconds(0.05f);
			this.hand.otherHand.controller.TriggerHapticPulse(1500, EVRButtonId.k_EButton_Axis0);
			yield return new WaitForSeconds(0.05f);
			this.hand.otherHand.controller.TriggerHapticPulse(800, EVRButtonId.k_EButton_Axis0);
			yield return new WaitForSeconds(0.05f);
			this.hand.otherHand.controller.TriggerHapticPulse(500, EVRButtonId.k_EButton_Axis0);
			yield return new WaitForSeconds(0.05f);
			this.hand.otherHand.controller.TriggerHapticPulse(300, EVRButtonId.k_EButton_Axis0);
			yield break;
		}

		private void OnHandFocusLost(Hand hand)
		{
			base.gameObject.SetActive(false);
		}

		private void OnHandFocusAcquired(Hand hand)
		{
			base.gameObject.SetActive(true);
		}

		private void FindBow()
		{
			this.bow = this.hand.otherHand.GetComponentInChildren<Longbow>();
		}

		private Hand hand;

		private Longbow bow;

		public GameObject currentArrow;

		public GameObject arrowPrefab;

		public Transform arrowNockTransform;

		public Transform arrowFollowTransform;

		public Transform nockPositionTransform;

		public float nockDistance = 0.1f;

		public float lerpCompleteDistance = 0.08f;

		public float rotationLerpThreshold = 0.15f;

		public float positionLerpThreshold = 0.15f;

		private bool allowArrowSpawn = true;

		private bool nocked;

		private bool inNockRange;

		private bool arrowLerpComplete;

		public SoundPlayOneshot arrowSpawnSound;

		private AllowTeleportWhileAttachedToHand allowTeleport;

		public int maxArrowCount = 10;

		private List<GameObject> arrowList;

		public bool useCustomScale;
	}
}
