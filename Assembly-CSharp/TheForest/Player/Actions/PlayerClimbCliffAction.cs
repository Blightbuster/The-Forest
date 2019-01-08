using System;
using System.Collections;
using HutongGames.PlayMaker;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	public class PlayerClimbCliffAction : MonoBehaviour
	{
		private void enterClimbCliff(Transform trn)
		{
			FsmBool fsmBool = this._player.PM.FsmVariables.GetFsmBool("climbBool");
			if (!fsmBool.Value && !this.doingClimb)
			{
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
				LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
				LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.RightHand);
				LocalPlayer.CamFollowHead.stopAllCameraShake();
				LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
				LocalPlayer.Animator.SetIntegerReflected("climbTypeInt", 1);
				LocalPlayer.FpCharacter.enabled = false;
				LocalPlayer.MainRotator.enabled = false;
				LocalPlayer.CamRotator.enabled = true;
				LocalPlayer.CamRotator.rotationRange = new Vector2(90f, 105f);
				LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
				LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
				LocalPlayer.AnimControl.enterClimbMode();
				LocalPlayer.AnimControl.cliffClimb = true;
				LocalPlayer.AnimControl.lockGravity = true;
				LocalPlayer.Rigidbody.velocity = Vector3.zero;
				LocalPlayer.AnimControl.playerCollider.height = 3.5f;
				LocalPlayer.AnimControl.playerCollider.center = new Vector3(0f, 0f, 0f);
				this._player.PM.FsmVariables.GetFsmGameObject("climbGo").Value = trn.gameObject;
				LocalPlayer.Animator.SetBoolReflected("exitClimbTopBool", false);
				this._player.PM.SendEvent("toClimb");
				fsmBool.Value = true;
				this.doingClimb = true;
				Vector3 vector;
				LocalPlayer.Transform.position.y = vector.y + 1f;
				LocalPlayer.Transform.position = this.enterPos;
				LocalPlayer.Transform.position -= LocalPlayer.Transform.forward;
			}
		}

		private void setEnterClimbPos(Vector3 pos)
		{
			this.enterPos = pos;
		}

		private void enableDoingClimbCliff()
		{
			if (this._player.PM.FsmVariables.GetFsmBool("climbBool").Value)
			{
				this.doingClimb = true;
			}
		}

		private void exitClimbCliffTop(Transform trn)
		{
			if (this.doingClimb)
			{
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
				LocalPlayer.Animator.SetBoolReflected("setClimbBool", false);
				LocalPlayer.Animator.SetBoolReflected("exitClimbTopBool", true);
				LocalPlayer.PlayerBase.transform.localEulerAngles = Vector3.zero;
				this._player.PM.FsmVariables.GetFsmBool("climbTopBool").Value = false;
				this._player.PM.FsmVariables.GetFsmBool("climbBool").Value = false;
				this._player.PM.SendEvent("toExitClimb");
				base.CancelInvoke("enableDoingClimb");
			}
		}

		public void exitClimbCliffGround()
		{
			if (this.doingClimb)
			{
				LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
				LocalPlayer.Animator.SetIntegerReflected("climbDirInt", -1);
				LocalPlayer.Animator.SetBoolReflected("setClimbBool", false);
				this._player.PM.FsmVariables.GetFsmBool("climbTopBool").Value = false;
				this._player.PM.FsmVariables.GetFsmBool("climbBool").Value = false;
				this._player.PM.SendEvent("toExitClimb");
				base.CancelInvoke("enableDoingClimb");
				LocalPlayer.AnimControl.cliffClimb = false;
				LocalPlayer.AnimControl.lockGravity = false;
				LocalPlayer.AnimControl.allowCliffReset = false;
				LocalPlayer.AnimControl.CancelInvoke("enableCliffReset");
				LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
				LocalPlayer.PlayerBase.transform.localEulerAngles = Vector3.zero;
				LocalPlayer.AnimControl.playerCollider.height = 4.7f;
				base.StartCoroutine(this.forcePlayerDrag());
			}
		}

		private void resetClimbCliff()
		{
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
			this.doingClimb = false;
			LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
			LocalPlayer.CamRotator.transform.localEulerAngles = Vector3.Scale(LocalPlayer.CamRotator.transform.localEulerAngles, Vector3.right);
			LocalPlayer.FpCharacter.enabled = true;
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
			LocalPlayer.GameObject.GetComponent<Rigidbody>().freezeRotation = true;
			LocalPlayer.GameObject.GetComponent<Rigidbody>().useGravity = true;
			LocalPlayer.GameObject.GetComponent<Rigidbody>().isKinematic = false;
			LocalPlayer.AnimControl.CancelInvoke("enableCliffReset");
			LocalPlayer.AnimControl.allowCliffReset = false;
			LocalPlayer.AnimControl.playerCollider.height = 4.7f;
			base.StartCoroutine(this.forcePlayerDrag());
		}

		private IEnumerator forcePlayerDrag()
		{
			if (this.forceRunning)
			{
				yield break;
			}
			float t = 0f;
			while (t < 1f)
			{
				this.forceRunning = true;
				LocalPlayer.Rigidbody.drag = 25f;
				LocalPlayer.Rigidbody.angularDrag = 25f;
				t += Time.deltaTime;
				yield return null;
			}
			this.forceRunning = false;
			LocalPlayer.Rigidbody.drag = 0f;
			LocalPlayer.Rigidbody.angularDrag = 0.05f;
			yield break;
		}

		public PlayerInventory _player;

		public float ropeAttachOffset;

		private Vector3 enterPos;

		public bool doingClimb;

		private bool forceRunning;
	}
}
