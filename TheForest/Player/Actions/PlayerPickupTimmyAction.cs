using System;
using System.Collections;
using Rewired;
using TheForest.Items;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerPickupTimmyAction : MonoBehaviour
	{
		
		public IEnumerator pickupTimmyRoutine(Transform mark)
		{
			float oldfar = 0f;
			float oldnear = 0f;
			Vector3 fixLocalPos = new Vector3(0f, -2.344841f, 0f);
			if (!this.spectator)
			{
				TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(false, ControllerType.Joystick, "Default");
				this.ActorAnimator = LocalPlayer.Animator;
				this.startedCutScene = true;
				LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(false);
				LocalPlayer.FpCharacter.allowFallDamage = false;
				LocalPlayer.FpCharacter.Locked = true;
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
				LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.Chest, false, true, false);
				Scene.HudGui.ShowHud(false);
				LocalPlayer.FpCharacter.CanJump = false;
				LocalPlayer.AnimControl.endGameCutScene = true;
				LocalPlayer.Create.Grabber.gameObject.SetActive(false);
				LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
				LocalPlayer.Animator.SetBool("onHand", false);
				LocalPlayer.Animator.SetBool("jumpBool", false);
				LocalPlayer.Inventory.HideAllEquiped(false, false);
				LocalPlayer.Inventory.StashLeftHand();
				LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				LocalPlayer.Animator.SetLayerWeightReflected(0, 1f);
				LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
				LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = true;
				LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 0f);
				LocalPlayer.MainRotator.enabled = false;
				LocalPlayer.CamRotator.stopInput = true;
				LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
				LocalPlayer.Stats.CancelInvoke("CheckBlood");
				LocalPlayer.FpCharacter.drinking = true;
				LocalPlayer.FpCharacter.enabled = false;
				LocalPlayer.CamFollowHead.smoothLock = true;
				LocalPlayer.CamFollowHead.lockYCam = true;
				LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
				LocalPlayer.AnimControl.lockGravity = true;
				LocalPlayer.Rigidbody.isKinematic = true;
				LocalPlayer.HitReactions.disableControllerFreeze();
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
				oldfar = LocalPlayer.MainCam.farClipPlane;
				oldnear = LocalPlayer.MainCam.nearClipPlane;
				LocalPlayer.MainCam.farClipPlane = 600f;
				LocalPlayer.MainCam.nearClipPlane = 0.1f;
				LocalPlayer.Animator.CrossFade("Base Layer.idle", 0f, 0, 0f);
				LocalPlayer.Animator.CrossFade("upperBody.idle", 0f, 1, 0f);
				LocalPlayer.Animator.CrossFade("fullBodyActions.idle", 0f, 2, 0f);
			}
			else
			{
				while (!this.ActorAnimator)
				{
					if (this.currentSequence.Proxy.state.Actor)
					{
						Animator animator = this.currentSequence.Proxy.state.Actor.GetComponentInChildren<Animator>();
						this.ActorAnimator = animator;
					}
					yield return null;
				}
			}
			float timer = 0f;
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
				LocalPlayer.PlayerBase.SendMessage("loadCustomAnimation", "putTimmyOnMachine", SendMessageOptions.DontRequireReceiver);
				while (LocalPlayer.AnimControl.loadingAnimation)
				{
					yield return null;
				}
				LocalPlayer.Transform.position = playerPos;
				LocalPlayer.Transform.rotation = mark.rotation;
				float localY = LocalPlayer.Transform.localEulerAngles.y;
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
				LocalPlayer.AnimControl.useRootMotion = true;
				LocalPlayer.Animator.SetBool("pickupTimmy", true);
			}
			this.machineAnimator.enabled = true;
			this.cablesAnimator.enabled = true;
			this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
			while (!this.currState2.IsName("fullBodyActions.putTimmyOnMachine"))
			{
				this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
				if (!this.spectator)
				{
					LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
					LocalPlayer.Transform.position = playerPos;
					LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
					this.lockPlayerParams();
				}
				if (this.currState2.IsName("fullBodyActions.putTimmyOnMachine"))
				{
					break;
				}
				yield return null;
			}
			LocalPlayer.Animator.SetBool("pickupTimmy", false);
			if (this.spectator && this.startedCutScene)
			{
				base.StartCoroutine(this.spectatorReset());
			}
			this.cablesAnimator.CrossFade(this.cablesHash, 0f, 0, this.currState2.normalizedTime);
			Animator timmyAnim = this.timmyGo.GetComponent<Animator>();
			timmyAnim.enabled = true;
			this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
			AnimatorStateInfo timmyState = timmyAnim.GetCurrentAnimatorStateInfo(0);
			timmyAnim.CrossFade("Base Layer.pickupToMachine", 0f, 0, this.currState2.normalizedTime);
			Quaternion lastPlayerAngle = Quaternion.identity;
			bool syncCables = false;
			bool syncTimmy = false;
			if (!this.spectator)
			{
				LocalPlayer.Inventory.StashLeftHand();
				if (LocalPlayer.ScriptSetup.leftHandHeld)
				{
					LocalPlayer.ScriptSetup.leftHandHeld.gameObject.SetActive(false);
				}
			}
			while (this.currState2.IsName("fullBodyActions.putTimmyOnMachine"))
			{
				if (!this.spectator)
				{
					if (this.currState2.normalizedTime < 0.07f)
					{
						LocalPlayer.Rigidbody.velocity = Vector3.zero;
						LocalPlayer.Transform.position = playerPos;
						LocalPlayer.Transform.rotation = mark.rotation;
					}
					LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
					LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
					LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
					this.lockPlayerParams();
					LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				}
				this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
				timmyState = timmyAnim.GetCurrentAnimatorStateInfo(0);
				if (this.currState2.normalizedTime > 0.2f && !syncTimmy)
				{
					timmyAnim.CrossFade(this.timmyPickupHash, 0f, 0, this.currState2.normalizedTime);
					syncTimmy = true;
				}
				if (this.currState2.normalizedTime > 0.585f && !syncCables)
				{
					this.cablesAnimator.CrossFade(this.cablesHash, 0f, 0, this.currState2.normalizedTime);
					syncCables = true;
				}
				if (!this.spectator)
				{
				}
				yield return null;
			}
			if (!this.spectator)
			{
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.AnimControl.playerCollider.enabled = true;
				LocalPlayer.AnimControl.lockGravity = false;
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
			}
			yield return null;
			if (!this.spectator)
			{
				LocalPlayer.PlayerBase.SendMessage("unloadCustomAnimation", "putTimmyOnMachine", SendMessageOptions.DontRequireReceiver);
				while (LocalPlayer.AnimControl.loadingAnimation)
				{
					yield return null;
				}
				if (LocalPlayer.ScriptSetup.leftHandHeld)
				{
					LocalPlayer.ScriptSetup.leftHandHeld.gameObject.SetActive(true);
				}
				LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(true);
				this.startedCutScene = false;
				this.unlockPlayerParams();
				LocalPlayer.AnimControl.endGameCutScene = false;
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
				LocalPlayer.FpCharacter.enabled = true;
				LocalPlayer.FpCharacter.Locked = false;
				LocalPlayer.FpCharacter.drinking = false;
				LocalPlayer.FpCharacter.CanJump = true;
				LocalPlayer.Rigidbody.isKinematic = false;
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				LocalPlayer.Inventory.ShowAllEquiped(true);
				Scene.HudGui.ShowHud(true);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
				if (LocalPlayer.ScriptSetup.events.toyHeld)
				{
					LocalPlayer.ScriptSetup.events.toyHeld.SetActive(false);
				}
				LocalPlayer.MainCam.farClipPlane = oldfar;
				LocalPlayer.MainCam.nearClipPlane = oldnear;
				LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
				LocalPlayer.MainRotator.resetOriginalRotation = true;
				LocalPlayer.MainRotator.enabled = true;
				this.currentSequence.CompleteStage(0);
			}
			this.girlTriggerGo.SetActive(true);
			timer = 0f;
			while (timer < 1f)
			{
				if (!this.spectator)
				{
					LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
				}
				timer += Time.deltaTime * 3f;
				yield return null;
			}
			yield return YieldPresets.WaitTwoSeconds;
			EventRegistry.Player.Publish(TfEvent.StoryProgress, GameStats.StoryElements.TimmyFound);
			yield break;
		}

		
		private void finalizePickupTimmy()
		{
		}

		
		private void setToy(GameObject toy)
		{
			this.placedToyGo = toy;
		}

		
		private void setArtifactAudioState(ArtifactAudioState audioState)
		{
			this.artifactAudioState = audioState;
		}

		
		private void sendCablesAnimator(Animator avatar)
		{
			this.cablesAnimator = avatar;
		}

		
		private void sendMachineAnimator(Animator avatar)
		{
			this.machineAnimator = avatar;
		}

		
		private void setGirlTrigger(GameObject go)
		{
			this.girlTriggerGo = go;
		}

		
		private void switchTimmyToy(GameObject heldToy)
		{
			this.placedToyGo.SetActive(true);
			this.placedToyGo.transform.localScale = heldToy.transform.localScale;
			this.placedToyGo.transform.position = heldToy.transform.position;
			this.placedToyGo.transform.rotation = heldToy.transform.rotation;
			heldToy.SetActive(false);
		}

		
		private void timmyRemovedFromArtifact()
		{
			this.artifactAudioState.SetBodyPresent(false);
		}

		
		private void allTubesConnected()
		{
			this.artifactAudioState.SetWorking(true);
		}

		
		private IEnumerator forceCarryReset()
		{
			base.StopCoroutine("pickupTimmyRoutine");
			if (LocalPlayer.AnimControl.skinningAnimal)
			{
				LocalPlayer.Animator.SetBool("pickupTimmy", false);
				LocalPlayer.AnimControl.skinningAnimal = false;
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
				LocalPlayer.Rigidbody.isKinematic = false;
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
				yield return null;
				LocalPlayer.Inventory.HideAllEquiped(false, false);
				yield return null;
				LocalPlayer.Inventory.ShowAllEquiped(true);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
			}
			yield break;
		}

		
		private IEnumerator spectatorReset()
		{
			if (this.startedCutScene)
			{
				LocalPlayer.Animator.SetBool("pickupTimmy", false);
				LocalPlayer.AnimControl.skinningAnimal = false;
				TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Default");
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
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
				yield return null;
				LocalPlayer.Inventory.HideAllEquiped(false, false);
				yield return null;
				LocalPlayer.Inventory.ShowAllEquiped(true);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
			}
			yield break;
		}

		
		private void sendTimmyPickup(GameObject go)
		{
			this.timmyGo = go;
		}

		
		private void setScreensGo(GameObject go)
		{
			this.screensGo = go;
		}

		
		private void activateNextScreen()
		{
			if (this.screensGo == null)
			{
				return;
			}
			if (this.currentSequence)
			{
				this.currentSequence.TickProgressStage();
			}
			foreach (object obj in this.screensGo.transform)
			{
				Transform transform = (Transform)obj;
				transform.SendMessage("CheckNextImage", SendMessageOptions.DontRequireReceiver);
			}
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
			LocalPlayer.Rigidbody.velocity = Vector3.zero;
			LocalPlayer.Rigidbody.angularVelocity = Vector3.zero;
			LocalPlayer.FpCharacter.allowFallDamage = false;
			LocalPlayer.FpCharacter.Locked = true;
			LocalPlayer.FpCharacter.CanJump = false;
			LocalPlayer.AnimControl.endGameCutScene = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.Rigidbody.isKinematic = true;
		}

		
		private void unlockPlayerParams()
		{
			TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Default");
			LocalPlayer.FpCharacter.allowFallDamage = true;
			LocalPlayer.FpCharacter.Locked = false;
			LocalPlayer.FpCharacter.CanJump = true;
			LocalPlayer.AnimControl.endGameCutScene = false;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
			LocalPlayer.FpCharacter.enabled = true;
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.Rigidbody.isKinematic = false;
		}

		
		private Animator ActorAnimator;

		
		private AnimatorStateInfo currState2;

		
		private GameObject timmyGo;

		
		private Quaternion desiredRotation;

		
		public float offsetPlayerRotation;

		
		private GameObject placedToyGo;

		
		private GameObject screensGo;

		
		private GameObject girlTriggerGo;

		
		private ArtifactAudioState artifactAudioState;

		
		public Animator cablesAnimator;

		
		public Animator machineAnimator;

		
		private int cablesHash = Animator.StringToHash("cableSequence");

		
		private int toDoorHash = Animator.StringToHash("turnToDoor");

		
		private int timmyOnMachineHash = Animator.StringToHash("putTimmyOnMachine");

		
		private int timmyPickupHash = Animator.StringToHash("pickupToMachine");

		
		private bool spectator;

		
		private bool startedCutScene;

		
		private AnimationSequence currentSequence;

		
		private Vector3 currPos;
	}
}
