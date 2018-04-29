using System;
using System.Collections;
using HutongGames.PlayMaker;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerSitAction : MonoBehaviour
	{
		
		private void SitOnBench(Transform trn)
		{
			this.secondPosOffset = 1f;
			FsmBool fsmBool = LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("seatedBool");
			if (!fsmBool.Value)
			{
				if (!this.isDynamic)
				{
					this.entryPosition = LocalPlayer.Transform.position;
				}
				this.currentChair = trn;
				LocalPlayer.ScriptSetup.pmBlock.SendEvent("toReset");
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmGameObject("seatedGo").Value = trn.gameObject;
				LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmFloat("seatOffsetY").Value = trn.position.y;
				LocalPlayer.FpCharacter.Sitting = true;
				Vector3 vector = trn.InverseTransformPoint(LocalPlayer.Transform.position);
				float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
				LocalPlayer.Transform.position = trn.position;
				if (LocalPlayer.Animator.GetBool("ballHeld"))
				{
					LocalPlayer.Inventory.DropEquipedWeapon(false);
				}
				if (LocalPlayer.Inventory.Logs.HasLogs)
				{
					LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
					if (LocalPlayer.Inventory.Logs.HasLogs)
					{
						LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
					}
				}
				if ((num > -90f && num < 90f) || this.isChair)
				{
					LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmFloat("seatOffset").Value = this._seatOffset * 20f;
					Vector3 vector2 = trn.position + trn.forward * -this._zOffset;
					vector2.y = LocalPlayer.Transform.position.y;
					if (this.useSecondPosition)
					{
						vector2 += trn.right * -this.secondPosOffset;
					}
					else if (!this.isChair)
					{
						vector2 += trn.right * this.secondPosOffset;
					}
					LocalPlayer.Transform.position = vector2;
					base.StartCoroutine(this.lockPlayerToBench(vector2, trn));
				}
				else
				{
					LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmFloat("seatOffset").Value = -this._seatOffset * 20f;
					Vector3 vector3 = trn.position + trn.forward * this._zOffset;
					vector3.y = LocalPlayer.Transform.position.y;
					if (this.useSecondPosition)
					{
						vector3 += trn.right * -this.secondPosOffset;
					}
					else if (!this.isChair)
					{
						vector3 += trn.right * this.secondPosOffset;
					}
					LocalPlayer.Transform.position = vector3;
					base.StartCoroutine(this.lockPlayerToBench(vector3, trn));
				}
				this.canEquipLeft = false;
				this.canEquipRight = false;
				if (!LocalPlayer.Inventory.IsLeftHandEmpty())
				{
					if (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.LeftHand, LocalPlayer.AnimControl._walkmanId))
					{
						LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.LeftHand);
						this.canEquipLeft = true;
					}
					LocalPlayer.Inventory.StashLeftHand();
				}
				if (!LocalPlayer.Inventory.IsRightHandEmpty() && !LocalPlayer.Inventory.RightHand.IsHeldOnly)
				{
					LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
					LocalPlayer.Inventory.StashEquipedWeapon(false);
					this.canEquipRight = true;
				}
				LocalPlayer.AnimControl.sitting = true;
				LocalPlayer.MainRotator.enabled = false;
				LocalPlayer.CamRotator.rotationRange = new Vector2(100f, 135f);
				LocalPlayer.FpCharacter.CanJump = false;
				LocalPlayer.Inventory.CancelReloadDelay();
				LocalPlayer.Animator.SetBool("attack", false);
				LocalPlayer.Animator.SetBool("canReload", false);
				LocalPlayer.Grabber.GetComponent<Collider>().enabled = false;
				LocalPlayer.Stats.SitDown();
				fsmBool.Value = true;
			}
		}

		
		public void UpFromBench()
		{
			if (LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("seatedBool").Value)
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
				LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				LocalPlayer.FpCharacter.CanJump = true;
				LocalPlayer.Stats.StandUp();
				LocalPlayer.Grabber.GetComponent<Collider>().enabled = true;
				LocalPlayer.Create.RefreshGrabber();
				LocalPlayer.AnimControl.sitting = false;
				if (this.canEquipLeft)
				{
					LocalPlayer.Inventory.EquipPreviousUtility(false);
					this.canEquipLeft = false;
				}
				if (this.canEquipRight)
				{
					LocalPlayer.Inventory.EquipPreviousWeapon(true);
					this.canEquipRight = false;
				}
				this.isDynamic = false;
				if (this.currentChair && this.isDynamic)
				{
					this.entryPosition = this.currentChair.transform.position - this.entryPosition;
					LocalPlayer.Transform.position = this.entryPosition;
				}
				else if (this.currentChair)
				{
					LocalPlayer.Transform.position = this.entryPosition;
				}
			}
		}

		
		public void useTriggerHeight(bool get)
		{
			this.isChair = get;
		}

		
		public void setSecondPosition(bool onoff)
		{
			this.useSecondPosition = onoff;
		}

		
		private IEnumerator lockPlayerToBench(Vector3 pos, Transform tr)
		{
			while (LocalPlayer.FpCharacter.Sitting)
			{
				if (tr == null)
				{
					this.UpFromBench();
					yield break;
				}
				LocalPlayer.AnimControl.lockGravity = true;
				LocalPlayer.Rigidbody.isKinematic = true;
				LocalPlayer.Transform.position = pos;
				yield return null;
			}
			yield break;
		}

		
		public void forceDisableBench()
		{
			base.StopCoroutine("lockPlayerToBench");
			this.isChair = false;
			LocalPlayer.Inventory.CancelReloadDelay();
			LocalPlayer.FpCharacter.Sitting = false;
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.Rigidbody.isKinematic = false;
			LocalPlayer.FpCharacter.CanJump = true;
			LocalPlayer.Animator.SetBoolReflected("sittingBool", false);
			LocalPlayer.Stats.StandUp();
			LocalPlayer.AnimControl.sitting = false;
			LocalPlayer.HitReactions.disableControllerFreeze();
			LocalPlayer.Grabber.GetComponent<Collider>().enabled = true;
			if (this.canEquipLeft)
			{
				LocalPlayer.Inventory.EquipPreviousUtility(false);
				this.canEquipLeft = false;
			}
			if (this.canEquipRight)
			{
				LocalPlayer.Inventory.EquipPreviousWeapon(true);
				this.canEquipRight = false;
			}
		}

		
		public float _zOffset = 1f;

		
		public float _seatOffset = 10f;

		
		public float secondPosOffset = 1f;

		
		private bool isChair;

		
		public bool useSecondPosition;

		
		private Vector3 entryPosition;

		
		private Transform currentChair;

		
		private bool isDynamic;

		
		private bool canEquipLeft;

		
		private bool canEquipRight;
	}
}
