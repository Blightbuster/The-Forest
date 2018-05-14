using System;
using System.Collections;
using Pathfinding;
using TheForest.Commons.Enums;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerEndCrashAction : MonoBehaviour
	{
		
		private void Awake()
		{
		}

		
		private void Start()
		{
			this.overrideController = (LocalPlayer.Animator.runtimeAnimatorController as AnimatorOverrideController);
		}

		
		public IEnumerator doEndPlaneCrashRoutine(Transform mark)
		{
			LocalPlayer.PlayerBase.SendMessage("loadCustomAnimation", "operatePanel", SendMessageOptions.DontRequireReceiver);
			while (LocalPlayer.AnimControl.loadingAnimation)
			{
				yield return null;
			}
			Vector3 fixLocalPos = new Vector3(0f, -2.344841f, 0f);
			LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
			LocalPlayer.FpCharacter.allowFallDamage = false;
			LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(false);
			LocalPlayer.FpCharacter.Locked = true;
			LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.Chest, false, true, false);
			LocalPlayer.FpCharacter.CanJump = false;
			LocalPlayer.Create.Grabber.gameObject.SetActive(false);
			LocalPlayer.AnimControl.endGameCutScene = true;
			LocalPlayer.vrPlayerControl.useGhostMode = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.Animator.SetBool("onHand", false);
			LocalPlayer.Rigidbody.interpolation = RigidbodyInterpolation.None;
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
			LocalPlayer.CamFollowHead.smoothLock = true;
			LocalPlayer.CamFollowHead.lockYCam = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.AnimControl.playerCollider.enabled = false;
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
			LocalPlayer.Transform.rotation = mark.rotation;
			LocalPlayer.Animator.CrossFade("Base Layer.idle", 0f, 0, 0f);
			LocalPlayer.Animator.CrossFade("upperBody.idle", 0f, 1, 0f);
			LocalPlayer.Animator.CrossFade("fullBodyActions.idle", 0f, 2, 0f);
			LocalPlayer.Animator.SetBool("operatePanel", true);
			float timer = 0f;
			Vector3 playerPos = mark.position;
			playerPos.y += 2.35f;
			LocalPlayer.Transform.position = playerPos;
			LocalPlayer.Transform.rotation = mark.rotation;
			LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
			LocalPlayer.AnimControl.useRootMotion = true;
			this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			while (!this.currState2.IsName("fullBodyActions.operatePanel"))
			{
				LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				LocalPlayer.Transform.position = playerPos;
				yield return null;
			}
			LocalPlayer.Animator.SetBool("operatePanel", false);
			this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			this.planeAnimator.CrossFade("Base Layer.endSequence", 0f, 0, this.currState2.normalizedTime);
			bool canShowEndgameUI = true;
			bool doArtifactGlow = false;
			bool doFlash = false;
			bool doButton = false;
			while (this.currState2.IsName("fullBodyActions.operatePanel"))
			{
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				this.lockPlayerParams();
				LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
				Vector3 hidePos = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * -100f;
				for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
				{
					if (Scene.SceneTracker.allPlayers[i] != null && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
					{
						Scene.SceneTracker.allPlayers[i].transform.position = hidePos;
					}
				}
				if (this.currState2.normalizedTime > 0.7f && !doButton)
				{
					this.endCrash.activateScreen.SetActive(false);
					doButton = true;
				}
				if (this.currState2.normalizedTime > 0.453f && !doArtifactGlow)
				{
					this.artifactGo.SendMessage("setArtifactOn");
					doArtifactGlow = true;
				}
				if (this.currState2.normalizedTime > 0.537f && !doFlash)
				{
					this.artifactGo.SendMessage("enableFlashEffectGo");
					doFlash = true;
				}
				if (this.currState2.normalizedTime > 0.83f)
				{
					Scene.HudGui.GuiCamC.enabled = false;
					if (canShowEndgameUI)
					{
						canShowEndgameUI = false;
						Scene.HudGui.EndgameScreen.SetActive(true);
					}
				}
				yield return null;
			}
			this.unlockPlayerParams();
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
			LocalPlayer.AnimControl.playerCollider.enabled = true;
			LocalPlayer.AnimControl.lockGravity = false;
			yield return null;
			LocalPlayer.AnimControl.skinningAnimal = false;
			LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(true);
			LocalPlayer.AnimControl.endGameCutScene = false;
			LocalPlayer.vrPlayerControl.useGhostMode = false;
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
			LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
			if (LocalPlayer.ScriptSetup.events.toyHeld)
			{
				LocalPlayer.ScriptSetup.events.toyHeld.SetActive(false);
			}
			LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
			LocalPlayer.MainRotator.resetOriginalRotation = true;
			LocalPlayer.MainRotator.enabled = true;
			timer = 0f;
			while (timer < 1f)
			{
				LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
				timer += Time.deltaTime * 3f;
				yield return null;
			}
			LocalPlayer.Inventory.enabled = false;
			yield return YieldPresets.WaitFiveSeconds;
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Loading;
			MenuMain.exitingToMenu = true;
			WorkScheduler.ClearInstance();
			UniqueIdentifier.AllIdentifiers.Clear();
			RecastMeshObj.Clear();
			GeoHash.ClearAll();
			if (BoltNetwork.isRunning)
			{
				if (CoopLobby.IsInLobby)
				{
					if (CoopLobby.Instance.Info.IsOwner)
					{
						CoopLobby.Instance.Destroy();
					}
					CoopLobby.LeaveActive();
				}
				yield return YieldPresets.WaitPointFiveSeconds;
				CoopSteamServer.Shutdown();
				CoopSteamClient.Shutdown();
				CoopTreeGrid.Clear();
				GameSetup.SetInitType(InitTypes.New);
				GameSetup.SetGameType(GameTypes.Standard);
				BoltLauncher.Shutdown();
			}
			if (LocalPlayer.GameObject)
			{
				Debug.Log("destroy player l210");
				UnityEngine.Object.Destroy(LocalPlayer.GameObject);
			}
			yield break;
		}

		
		public IEnumerator doShutDownRoutine(Transform mark)
		{
			this.overrideController = (LocalPlayer.Animator.runtimeAnimatorController as AnimatorOverrideController);
			ResourceRequest request = Resources.LoadAsync("CutScene/operatePanelExit");
			yield return request;
			AnimationClip animClip = request.asset as AnimationClip;
			this.overrideController["operatePanel Empty"] = animClip;
			LocalPlayer.Animator.Update(0f);
			Vector3 fixLocalPos = new Vector3(0f, -2.344841f, 0f);
			LocalPlayer.Inventory.StashLeftHand();
			LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
			LocalPlayer.FpCharacter.allowFallDamage = false;
			LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(false);
			LocalPlayer.FpCharacter.Locked = true;
			LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.Chest, false, true, false);
			LocalPlayer.FpCharacter.CanJump = false;
			LocalPlayer.Create.Grabber.gameObject.SetActive(false);
			LocalPlayer.AnimControl.endGameCutScene = true;
			LocalPlayer.vrPlayerControl.useGhostMode = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.Animator.SetBool("onHand", false);
			LocalPlayer.Rigidbody.interpolation = RigidbodyInterpolation.None;
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
			LocalPlayer.CamFollowHead.smoothLock = true;
			LocalPlayer.CamFollowHead.lockYCam = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.AnimControl.playerCollider.enabled = false;
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
			LocalPlayer.Transform.rotation = mark.rotation;
			LocalPlayer.Animator.CrossFade("Base Layer.idle", 0f, 0, 0f);
			LocalPlayer.Animator.CrossFade("upperBody.idle", 0f, 1, 0f);
			LocalPlayer.Animator.CrossFade("fullBodyActions.idle", 0f, 2, 0f);
			LocalPlayer.Animator.SetBool("operatePanelExit", true);
			float timer = 0f;
			Vector3 playerPos = mark.position;
			playerPos.y += 2.35f;
			LocalPlayer.Transform.position = playerPos;
			LocalPlayer.Transform.rotation = mark.rotation;
			LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
			LocalPlayer.AnimControl.useRootMotion = true;
			LocalPlayer.AnimControl.useRootRotation = true;
			this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			while (!this.currState2.IsName("fullBodyActions.operatePanelExit"))
			{
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				LocalPlayer.Transform.position = playerPos;
				yield return null;
			}
			LocalPlayer.Animator.SetBool("operatePanelExit", false);
			this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			bool doArtifactGlow = false;
			bool doElevatorOn = false;
			bool switchScreen = false;
			while (this.currState2.IsName("fullBodyActions.operatePanelExit"))
			{
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				this.lockPlayerParams();
				LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
				Vector3 hidePos = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * -100f;
				for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
				{
					if (Scene.SceneTracker.allPlayers[i] != null && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
					{
						Scene.SceneTracker.allPlayers[i].transform.position = hidePos;
					}
				}
				if (this.currState2.normalizedTime > 0.487f && !switchScreen)
				{
					this.endCrash.shutdownActivatedGo.SetActive(true);
					this.endCrash.activateGo.SetActive(false);
					switchScreen = true;
				}
				if (this.currState2.normalizedTime > 0.61f && !doArtifactGlow)
				{
					this.artifactGo.SendMessage("setArtifactOff");
					doArtifactGlow = true;
				}
				if (this.currState2.normalizedTime > 0.919f && !doElevatorOn)
				{
					AutomatedDoorSystem component = this.ElevatorDoorGo.GetComponent<AutomatedDoorSystem>();
					component.enabled = true;
					component.Unlock();
					component.StartOpening();
					doElevatorOn = true;
				}
				yield return null;
			}
			this.unlockPlayerParams();
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
			LocalPlayer.AnimControl.playerCollider.enabled = true;
			LocalPlayer.AnimControl.lockGravity = false;
			yield return null;
			LocalPlayer.AnimControl.skinningAnimal = false;
			LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(true);
			LocalPlayer.AnimControl.endGameCutScene = false;
			LocalPlayer.vrPlayerControl.useGhostMode = false;
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
			LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
			if (LocalPlayer.ScriptSetup.events.toyHeld)
			{
				LocalPlayer.ScriptSetup.events.toyHeld.SetActive(false);
			}
			this.endCrash.shutdownActivatedGo.SetActive(false);
			this.endCrash.RedLight.SetActive(false);
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
			LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
			LocalPlayer.MainRotator.resetOriginalRotation = true;
			LocalPlayer.MainRotator.enabled = true;
			timer = 0f;
			while (timer < 1f)
			{
				LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
				timer += Time.deltaTime * 3f;
				yield return null;
			}
			yield break;
		}

		
		private void setPlaneGo(GameObject go)
		{
			this.planeGo = go;
			this.planeAnimator = go.GetComponent<Animator>();
		}

		
		private void setSecondArtifactGo(GameObject go)
		{
			this.artifactGo = go;
		}

		
		private void setAltAnim(AnimationClip anim)
		{
			this.altOperatePanel = anim;
		}

		
		private void setElevatorDoor(GameObject go)
		{
			this.ElevatorDoorGo = go;
		}

		
		private void setEndCrashScript(activateEndCrash script)
		{
			this.endCrash = script;
		}

		
		private void setActivateScreen(GameObject screen)
		{
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
			LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		}

		
		private void unlockPlayerParams()
		{
			LocalPlayer.FpCharacter.allowFallDamage = true;
			LocalPlayer.FpCharacter.Locked = false;
			LocalPlayer.FpCharacter.CanJump = true;
			LocalPlayer.AnimControl.endGameCutScene = false;
			LocalPlayer.vrPlayerControl.useGhostMode = false;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
			LocalPlayer.FpCharacter.enabled = true;
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.Rigidbody.isKinematic = false;
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		}

		
		private AnimatorStateInfo currState2;

		
		private Quaternion desiredRotation;

		
		public GameObject planeGo;

		
		public GameObject artifactGo;

		
		public GameObject ElevatorDoorGo;

		
		public activateEndCrash endCrash;

		
		private Animator planeAnimator;

		
		private AnimationClip altOperatePanel;

		
		private AnimatorOverrideController overrideController;

		
		private int panelHash = Animator.StringToHash("operatePanel");
	}
}
