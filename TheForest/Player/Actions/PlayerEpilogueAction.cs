using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerEpilogueAction : MonoBehaviour
	{
		
		private void Start()
		{
			if (this.doEpilogue)
			{
				base.StartCoroutine(this.SitOnCouch());
			}
		}

		
		private IEnumerator SitOnCouch()
		{
			yield return new WaitForSeconds(1f);
			this.currentChair = GameObject.Find("playerSeatPos").transform;
			LocalPlayer.ScriptSetup.pmBlock.SendEvent("toReset");
			LocalPlayer.FpCharacter.Sitting = true;
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = true;
			LocalPlayer.Animator.SetBool("couchBool", true);
			Vector3 playerPos = this.currentChair.position;
			playerPos.y += 2.35f;
			LocalPlayer.Transform.position = playerPos;
			LocalPlayer.Transform.rotation = this.currentChair.rotation;
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.CamRotator.rotationRange = new Vector2(90f, 140f);
			LocalPlayer.FpCharacter.CanJump = false;
			LocalPlayer.Inventory.CancelReloadDelay();
			LocalPlayer.Animator.SetBool("attack", false);
			LocalPlayer.Animator.SetBool("canReload", false);
			LocalPlayer.Animator.SetLayerWeight(2, 1f);
			LocalPlayer.Animator.SetLayerWeight(4, 0f);
			LocalPlayer.CamFollowHead.stopAllCameraShake();
			bool skipScene = false;
			while (!skipScene)
			{
				playerPos = this.currentChair.position;
				playerPos.y += 2.35f;
				LocalPlayer.Transform.position = playerPos;
				LocalPlayer.Transform.rotation = this.currentChair.rotation;
				yield return null;
			}
			yield return null;
			yield break;
		}

		
		public void UpFromCouch()
		{
			base.StopCoroutine("lockPlayerToBench");
			this.isChair = false;
			LocalPlayer.FpCharacter.Sitting = false;
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.Rigidbody.isKinematic = false;
			LocalPlayer.Inventory.CancelReloadDelay();
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("seatedBool").Value = false;
			LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
			LocalPlayer.MainRotator.resetOriginalRotation = true;
			LocalPlayer.MainRotator.enabled = true;
			LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
			LocalPlayer.CamRotator.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			LocalPlayer.FpCharacter.CanJump = true;
			LocalPlayer.Stats.StandUp();
			LocalPlayer.Create.RefreshGrabber();
			LocalPlayer.AnimControl.sitting = false;
			LocalPlayer.FpCharacter.allowFallDamage = true;
			LocalPlayer.FpCharacter.Locked = false;
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
		}

		
		public bool doEpilogue;

		
		private bool isChair;

		
		private Vector3 entryPosition;

		
		private Transform currentChair;
	}
}
