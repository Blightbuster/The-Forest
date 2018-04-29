using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class playerOpenKeypadDoorAction : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void openKeypadDoor(Transform pos)
	{
		base.StartCoroutine(this.openDoorRoutine(pos));
	}

	
	public IEnumerator openDoorRoutine(Transform pos)
	{
		GameObject keycardHeld = LocalPlayer.Inventory.InventoryItemViewsCache[this._keycardId][0]._held;
		bool keycardWasActive = keycardHeld.activeSelf;
		bool flashLightHeld = LocalPlayer.Animator.GetBool("flashLightHeld");
		bool lighterHeld = LocalPlayer.Animator.GetBool("lighterHeld");
		bool pedHeld = LocalPlayer.Animator.GetBool("pedHeld");
		LocalPlayer.Stats.cancelCheckItem();
		LocalPlayer.ScriptSetup.pmControl.enabled = false;
		LocalPlayer.Animator.SetBoolReflected("stickBlock", false);
		LocalPlayer.Animator.SetBool("lookAtPhoto", false);
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.CamRotator.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.CamFollowHead.followAnim = true;
		LocalPlayer.AnimControl.lockGravity = true;
		if (!keycardWasActive)
		{
			LocalPlayer.Inventory.HideRightHand(false);
		}
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.AnimControl.onRope = true;
		Vector3 standPos = pos.position;
		standPos.y += LocalPlayer.AnimControl.playerCollider.height / 2f;
		LocalPlayer.Transform.parent = pos;
		LocalPlayer.Transform.localPosition = new Vector3(0f, 0.01945f, 0f);
		LocalPlayer.Transform.localEulerAngles = Vector3.zero;
		if (this.shortSequence)
		{
			LocalPlayer.Animator.SetFloat("keypadFloat", 1f);
		}
		else
		{
			LocalPlayer.Animator.SetFloat("keypadFloat", 0f);
		}
		LocalPlayer.Animator.SetBool("openKeypadDoor", true);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		base.Invoke("resetDoorParams", 1f);
		LocalPlayer.Inventory.CancelEquipPreviousWeaponDelayed();
		if (LocalPlayer.Inventory.Owns(this._keycardId, true))
		{
			keycardHeld.SetActive(true);
		}
		if (this.shortSequence)
		{
			keycardHeld.SetActive(true);
		}
		yield return YieldPresets.WaitOneSecond;
		bool syncDoor = false;
		this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
		if (this.doorAnimator)
		{
			this.doorAnimator.StopPlayback();
		}
		while (this.layer0.tagHash == this.enterDoorHash || LocalPlayer.AnimControl.loadingAnimation)
		{
			yield return null;
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			this.lockPlayerParams();
			this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			if (this.layer0.normalizedTime < 0.75f)
			{
				if (this.doorAnimator)
				{
					this.doorAnimator.Play("Base Layer.open", 0, this.layer0.normalizedTime);
				}
			}
			else if (!syncDoor)
			{
				if (this.doorAnimator)
				{
					this.doorAnimator.CrossFade("Base Layer.open", 0f, 0, this.layer0.normalizedTime);
					this.doorAnimator.StopPlayback();
				}
				syncDoor = true;
			}
		}
		if (this.doorAnimator)
		{
			this.doorAnimator.CrossFade("Base Layer.openStatic", 0f, 0, 0f);
		}
		this.unlockPlayerParams();
		LocalPlayer.ScriptSetup.pmControl.enabled = true;
		LocalPlayer.ScriptSetup.pmControl.SendEvent("toReset2");
		LocalPlayer.Transform.parent = null;
		LocalPlayer.Transform.localScale = new Vector3(1f, 1f, 1f);
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.AnimControl.onRope = false;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.AnimControl.controller.useGravity = true;
		LocalPlayer.AnimControl.controller.isKinematic = false;
		if (!keycardWasActive)
		{
			keycardHeld.SetActive(false);
			LocalPlayer.Inventory.EquipPreviousWeapon(false);
		}
		if (flashLightHeld)
		{
			LocalPlayer.Animator.SetBool("flashLightHeld", true);
		}
		if (lighterHeld)
		{
			LocalPlayer.Animator.SetBool("lighterHeld", true);
		}
		if (pedHeld)
		{
			LocalPlayer.Animator.SetBool("pedHeld", true);
		}
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		yield break;
	}

	
	private void setShortSequence(bool set)
	{
		this.shortSequence = set;
	}

	
	private void setKeycardId(int keycardId)
	{
		if (keycardId > 0)
		{
			this._keycardId = keycardId;
		}
	}

	
	private void resetDoorParams()
	{
		LocalPlayer.Animator.SetBool("openKeypadDoor", false);
	}

	
	private void setDoorAnimator(Animator a)
	{
		this.doorAnimator = a;
	}

	
	private void lockPlayerParams()
	{
		LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
		LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.RightHand);
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
		LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
		LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
		LocalPlayer.FpCharacter.allowFallDamage = true;
		LocalPlayer.FpCharacter.Locked = false;
		LocalPlayer.FpCharacter.CanJump = true;
		LocalPlayer.AnimControl.endGameCutScene = false;
		LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.Rigidbody.isKinematic = false;
	}

	
	private AnimatorStateInfo layer0;

	
	private int enterDoorHash = Animator.StringToHash("openDoor");

	
	private bool ignoreLighting;

	
	private bool shortSequence;

	
	public Animator doorAnimator;

	
	public GameObject keycardElevator;

	
	[ItemIdPicker]
	public int _keycardId;
}
