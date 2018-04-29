using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerSkinAnimalAction : MonoBehaviour
	{
		
		private void Start()
		{
			this.heldMeatGo.SetActive(false);
			this.deerSkinGo.SetActive(false);
			this.rabbitSkinGo.SetActive(false);
			this.lizardSkinGo.SetActive(false);
			this.turtleMeatGo.SetActive(false);
		}

		
		private void setAnimalTransform(Transform tr)
		{
			this.animalTr = tr;
		}

		
		private void setAnimalType(int getType)
		{
			this.animalType = getType;
		}

		
		public IEnumerator skinAnimalRoutine(Vector3 animalPos)
		{
			this.resetRigidBody = false;
			this.otherRb = null;
			if (this.animalTr)
			{
				this.otherRb = this.animalTr.GetComponentInChildren<Rigidbody>();
				if (this.otherRb && this.otherRb.useGravity)
				{
					this.otherRb.isKinematic = true;
					this.otherRb.useGravity = false;
					this.resetRigidBody = true;
				}
			}
			Vector3 startpos = LocalPlayer.Transform.position;
			LocalPlayer.FpCharacter.allowFallDamage = false;
			LocalPlayer.FpCharacter.Locked = true;
			LocalPlayer.AnimControl.skinningAnimal = true;
			LocalPlayer.FpCharacter.CanJump = false;
			LocalPlayer.Create.Grabber.gameObject.SetActive(false);
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.Animator.SetBool("onHand", false);
			bool canEquipLeft = false;
			bool canEquipRight = false;
			if (!LocalPlayer.Inventory.IsLeftHandEmpty())
			{
				if (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, this._walkmanId))
				{
					LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.LeftHand);
					canEquipLeft = true;
				}
				LocalPlayer.Inventory.StashLeftHand();
			}
			if (!LocalPlayer.Inventory.IsRightHandEmpty() && !LocalPlayer.Inventory.RightHand.IsHeldOnly)
			{
				LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
				LocalPlayer.Inventory.StashEquipedWeapon(false);
				canEquipRight = true;
			}
			LocalPlayer.Animator.SetLayerWeightReflected(0, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = true;
			LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 0f);
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.CamRotator.stopInput = true;
			LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
			LocalPlayer.FpCharacter.drinking = true;
			LocalPlayer.CamFollowHead.smoothLock = true;
			LocalPlayer.CamFollowHead.lockYCam = true;
			LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
			Vector3 moveDir = (LocalPlayer.PlayerBase.transform.position - animalPos).normalized;
			Vector3 movePos = animalPos + moveDir * 1.3f;
			movePos.y += 2.3f;
			LocalPlayer.Animator.SetBool("skinAnimal", true);
			if (LocalPlayer.Inventory.Logs.HasLogs)
			{
				LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
				if (LocalPlayer.Inventory.Logs.HasLogs)
				{
					LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
				}
			}
			float timer = 0f;
			while (timer < 1f)
			{
				this.doSmoothLookAt(animalPos, 4f);
				LocalPlayer.Transform.position = Vector3.Slerp(LocalPlayer.Transform.position, movePos, timer);
				timer += Time.deltaTime * 4f;
				yield return null;
			}
			this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			while (!this.currState2.IsName("fullBodyActions.skinAnimal"))
			{
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				this.doSmoothLookAt(animalPos, 4f);
				LocalPlayer.Transform.position = movePos;
				if (this.currState2.IsName("fullBodyActions.skinAnimal"))
				{
					break;
				}
				yield return null;
			}
			LocalPlayer.Animator.SetBool("skinAnimal", false);
			this.planeAxeHeldGo.SetActive(true);
			float resetTimer = 0f;
			Vector3 backVector = startpos - LocalPlayer.Transform.position;
			if (backVector.magnitude > 2f)
			{
				backVector = backVector.normalized * 2f;
			}
			Vector3 backPos = LocalPlayer.Transform.position + backVector;
			LocalPlayer.Transform.position = movePos;
			while (this.currState2.shortNameHash == this.skinAnimalHash)
			{
				this.planeAxeHeldGo.SetActive(true);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				if (this.currState2.normalizedTime < 0.8f)
				{
					LocalPlayer.Transform.position = movePos;
					LocalPlayer.Rigidbody.velocity = Vector3.zero;
				}
				if (this.currState2.normalizedTime > 0.46f)
				{
					this.enablePickupType(true);
					this.planeAxeHeldGo.SendMessage("GotBloody");
				}
				this.doSmoothLookAt(animalPos, 4f);
				if (this.currState2.normalizedTime > 0.8f)
				{
					resetTimer = (this.currState2.normalizedTime - 0.8f) * 10f;
					resetTimer = Mathf.Clamp(resetTimer, 0f, 1f);
					LocalPlayer.Transform.position = Vector3.Lerp(LocalPlayer.Transform.position, backPos, resetTimer);
				}
				if (this.currState2.normalizedTime > 0.9f)
				{
					break;
				}
				yield return null;
			}
			LocalPlayer.Transform.position = backPos;
			this.enablePickupType(false);
			this.planeAxeHeldGo.SetActive(false);
			timer = 0f;
			while (this.currState2.IsName("fullBodyActions.skinAnimal"))
			{
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				if (!this.currState2.IsName("fullBodyActions.skinAnimal"))
				{
					break;
				}
				LocalPlayer.Transform.position = backPos;
				yield return null;
			}
			if (LocalPlayer.AnimControl.skinningAnimal)
			{
				LocalPlayer.Transform.position = backPos;
				LocalPlayer.FpCharacter.enforceHighDrag = true;
				base.StartCoroutine(this.resetPlayerDragParams());
				LocalPlayer.AnimControl.skinningAnimal = false;
				this.planeAxeHeldGo.SetActive(false);
				this.enablePickupType(false);
				LocalPlayer.Create.Grabber.gameObject.SetActive(true);
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.AnimControl.playerCollider.enabled = true;
				LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
				LocalPlayer.CamFollowHead.lockYCam = false;
				LocalPlayer.CamFollowHead.smoothLock = false;
				LocalPlayer.MainRotator.resetOriginalRotation = true;
				LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
				LocalPlayer.MainRotator.enabled = true;
				LocalPlayer.CamRotator.resetOriginalRotation = true;
				LocalPlayer.CamRotator.stopInput = false;
				LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
				LocalPlayer.FpCharacter.Locked = false;
				LocalPlayer.FpCharacter.drinking = false;
				LocalPlayer.FpCharacter.CanJump = true;
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
				if (canEquipLeft)
				{
					LocalPlayer.Inventory.EquipPreviousUtility(false);
				}
				if (canEquipRight)
				{
					LocalPlayer.Inventory.EquipPreviousWeapon(true);
				}
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
				LocalPlayer.Transform.position = backPos;
				if (this.resetRigidBody && this.otherRb)
				{
					this.otherRb.isKinematic = false;
					this.otherRb.useGravity = true;
					this.resetRigidBody = false;
				}
			}
			timer = 0f;
			while (timer < 1f)
			{
				LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
				timer += Time.deltaTime * 3f;
				yield return null;
			}
			yield break;
		}

		
		private void enablePickupType(bool onoff)
		{
			if (onoff)
			{
				if (this.animalType == 0)
				{
					this.heldMeatGo.SetActive(true);
				}
				else if (this.animalType == 1)
				{
					this.lizardSkinGo.SetActive(true);
				}
				else if (this.animalType == 2)
				{
					this.rabbitSkinGo.SetActive(true);
				}
				else if (this.animalType == 3)
				{
					this.deerSkinGo.SetActive(true);
				}
				else if (this.animalType == 4)
				{
					this.turtleMeatGo.SetActive(true);
				}
			}
			else
			{
				this.heldMeatGo.SetActive(false);
				this.lizardSkinGo.SetActive(false);
				this.rabbitSkinGo.SetActive(false);
				this.deerSkinGo.SetActive(false);
				this.turtleMeatGo.SetActive(false);
			}
		}

		
		private IEnumerator forceSkinningReset()
		{
			Debug.Log("doing force reset");
			base.StopCoroutine("skinAnimalRoutine");
			if (LocalPlayer.AnimControl.skinningAnimal)
			{
				if (this.resetRigidBody && this.otherRb)
				{
					this.otherRb.isKinematic = false;
					this.otherRb.useGravity = true;
					this.resetRigidBody = false;
				}
				base.StartCoroutine(this.resetPlayerDragParams());
				LocalPlayer.Animator.SetBool("skinAnimal", false);
				this.planeAxeHeldGo.SetActive(false);
				LocalPlayer.AnimControl.skinningAnimal = false;
				this.enablePickupType(false);
				LocalPlayer.Create.Grabber.gameObject.SetActive(true);
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.AnimControl.playerCollider.enabled = true;
				LocalPlayer.CamFollowHead.lockYCam = false;
				LocalPlayer.CamFollowHead.smoothLock = false;
				LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
				LocalPlayer.MainRotator.enabled = true;
				LocalPlayer.CamRotator.stopInput = false;
				LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
				LocalPlayer.FpCharacter.Locked = false;
				LocalPlayer.FpCharacter.drinking = false;
				LocalPlayer.FpCharacter.CanJump = true;
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
				yield return null;
				LocalPlayer.Inventory.HideAllEquiped(false, false);
				yield return null;
				LocalPlayer.Inventory.ShowAllEquiped(true);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
			}
			yield break;
		}

		
		private IEnumerator resetPlayerDragParams()
		{
			yield return YieldPresets.WaitPointFiveSeconds;
			LocalPlayer.FpCharacter.enforceHighDrag = false;
			yield break;
		}

		
		private void doSmoothLookAt(Vector3 lookAtPos, float speed)
		{
			lookAtPos.y = LocalPlayer.Transform.position.y;
			Vector3 vector = lookAtPos - LocalPlayer.Transform.position;
			Quaternion quaternion = LocalPlayer.Transform.rotation;
			if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
			{
				this.desiredRotation = Quaternion.LookRotation(vector, Vector3.up);
			}
			quaternion = Quaternion.Slerp(quaternion, this.desiredRotation, speed * Time.deltaTime);
			LocalPlayer.Transform.rotation = quaternion;
		}

		
		private AnimatorStateInfo currState2;

		
		public GameObject planeAxeHeldGo;

		
		public GameObject heldMeatGo;

		
		public GameObject deerSkinGo;

		
		public GameObject rabbitSkinGo;

		
		public GameObject lizardSkinGo;

		
		public GameObject turtleMeatGo;

		
		private int animalType;

		
		private Transform animalTr;

		
		private bool resetRigidBody;

		
		private Rigidbody otherRb;

		
		private int skinAnimalHash = Animator.StringToHash("skinAnimal");

		
		private Quaternion desiredRotation;

		
		private List<Quaternion> jointRot = new List<Quaternion>();

		
		private List<Transform> allJoints = new List<Transform>();

		
		[ItemIdPicker]
		public int _walkmanId;
	}
}
