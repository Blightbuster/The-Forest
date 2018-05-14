using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerGirlTransformAction : MonoBehaviour
	{
		
		public IEnumerator doGirlTransformRoutine(Transform mark)
		{
			if (!this.spectator)
			{
				setupEndBoss setupEndBoss = setupEndBoss.Create(GlobalTargets.Everyone);
				setupEndBoss.disableBossTrigger = true;
				setupEndBoss.Send();
			}
			Vector3 fixLocalPos = new Vector3(0f, -2.344841f, 0f);
			yield return new WaitForSeconds(0.5f);
			if (!this.spectator)
			{
				LocalPlayer.PlayerBase.SendMessage("loadCustomAnimation", "girlTransformReaction", SendMessageOptions.DontRequireReceiver);
				while (LocalPlayer.AnimControl.loadingAnimation)
				{
					yield return null;
				}
				this.ActorAnimator = LocalPlayer.Animator;
				LocalPlayer.Inventory.Close();
				LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.Chest, false, true, false);
				LocalPlayer.FpCharacter.allowFallDamage = false;
				LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(false);
				LocalPlayer.AnimControl.endGameCutScene = true;
				LocalPlayer.vrPlayerControl.useGhostMode = true;
				LocalPlayer.ScriptSetup.pmControl.enabled = false;
				LocalPlayer.FpCharacter.Locked = true;
				LocalPlayer.FpCharacter.CanJump = false;
				LocalPlayer.Create.Grabber.gameObject.SetActive(false);
				LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
				LocalPlayer.Animator.SetBool("onHand", false);
				LocalPlayer.Animator.SetBool("jumpBool", false);
				LocalPlayer.Inventory.HideAllEquiped(false, false);
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
				LocalPlayer.FpCharacter.enabled = false;
				LocalPlayer.CamFollowHead.smoothLock = true;
				LocalPlayer.CamFollowHead.lockYCam = true;
				LocalPlayer.AnimControl.lockGravity = true;
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
				LocalPlayer.Animator.SetBool("girlTransform", true);
				LocalPlayer.HitReactions.disableControllerFreeze();
				LocalPlayer.Animator.CrossFade("Base Layer.idle", 0f, 0, 0f);
				LocalPlayer.Animator.CrossFade("upperBody.idle", 0f, 1, 0f);
				LocalPlayer.Animator.CrossFade("fullBodyActions.idle", 0f, 2, 0f);
			}
			Vector3 playerPos = mark.position;
			playerPos.y += 2.35f;
			float t = 0f;
			while (t < 1f)
			{
				if (!this.spectator)
				{
					LocalPlayer.Transform.position = Vector3.Slerp(LocalPlayer.Transform.position, playerPos, t);
					LocalPlayer.Transform.rotation = Quaternion.Slerp(LocalPlayer.Transform.rotation, mark.rotation, t);
					LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
				}
				t += Time.deltaTime;
				yield return null;
			}
			if (!this.spectator)
			{
				LocalPlayer.Transform.position = playerPos;
				LocalPlayer.Transform.rotation = mark.rotation;
				LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
				LocalPlayer.AnimControl.useRootMotion = true;
			}
			if (this.girlAnimator)
			{
				this.girlAnimator.transform.parent.localScale = new Vector3(1f, 1f, 1f);
			}
			this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
			while (!this.currState2.IsName("fullBodyActions.girlTransformReaction"))
			{
				this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
				if (!this.spectator)
				{
					LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
					LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
					LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
					LocalPlayer.Transform.position = playerPos;
					LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
					this.lockPlayerParams();
				}
				yield return null;
			}
			if (!this.spectator)
			{
				LocalPlayer.Animator.SetBool("girlTransform", false);
			}
			this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
			this.girlAnimator.CrossFade("Base Layer.transform", 0.2f, 0, this.currState2.normalizedTime);
			bool doScaleHack = false;
			while (this.currState2.IsName("fullBodyActions.girlTransformReaction"))
			{
				if (!this.spectator)
				{
					this.lockPlayerParams();
					LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
					LocalPlayer.HitReactions.disableControllerFreeze();
					LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
					LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
					LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
					LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
				}
				if (this.currState2.normalizedTime > 0.92f && !doScaleHack)
				{
					base.StartCoroutine(this.scaleGirlMutant(1.25f));
					doScaleHack = true;
					if (BoltNetwork.isClient && !this.spectator)
					{
						setupEndBoss setupEndBoss2 = setupEndBoss.Create(GlobalTargets.OnlyServer);
						setupEndBoss2.target = this.girlAnimator.transform.parent.GetComponent<BoltEntity>();
						setupEndBoss2.scaleHack = true;
						setupEndBoss2.Send();
					}
				}
				this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
				yield return null;
			}
			if (!BoltNetwork.isClient)
			{
				creepyAnimatorControl creepyAnimatorControl = this.girlAnimator.transform.parent.GetComponentsInChildren<creepyAnimatorControl>(true)[0];
				creepyAnimatorControl.enabled = true;
				creepyAnimatorControl.activateGirlMutant();
			}
			if (BoltNetwork.isClient && !this.spectator)
			{
				setupEndBoss setupEndBoss3 = setupEndBoss.Create(GlobalTargets.OnlyServer);
				setupEndBoss3.target = this.girlAnimator.transform.parent.GetComponent<BoltEntity>();
				setupEndBoss3.activateBoss = true;
				setupEndBoss3.Send();
			}
			if (!this.spectator)
			{
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.AnimControl.playerCollider.enabled = true;
				LocalPlayer.AnimControl.lockGravity = false;
			}
			yield return null;
			if (!this.spectator)
			{
				LocalPlayer.PlayerBase.SendMessage("unloadCustomAnimation", "girlTransformReaction", SendMessageOptions.DontRequireReceiver);
				while (LocalPlayer.AnimControl.loadingAnimation)
				{
					yield return null;
				}
				this.unlockPlayerParams();
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				LocalPlayer.FpCharacter.enabled = true;
				LocalPlayer.ScriptSetup.pmControl.enabled = true;
				LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(true);
				LocalPlayer.AnimControl.endGameCutScene = false;
				LocalPlayer.vrPlayerControl.useGhostMode = false;
				LocalPlayer.AnimControl.skinningAnimal = false;
				LocalPlayer.AnimControl.useRootMotion = false;
				LocalPlayer.AnimControl.useRootRotation = false;
				LocalPlayer.Create.Grabber.gameObject.SetActive(true);
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.AnimControl.playerCollider.enabled = true;
				LocalPlayer.CamFollowHead.lockYCam = false;
				LocalPlayer.CamFollowHead.smoothLock = false;
				LocalPlayer.CamRotator.resetOriginalRotation = true;
				LocalPlayer.CamRotator.stopInput = false;
				LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
				LocalPlayer.FpCharacter.Locked = false;
				LocalPlayer.FpCharacter.drinking = false;
				LocalPlayer.FpCharacter.CanJump = true;
				LocalPlayer.Rigidbody.isKinematic = false;
				LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
				LocalPlayer.Inventory.ShowAllEquiped(true);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
				if (LocalPlayer.ScriptSetup.events.toyHeld)
				{
					LocalPlayer.ScriptSetup.events.toyHeld.SetActive(false);
				}
				LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
				LocalPlayer.MainRotator.resetOriginalRotation = true;
				LocalPlayer.MainRotator.enabled = true;
				float timer = 0f;
				while (timer < 1f)
				{
					LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
					timer += Time.deltaTime * 3f;
					yield return null;
				}
			}
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
			yield break;
		}

		
		private void setGirlAnimator(Animator a)
		{
			this.girlAnimator = a;
		}

		
		private IEnumerator scaleGirlMutant(float amount)
		{
			float timer = 0f;
			Vector3 newScale = new Vector3(amount, amount, amount);
			while (timer < 1f)
			{
				timer += Time.deltaTime / 3f;
				this.girlAnimator.transform.parent.localScale = Vector3.Slerp(this.girlAnimator.transform.localScale, newScale, timer);
				yield return null;
			}
			yield return null;
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

		
		private void setCurrentSequence(AnimationSequence sequence)
		{
			this.currentSequence = sequence;
		}

		
		private void setActorAnimator(Animator animator)
		{
			this.ActorAnimator = animator;
		}

		
		private void setSpectator(bool isSpectator)
		{
			this.spectator = isSpectator;
		}

		
		private void lockPlayerParams()
		{
			LocalPlayer.FpCharacter.allowFallDamage = false;
			LocalPlayer.FpCharacter.Locked = true;
			LocalPlayer.FpCharacter.CanJump = false;
			LocalPlayer.AnimControl.endGameCutScene = true;
			LocalPlayer.vrPlayerControl.useGhostMode = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.Rigidbody.isKinematic = true;
		}

		
		private void unlockPlayerParams()
		{
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
			LocalPlayer.FpCharacter.allowFallDamage = true;
			LocalPlayer.FpCharacter.Locked = false;
			LocalPlayer.FpCharacter.CanJump = true;
			LocalPlayer.AnimControl.useRootMotion = true;
			LocalPlayer.AnimControl.endGameCutScene = false;
			LocalPlayer.vrPlayerControl.useGhostMode = false;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
			LocalPlayer.FpCharacter.enabled = true;
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.Rigidbody.isKinematic = false;
		}

		
		private Animator ActorAnimator;

		
		private AnimatorStateInfo currState2;

		
		private Quaternion desiredRotation;

		
		private GameObject placedToyGo;

		
		public Animator girlAnimator;

		
		private bool skipScene;

		
		private int girlTransformHash = Animator.StringToHash("transform");

		
		private bool spectator;

		
		private AnimationSequence currentSequence;
	}
}
