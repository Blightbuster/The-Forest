using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerEatMeatAction : MonoBehaviour
	{
		
		private void Awake()
		{
		}

		
		public IEnumerator eatMeatRoutine(bool isBurnt)
		{
			LocalPlayer.Animator.SetBoolReflected("eatMeat", true);
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.Inventory.HideAllEquiped(false, false);
			LocalPlayer.Animator.SetLayerWeightReflected(0, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = true;
			LocalPlayer.CamRotator.stopInput = true;
			LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
			LocalPlayer.FpCharacter.drinking = true;
			LocalPlayer.CamFollowHead.smoothLock = true;
			LocalPlayer.CamFollowHead.lockYCam = true;
			LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothDisableSpine");
			this.currState1 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1);
			while (!this.currState1.IsName("upperBody.eatMeat"))
			{
				this.currState1 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1);
				yield return null;
			}
			LocalPlayer.Animator.SetBoolReflected("eatMeat", false);
			this.currState1 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1);
			this.animatedMeatGo.SetActive(true);
			this.animatedMeatGo.SendMessage("setMeatMaterial", isBurnt, SendMessageOptions.DontRequireReceiver);
			this.animatedMeatGo.SendMessage("playMeat", this.currState1.normalizedTime, SendMessageOptions.DontRequireReceiver);
			Animator meatAnim = this.animatedMeatGo.GetComponent<Animator>();
			while (this.currState1.IsName("upperBody.eatMeat"))
			{
				this.currState1 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1);
				LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
				LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
				if (this.currState1.normalizedTime > 0.72f)
				{
					this.animatedMeatGo.SetActive(false);
				}
				yield return null;
			}
			if (LocalPlayer.FpCharacter.drinking)
			{
				this.animatedMeatGo.SetActive(false);
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.FpCharacter.Locked = false;
				LocalPlayer.FpCharacter.CanJump = true;
				LocalPlayer.CamFollowHead.lockYCam = false;
				LocalPlayer.CamFollowHead.smoothLock = false;
				LocalPlayer.MainRotator.enabled = true;
				LocalPlayer.CamRotator.stopInput = false;
				LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
				LocalPlayer.FpCharacter.enabled = true;
				LocalPlayer.CamRotator.enabled = true;
				LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
				LocalPlayer.FpCharacter.Locked = false;
				LocalPlayer.AnimControl.animEvents.StartCoroutine("smoothEnableSpine");
				LocalPlayer.FpCharacter.drinking = false;
				LocalPlayer.Inventory.EquipPreviousUtility(false);
				if (!LocalPlayer.Inventory.Logs.HasLogs)
				{
					LocalPlayer.Inventory.EquipPreviousWeapon(false);
				}
				LocalPlayer.ScriptSetup.pmControl.enabled = true;
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
				LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
			}
			yield return null;
			yield break;
		}

		
		private void hideMeat()
		{
			this.animatedMeatGo.SetActive(false);
		}

		
		private AnimatorStateInfo currState1;

		
		public GameObject animatedMeatGo;
	}
}
