using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerGirlPickupAction : MonoBehaviour
	{
		
		public IEnumerator pickupGirlRoutine(Vector3 pos)
		{
			this.enablePlayerLocked();
			LocalPlayer.Animator.SetBool("girlPickup", true);
			this.girlGo.transform.position = LocalPlayer.PlayerBase.transform.position;
			this.girlGo.transform.rotation = LocalPlayer.PlayerBase.transform.rotation;
			float timer = 0f;
			this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			while (this.currState2.shortNameHash != this.idleToGirlHash)
			{
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				LocalPlayer.Rigidbody.velocity = Vector3.zero;
				this.girlGo.transform.position = LocalPlayer.PlayerBase.transform.position;
				this.girlGo.transform.rotation = LocalPlayer.PlayerBase.transform.rotation;
				if (this.currState2.shortNameHash == this.idleToGirlHash)
				{
					break;
				}
				yield return null;
			}
			Animator girlAnimator = this.girlGo.GetComponentInChildren<Animator>();
			girlAnimator.enabled = true;
			if (BoltNetwork.isRunning)
			{
				syncGirlPickup ev = syncGirlPickup.Create(GlobalTargets.Others);
				ev.playerTarget = base.transform.root.GetComponent<BoltEntity>();
				ev.target = this.girlGo.GetComponent<BoltEntity>();
				ev.syncPickupAnimation = true;
				ev.Send();
			}
			while (this.currState2.shortNameHash == this.idleToGirlHash)
			{
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				girlAnimator.Play("Base Layer.girlPickup", 0, this.currState2.normalizedTime);
				yield return null;
			}
			girlAnimator.CrossFade("Base Layer.girlPickup", 0f, 0, 1f);
			if (this.girlGo)
			{
				this.girlHeld.SetActive(true);
				this.girlHeld.transform.parent = LocalPlayer.ScriptSetup.spine3.transform;
				if (!BoltNetwork.isRunning || this.girlGo.GetComponent<BoltEntity>().isOwner)
				{
					UnityEngine.Object.Destroy(this.girlGo);
				}
				else
				{
					syncGirlPickup destroy = syncGirlPickup.Create(GlobalTargets.Others);
					destroy.target = this.girlGo.GetComponent<BoltEntity>();
					destroy.destroyPickup = true;
					destroy.Send();
					this.girlGo = null;
				}
				this.disablePlayerLocked();
				timer = 0f;
				while (timer < 1f)
				{
					LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
					timer += Time.deltaTime * 3f;
					yield return null;
				}
				while (this.currState2.shortNameHash == this.girlIdleHash)
				{
					LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
					this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.RightHand);
					LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
					if (TheForest.Utils.Input.GetButtonDown("Drop"))
					{
						if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash == this.idleHash && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.onRaft && LocalPlayer.FpCharacter.Grounded && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Book && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory)
						{
							base.StartCoroutine(this.putDownGirlRoutine());
							yield break;
						}
						yield return null;
					}
					yield return null;
				}
			}
			else
			{
				LocalPlayer.Animator.SetBool("girlPickup", false);
				this.disablePlayerLocked();
				LocalPlayer.AnimControl.holdingGirl = false;
				LocalPlayer.Inventory.EquipPreviousWeapon(true);
				LocalPlayer.Inventory.ShowAllEquiped(true);
			}
			yield break;
		}

		
		public IEnumerator putDownGirlRoutine()
		{
			this.enablePlayerLocked();
			LocalPlayer.Animator.SetBool("girlPickup", false);
			this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			while (this.currState2.shortNameHash != this.putDownGirlHash)
			{
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				LocalPlayer.Rigidbody.velocity = Vector3.zero;
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				if (this.currState2.shortNameHash == this.putDownGirlHash)
				{
					break;
				}
				yield return null;
			}
			if (BoltNetwork.isClient)
			{
				syncGirlPickup ev = syncGirlPickup.Create(GlobalTargets.Everyone);
				ev.target = base.transform.root.GetComponent<BoltEntity>();
				ev.playerTarget = base.transform.root.GetComponent<BoltEntity>();
				ev.spawnGirl = true;
				ev.syncPutDownAnimation = true;
				ev.Send();
			}
			else
			{
				CoopSyncGirlPickupToken token = new CoopSyncGirlPickupToken();
				token.putDown = true;
				token.pickup = false;
				token.playerTarget = base.transform.root.GetComponent<BoltEntity>();
				GameObject spawn = null;
				if (BoltNetwork.isRunning)
				{
					spawn = BoltNetwork.Instantiate(Resources.Load("CutScene/girl_Pickup") as GameObject, token, LocalPlayer.PlayerBase.transform.position, LocalPlayer.PlayerBase.transform.rotation).gameObject;
				}
				else
				{
					spawn = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_Pickup"), LocalPlayer.PlayerBase.transform.position, LocalPlayer.PlayerBase.transform.rotation);
				}
				Animator girlAnimator = spawn.GetComponentInChildren<Animator>();
				girlAnimator.enabled = true;
				girlAnimator.CrossFade("Base Layer.putDownGirl", 0f, 0, this.currState2.normalizedTime);
			}
			this.girlHeld.SetActive(false);
			while (this.currState2.shortNameHash == this.putDownGirlHash)
			{
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				yield return null;
			}
			this.disablePlayerLocked();
			LocalPlayer.AnimControl.holdingGirl = false;
			LocalPlayer.Inventory.EquipPreviousWeapon(true);
			LocalPlayer.Inventory.ShowAllEquiped(true);
			float timer = 0f;
			while (timer < 1f)
			{
				LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
				timer += Time.deltaTime * 3f;
				yield return null;
			}
			yield break;
		}

		
		public IEnumerator girlToMachineRoutine(Transform pos)
		{
			Vector3 fixLocalPos = new Vector3(0f, -2.344841f, 0f);
			if (!this.spectator)
			{
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
				this.ActorAnimator = LocalPlayer.Animator;
				this.enablePlayerLocked();
			}
			Vector3 playerPos = pos.position;
			playerPos.y += 2.35f;
			float t = 0f;
			while (t < 1f)
			{
				if (!this.spectator)
				{
					LocalPlayer.Transform.position = Vector3.Slerp(LocalPlayer.Transform.position, playerPos, t);
					LocalPlayer.Transform.rotation = Quaternion.Slerp(LocalPlayer.Transform.rotation, pos.rotation, t);
				}
				t += Time.deltaTime;
				yield return null;
			}
			if (!this.spectator)
			{
				LocalPlayer.Transform.position = playerPos;
				LocalPlayer.Transform.rotation = pos.rotation;
				LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
				LocalPlayer.AnimControl.endGameCutScene = true;
				LocalPlayer.AnimControl.useRootMotion = true;
				LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.Chest, false, true, false);
				LocalPlayer.Animator.SetBool("toMachine", true);
				this.girlHeld.SetActive(false);
			}
			if (!this.spectator)
			{
				syncGirlPickup ev = syncGirlPickup.Create(GlobalTargets.OnlyServer);
				ev.dedicatedSpawn = true;
				ev.spawnPos = this.ActorAnimator.transform.position;
				ev.spawnRot = this.ActorAnimator.transform.rotation;
				ev.Send();
			}
			GameObject spawn;
			Animator girlAnimator;
			if (!BoltNetwork.isRunning)
			{
				spawn = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_NoPickup"), this.ActorAnimator.transform.position, this.ActorAnimator.transform.rotation);
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				syncGirlPickup ev2 = syncGirlPickup.Create(GlobalTargets.Everyone);
				ev2.target = spawn.GetComponent<BoltEntity>();
				ev2.toMachine = true;
				ev2.Send();
				girlAnimator = spawn.GetComponentInChildren<Animator>();
				enableWithDelay ewd = spawn.GetComponent<enableWithDelay>();
				if (ewd)
				{
					ewd.enabled = false;
				}
				girlAnimator.enabled = true;
				girlAnimator.CrossFade("Base Layer.girlToMachine", 0f, 0, 0f);
			}
			else
			{
				spawn = null;
				girlAnimator = null;
			}
			this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
			while (this.currState2.shortNameHash != this.girlToMachineHash)
			{
				this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
				if (!this.spectator)
				{
					LocalPlayer.Rigidbody.velocity = Vector3.zero;
					LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
					LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				}
				if (this.currState2.shortNameHash == this.girlToMachineHash)
				{
					break;
				}
				yield return null;
			}
			if (!BoltNetwork.isClient && !this.spectator)
			{
				if (girlAnimator != null)
				{
					girlAnimator.CrossFade("Base Layer.girlToMachine", 0f, 0, this.currState2.normalizedTime);
				}
				if (spawn != null)
				{
					spawn.transform.position = this.ActorAnimator.transform.position;
				}
			}
			if (!this.spectator)
			{
				LocalPlayer.Animator.SetBool("girlPickup", false);
			}
			bool spawnKey = false;
			while (this.currState2.shortNameHash == this.girlToMachineHash)
			{
				if (!this.spectator)
				{
					if (this.currState2.normalizedTime > 0.328f && !spawnKey && !BoltNetwork.isClient)
					{
						if (girlAnimator)
						{
							girlAnimator.transform.SendMessage("doEnableGo", SendMessageOptions.DontRequireReceiver);
						}
						spawnKey = true;
					}
					LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				}
				this.currState2 = this.ActorAnimator.GetCurrentAnimatorStateInfo(2);
				yield return null;
			}
			if (!this.spectator)
			{
				this.disablePlayerLocked();
				LocalPlayer.AnimControl.useRootMotion = false;
				LocalPlayer.AnimControl.useRootRotation = false;
				LocalPlayer.AnimControl.holdingGirl = false;
				LocalPlayer.AnimControl.endGameCutScene = false;
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
				LocalPlayer.Inventory.EquipPreviousWeapon(true);
				LocalPlayer.Inventory.ShowAllEquiped(true);
				float timer = 0f;
				while (timer < 1f)
				{
					LocalPlayer.CamFollowHead.transform.localRotation = Quaternion.Lerp(LocalPlayer.CamFollowHead.transform.localRotation, LocalPlayer.CamFollowHead.transform.parent.localRotation, timer);
					timer += Time.deltaTime * 3f;
					yield return null;
				}
			}
			yield break;
		}

		
		private void setGirlGo(GameObject go)
		{
			this.girlGo = go;
		}

		
		private void setGirlTrigger(GameObject go)
		{
			this.girlTrigger = go;
		}

		
		private IEnumerator forceGirlReset()
		{
			base.StopCoroutine("pickupGirlRoutine");
			if (LocalPlayer.AnimControl.holdingGirl)
			{
				LocalPlayer.AnimControl.holdingGirl = false;
				LocalPlayer.Animator.SetBool("girlPickup", false);
				this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
				if (this.girlGo)
				{
					Animator ga = this.girlGo.GetComponentInChildren<Animator>();
					if (ga)
					{
						base.StartCoroutine(this.resetGirlAnimation(ga));
					}
					this.girlGo.SendMessage("enableThisGo", SendMessageOptions.DontRequireReceiver);
					if (this.girlTrigger)
					{
						this.girlTrigger.SendMessage("resetPickup", SendMessageOptions.DontRequireReceiver);
					}
					syncGirlPickup ev = syncGirlPickup.Create(GlobalTargets.Everyone);
					ev.target = this.girlGo.GetComponent<BoltEntity>();
					ev.enableTrigger = true;
					ev.Send();
				}
				else if (this.currState2.shortNameHash != this.putDownGirlHash)
				{
					if (BoltNetwork.isClient)
					{
						syncGirlPickup ev2 = syncGirlPickup.Create(GlobalTargets.Everyone);
						ev2.target = base.transform.root.GetComponent<BoltEntity>();
						ev2.spawnGirl = true;
						ev2.Send();
					}
					else
					{
						UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_Pickup"), LocalPlayer.PlayerBase.transform.position, LocalPlayer.PlayerBase.transform.rotation);
					}
				}
				this.girlHeld.SetActive(false);
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
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				yield return null;
				LocalPlayer.Inventory.HideAllEquiped(false, false);
				yield return null;
				LocalPlayer.Inventory.ShowAllEquiped(true);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
			}
			yield break;
		}

		
		private IEnumerator resetGirlAnimation(Animator a)
		{
			a.CrossFade("Base Layer.girlPickup", 0f, 0, 0f);
			yield return YieldPresets.WaitForEndOfFrame;
			a.enabled = false;
			yield break;
		}

		
		private void forceGirlResetFromExplosion()
		{
			base.StopCoroutine("pickupGirlRoutine");
			if (LocalPlayer.AnimControl.holdingGirl)
			{
				LocalPlayer.AnimControl.holdingGirl = false;
				LocalPlayer.Animator.SetBool("girlPickup", false);
				if (this.girlGo)
				{
					Animator componentInChildren = this.girlGo.GetComponentInChildren<Animator>();
					if (componentInChildren)
					{
						base.StartCoroutine(this.resetGirlAnimation(componentInChildren));
					}
					this.girlGo.SendMessage("enableThisGo", SendMessageOptions.DontRequireReceiver);
					if (this.girlTrigger)
					{
						this.girlTrigger.SendMessage("resetPickup", SendMessageOptions.DontRequireReceiver);
					}
					syncGirlPickup syncGirlPickup = syncGirlPickup.Create(GlobalTargets.Everyone);
					syncGirlPickup.target = this.girlGo.GetComponent<BoltEntity>();
					syncGirlPickup.enableTrigger = true;
					syncGirlPickup.Send();
				}
				else
				{
					this.currState2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
					if (this.currState2.shortNameHash != this.putDownGirlHash)
					{
						if (BoltNetwork.isClient)
						{
							syncGirlPickup syncGirlPickup2 = syncGirlPickup.Create(GlobalTargets.Everyone);
							syncGirlPickup2.target = base.transform.root.GetComponent<BoltEntity>();
							syncGirlPickup2.spawnGirl = true;
							syncGirlPickup2.Send();
						}
						else
						{
							UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_Pickup"), LocalPlayer.PlayerBase.transform.position, LocalPlayer.PlayerBase.transform.rotation);
						}
					}
				}
				this.girlHeld.SetActive(false);
				LocalPlayer.Create.Grabber.gameObject.SetActive(true);
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.AnimControl.playerCollider.enabled = true;
				LocalPlayer.FpCharacter.drinking = false;
				LocalPlayer.FpCharacter.Locked = false;
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
			}
		}

		
		private void enablePlayerLocked()
		{
			LocalPlayer.ScriptSetup.pmControl.SendEvent("toResetSpear");
			LocalPlayer.FpCharacter.allowFallDamage = false;
			LocalPlayer.FpCharacter.Locked = true;
			LocalPlayer.AnimControl.holdingGirl = true;
			LocalPlayer.Rigidbody.isKinematic = true;
			LocalPlayer.FpCharacter.CanJump = false;
			LocalPlayer.Create.Grabber.gameObject.SetActive(false);
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.Animator.SetBool("onHand", false);
			LocalPlayer.Inventory.HideAllEquiped(false, false);
			LocalPlayer.Animator.SetBool("blockColdBool", true);
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
			LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.RightHand);
			LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		}

		
		private void disablePlayerLocked()
		{
			if (LocalPlayer.AnimControl.holdingGirl)
			{
				LocalPlayer.Rigidbody.isKinematic = false;
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
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
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

		
		private Animator ActorAnimator;

		
		private AnimatorStateInfo currState2;

		
		public GameObject planeAxeHeldGo;

		
		public GameObject girlGo;

		
		public GameObject girlHeld;

		
		public GameObject girlTrigger;

		
		private Quaternion desiredRotation;

		
		private int idleToGirlHash = Animator.StringToHash("idleToGirlPickup");

		
		private int girlIdleHash = Animator.StringToHash("girlPickupIdle");

		
		private int putDownGirlHash = Animator.StringToHash("putDownGirl");

		
		private int idleHash = Animator.StringToHash("idling");

		
		private int heldHash = Animator.StringToHash("held");

		
		private int girlToMachineHash = Animator.StringToHash("girlToMachine");

		
		private bool spectator;

		
		private AnimationSequence currentSequence;
	}
}
