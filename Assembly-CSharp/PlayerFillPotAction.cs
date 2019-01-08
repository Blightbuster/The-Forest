using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

public class PlayerFillPotAction : MonoBehaviour
{
	private void Start()
	{
	}

	private IEnumerator doFillPotRoutine()
	{
		LocalPlayer.Create.Grabber.gameObject.SetActive(false);
		LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.RightHand);
		LocalPlayer.Animator.SetBool("fillPotBool", true);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		base.StartCoroutine(LocalPlayer.AnimControl.smoothEnableLayerNew(2));
		LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
		LocalPlayer.Transform.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.stopInput = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.FpCharacter.drinking = true;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.CamFollowHead.smoothLock = true;
		LocalPlayer.CamFollowHead.lockYCam = true;
		LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
		base.StartCoroutine("forceStop");
		yield return YieldPresets.WaitOneSecond;
		LocalPlayer.Animator.SetBool("fillPotBool", false);
		LocalPlayer.Sfx.PlayTwinkle();
		AnimatorStateInfo state2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
		while (state2.shortNameHash == this.fillPotHash || state2.shortNameHash == this.fillWaterSkinHash)
		{
			state2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			if (LocalPlayer.Animator.IsInTransition(2) && state2.tagHash != this.attackingHash)
			{
				break;
			}
			yield return null;
		}
		if (LocalPlayer.FpCharacter.drinking)
		{
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
			LocalPlayer.Create.Grabber.gameObject.SetActive(true);
			LocalPlayer.FpCharacter.Locked = false;
			LocalPlayer.FpCharacter.CanJump = true;
			LocalPlayer.CamFollowHead.lockYCam = false;
			LocalPlayer.CamFollowHead.smoothLock = false;
			LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			LocalPlayer.MainRotator.enabled = true;
			LocalPlayer.CamRotator.stopInput = false;
			LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
			LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
			LocalPlayer.FpCharacter.enabled = true;
			LocalPlayer.FpCharacter.Locked = false;
			base.StartCoroutine(LocalPlayer.AnimControl.smoothEnableLayerNew(4));
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
			LocalPlayer.FpCharacter.drinking = false;
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
		}
		yield break;
	}

	private IEnumerator forceStop()
	{
		float t = 0f;
		while (t < 1f)
		{
			LocalPlayer.Rigidbody.velocity = new Vector3(0f, 0f, 0f);
			t += Time.deltaTime;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	private int fillPotHash = Animator.StringToHash("fillPot");

	private int fillWaterSkinHash = Animator.StringToHash("fillWaterSkin");

	private int attackingHash = Animator.StringToHash("attacking");
}
