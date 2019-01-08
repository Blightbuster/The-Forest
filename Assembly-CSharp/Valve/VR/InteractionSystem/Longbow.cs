using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(Interactable))]
	public class Longbow : MonoBehaviour
	{
		private void OnAttachedToHand(Hand attachedHand)
		{
			this.hand = attachedHand;
		}

		private void Awake()
		{
			this.newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(new UnityAction(this.OnNewPosesApplied));
		}

		private void OnEnable()
		{
			this.newPosesAppliedAction.enabled = true;
		}

		private void OnDisable()
		{
			this.newPosesAppliedAction.enabled = false;
		}

		private void LateUpdate()
		{
			if (this.deferNewPoses)
			{
				this.lateUpdatePos = base.transform.position;
				this.lateUpdateRot = base.transform.rotation;
			}
		}

		private void OnNewPosesApplied()
		{
			if (this.deferNewPoses)
			{
				base.transform.position = this.lateUpdatePos;
				base.transform.rotation = this.lateUpdateRot;
				this.deferNewPoses = false;
			}
		}

		private void HandAttachedUpdate(Hand hand)
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			if (this.nocked)
			{
				if (ForestVR.Enabled)
				{
					LocalPlayer.Animator.SetBool("drawBowBool", true);
				}
				this.deferNewPoses = true;
				Vector3 lhs = this.arrowHand.arrowNockTransform.parent.position - this.nockRestTransform.position;
				float num = Util.RemapNumberClamped(Time.time, this.nockLerpStartTime, this.nockLerpStartTime + this.lerpDuration, 0f, 1f);
				float d = Util.RemapNumberClamped(lhs.magnitude, this.minPull, this.maxPull, 0f, 1f);
				Vector3 normalized = (Player.instance.hmdTransform.position + Vector3.down * 0.05f - this.arrowHand.arrowNockTransform.parent.position).normalized;
				Vector3 a = this.arrowHand.arrowNockTransform.parent.position + normalized * this.drawOffset * d;
				Vector3 normalized2 = (a - this.pivotTransform.position).normalized;
				Vector3 normalized3 = (this.handleTransform.position - this.pivotTransform.position).normalized;
				this.bowLeftVector = -Vector3.Cross(normalized3, normalized2);
				this.pivotTransform.rotation = Quaternion.Lerp(this.nockLerpStartRotation, Quaternion.LookRotation(normalized2, this.bowLeftVector), num);
				if (Vector3.Dot(lhs, -this.nockTransform.forward) > 0f)
				{
					float num2 = lhs.magnitude / 3f * num;
					this.nockTransform.localPosition = new Vector3(0f, 0f, Mathf.Clamp(-num2, -this.maxPull, 0f));
					this.nockDistanceTravelled = -this.nockTransform.localPosition.z;
					this.arrowVelocity = Util.RemapNumber(this.nockDistanceTravelled, this.minPull, this.maxPull, this.arrowMinVelocity, this.arrowMaxVelocity);
					this.drawTension = Util.RemapNumberClamped(this.nockDistanceTravelled, 0f, this.maxPull, 0f, 1f);
					this.bowDrawLinearMapping.value = this.drawTension;
					if (this.nockDistanceTravelled > this.minPull)
					{
						this.pulled = true;
					}
					else
					{
						this.pulled = false;
					}
					if (this.nockDistanceTravelled > this.lastTickDistance + this.hapticDistanceThreshold || this.nockDistanceTravelled < this.lastTickDistance - this.hapticDistanceThreshold)
					{
						ushort durationMicroSec = (ushort)Util.RemapNumber(this.nockDistanceTravelled, 0f, this.maxPull, 100f, 500f);
						hand.controller.TriggerHapticPulse(durationMicroSec, EVRButtonId.k_EButton_Axis0);
						hand.otherHand.controller.TriggerHapticPulse(durationMicroSec, EVRButtonId.k_EButton_Axis0);
						this.drawSound.PlayBowTensionClicks(this.drawTension);
						this.lastTickDistance = this.nockDistanceTravelled;
					}
					if (this.nockDistanceTravelled >= this.maxPull && Time.time > this.nextStrainTick)
					{
						hand.controller.TriggerHapticPulse(400, EVRButtonId.k_EButton_Axis0);
						hand.otherHand.controller.TriggerHapticPulse(400, EVRButtonId.k_EButton_Axis0);
						this.drawSound.PlayBowTensionClicks(this.drawTension);
						this.nextStrainTick = Time.time + UnityEngine.Random.Range(this.minStrainTickTime, this.maxStrainTickTime);
					}
				}
				else
				{
					this.nockTransform.localPosition = new Vector3(0f, 0f, 0f);
					this.bowDrawLinearMapping.value = 0f;
				}
			}
			else
			{
				if (ForestVR.Enabled)
				{
					LocalPlayer.Animator.SetBool("drawBowBool", false);
				}
				if (this.lerpBackToZeroRotation)
				{
					float num3 = Util.RemapNumber(Time.time, this.lerpStartTime, this.lerpStartTime + this.lerpDuration, 0f, 1f);
					this.pivotTransform.localRotation = Quaternion.Lerp(this.lerpStartRotation, Quaternion.identity, num3);
					if (num3 >= 1f)
					{
						this.lerpBackToZeroRotation = false;
					}
				}
			}
		}

		public void ArrowReleased()
		{
			if (ForestVR.Enabled)
			{
				LocalPlayer.Animator.SetBool("bowFireBool", true);
			}
			this.nocked = false;
			this.hand.HoverUnlock(base.GetComponent<Interactable>());
			this.hand.otherHand.HoverUnlock(this.arrowHand.GetComponent<Interactable>());
			if (this.releaseSound != null)
			{
				this.releaseSound.Play();
			}
			base.StartCoroutine(this.ResetDrawAnim());
		}

		private IEnumerator ResetDrawAnim()
		{
			float startTime = Time.time;
			float startLerp = this.drawTension;
			while (Time.time < startTime + 0.02f)
			{
				float lerp = Util.RemapNumberClamped(Time.time, startTime, startTime + 0.02f, startLerp, 0f);
				this.bowDrawLinearMapping.value = lerp;
				yield return null;
			}
			if (ForestVR.Enabled)
			{
				LocalPlayer.Animator.SetBool("bowFireBool", false);
			}
			this.bowDrawLinearMapping.value = 0f;
			yield break;
		}

		public float GetArrowVelocity()
		{
			return this.arrowVelocity;
		}

		public void StartRotationLerp()
		{
			this.lerpStartTime = Time.time;
			this.lerpBackToZeroRotation = true;
			this.lerpStartRotation = this.pivotTransform.localRotation;
			Util.ResetTransform(this.nockTransform, true);
		}

		public void StartNock(ArrowHand currentArrowHand)
		{
			this.arrowHand = currentArrowHand;
			this.hand.HoverLock(base.GetComponent<Interactable>());
			this.nocked = true;
			this.nockLerpStartTime = Time.time;
			this.nockLerpStartRotation = this.pivotTransform.rotation;
			this.arrowSlideSound.Play();
			this.DoHandednessCheck();
		}

		private void EvaluateHandedness()
		{
			if (this.hand.GuessCurrentHandType() == Hand.HandType.Left)
			{
				if (this.possibleHandSwitch && this.currentHandGuess == Longbow.Handedness.Left)
				{
					this.possibleHandSwitch = false;
				}
				if (!this.possibleHandSwitch && this.currentHandGuess == Longbow.Handedness.Right)
				{
					this.possibleHandSwitch = true;
					this.timeOfPossibleHandSwitch = Time.time;
				}
				if (this.possibleHandSwitch && Time.time > this.timeOfPossibleHandSwitch + this.timeBeforeConfirmingHandSwitch)
				{
					this.currentHandGuess = Longbow.Handedness.Left;
					this.possibleHandSwitch = false;
				}
			}
			else
			{
				if (this.possibleHandSwitch && this.currentHandGuess == Longbow.Handedness.Right)
				{
					this.possibleHandSwitch = false;
				}
				if (!this.possibleHandSwitch && this.currentHandGuess == Longbow.Handedness.Left)
				{
					this.possibleHandSwitch = true;
					this.timeOfPossibleHandSwitch = Time.time;
				}
				if (this.possibleHandSwitch && Time.time > this.timeOfPossibleHandSwitch + this.timeBeforeConfirmingHandSwitch)
				{
					this.currentHandGuess = Longbow.Handedness.Right;
					this.possibleHandSwitch = false;
				}
			}
		}

		private void DoHandednessCheck()
		{
			if (this.currentHandGuess == Longbow.Handedness.Left)
			{
				this.pivotTransform.localScale = new Vector3(1f, 1f, 1f);
			}
			else
			{
				this.pivotTransform.localScale = new Vector3(1f, -1f, 1f);
			}
		}

		public void ArrowInPosition()
		{
			this.DoHandednessCheck();
			if (this.nockSound != null)
			{
				this.nockSound.Play();
			}
		}

		public void ReleaseNock()
		{
			this.nocked = false;
			this.hand.HoverUnlock(base.GetComponent<Interactable>());
			base.StartCoroutine(this.ResetDrawAnim());
		}

		private void ShutDown()
		{
			if (this.hand != null && this.hand.otherHand.currentAttachedObject != null && this.hand.otherHand.currentAttachedObject.GetComponent<ItemPackageReference>() != null && this.hand.otherHand.currentAttachedObject.GetComponent<ItemPackageReference>().itemPackage == this.arrowHandItemPackage)
			{
				this.hand.otherHand.DetachObject(this.hand.otherHand.currentAttachedObject, true);
			}
		}

		private void OnHandFocusLost(Hand hand)
		{
			base.gameObject.SetActive(false);
		}

		private void OnHandFocusAcquired(Hand hand)
		{
			base.gameObject.SetActive(true);
			this.OnAttachedToHand(hand);
		}

		private void OnDetachedFromHand(Hand hand)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void OnDestroy()
		{
			this.ShutDown();
		}

		public Longbow.Handedness currentHandGuess;

		private float timeOfPossibleHandSwitch;

		private float timeBeforeConfirmingHandSwitch = 1.5f;

		private bool possibleHandSwitch;

		public Transform bowFollowTransform;

		public Transform pivotTransform;

		public Transform handleTransform;

		private Hand hand;

		public ArrowHand arrowHand;

		public Transform nockTransform;

		public Transform nockRestTransform;

		public bool autoSpawnArrowHand = true;

		public ItemPackage arrowHandItemPackage;

		public GameObject arrowHandPrefab;

		public bool nocked;

		public bool pulled;

		public float minPull = 0.05f;

		public float maxPull = 0.5f;

		private float nockDistanceTravelled;

		private float hapticDistanceThreshold = 0.01f;

		private float lastTickDistance;

		private const float bowPullPulseStrengthLow = 100f;

		private const float bowPullPulseStrengthHigh = 500f;

		private Vector3 bowLeftVector;

		public float arrowMinVelocity = 3f;

		public float arrowMaxVelocity = 30f;

		private float arrowVelocity = 30f;

		private float minStrainTickTime = 0.1f;

		private float maxStrainTickTime = 0.5f;

		private float nextStrainTick;

		private bool lerpBackToZeroRotation;

		private float lerpStartTime;

		private float lerpDuration = 0.15f;

		private Quaternion lerpStartRotation;

		private float nockLerpStartTime;

		private Quaternion nockLerpStartRotation;

		public float drawOffset = 0.06f;

		public bool useCustomScale;

		public LinearMapping bowDrawLinearMapping;

		private bool deferNewPoses;

		private Vector3 lateUpdatePos;

		private Quaternion lateUpdateRot;

		public SoundBowClick drawSound;

		private float drawTension;

		public SoundPlayOneshot arrowSlideSound;

		public SoundPlayOneshot releaseSound;

		public SoundPlayOneshot nockSound;

		private SteamVR_Events.Action newPosesAppliedAction;

		public enum Handedness
		{
			Left,
			Right
		}
	}
}
