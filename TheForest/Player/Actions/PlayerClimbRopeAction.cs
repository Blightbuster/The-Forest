using System;
using System.Collections;
using HutongGames.PlayMaker;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player.Actions
{
	
	public class PlayerClimbRopeAction : MonoBehaviour
	{
		
		private void Start()
		{
			this.climbHash = Animator.StringToHash("climbing");
			this.climbIdleHash = Animator.StringToHash("climbIdle");
		}

		
		private void enterClimbRope(Transform trn)
		{
			FsmBool fsmBool = this._player.PM.FsmVariables.GetFsmBool("climbBool");
			if (!fsmBool.Value && !this.doingClimb && !LocalPlayer.FpCharacter.Sitting)
			{
				LocalPlayer.CamFollowHead.stopAllCameraShake();
				LocalPlayer.Animator.SetBoolReflected("resetClimbBool", false);
				LocalPlayer.Animator.SetIntegerReflected("climbTypeInt", 0);
				LocalPlayer.MainRotator.enabled = false;
				LocalPlayer.CamRotator.enabled = true;
				LocalPlayer.CamRotator.rotationRange = new Vector2(78f, 105f);
				LocalPlayer.AnimControl.lockGravity = true;
				Vector3 vector = trn.position - trn.forward * this.ropeAttachOffset;
				if (!LocalPlayer.IsInCaves && !Scene.IsInSinkhole(LocalPlayer.Transform.position))
				{
					float num = Terrain.activeTerrain.SampleHeight(trn.position);
					if (vector.y < num + 2.75f)
					{
						vector.y = Mathf.Max(vector.y, num + 2.75f);
					}
				}
				this._player.PM.FsmVariables.GetFsmVector3("attachPos").Value = vector;
				if (LocalPlayer.Transform.position.y + 2.5f < vector.y)
				{
					LocalPlayer.Transform.position = vector;
				}
				activateClimb componentInChildren = trn.parent.GetComponentInChildren<activateClimb>();
				if (componentInChildren)
				{
					this._bottomTriggerTr = componentInChildren.transform;
				}
				this._player.transform.position = vector;
				this._player.transform.rotation = trn.rotation;
				LocalPlayer.AnimControl.enterClimbMode();
				this._player.HideRightHand(false);
				this._player.PM.FsmVariables.GetFsmGameObject("climbGo").Value = trn.gameObject;
				LocalPlayer.Animator.SetBoolReflected("exitClimbTopBool", false);
				this._player.PM.SendEvent("toClimb");
				fsmBool.Value = true;
				this.doingClimb = true;
				this.enteredTop = false;
				base.StartCoroutine("stickToRope", trn);
				this._currentRopeRoot = trn.parent;
			}
		}

		
		private void enterClimbRopeTop(Transform trn)
		{
			FsmBool fsmBool = this._player.PM.FsmVariables.GetFsmBool("climbBool");
			if (!fsmBool.Value && !this.doingClimb && !LocalPlayer.FpCharacter.Sitting)
			{
				base.StartCoroutine(this.blockRopeGroundCheck());
				LocalPlayer.CamFollowHead.stopAllCameraShake();
				LocalPlayer.Animator.SetBoolReflected("resetClimbBool", false);
				fsmBool.Value = true;
				LocalPlayer.MainRotator.enabled = false;
				LocalPlayer.AnimControl.lockGravity = true;
				LocalPlayer.MainCamTr.localEulerAngles = new Vector3(0f, 0f, 0f);
				LocalPlayer.CamRotator.enabled = true;
				LocalPlayer.CamRotator.rotationRange = new Vector2(78f, 105f);
				activateClimb componentInChildren = trn.parent.GetComponentInChildren<activateClimb>();
				if (componentInChildren)
				{
					this._bottomTriggerTr = componentInChildren.transform;
				}
				exitClimb componentInChildren2 = trn.parent.GetComponentInChildren<exitClimb>();
				if (componentInChildren2)
				{
					this.exitTopTrigger = componentInChildren2.GetComponent<Collider>();
					if (this.exitTopTrigger)
					{
						this.exitTopTrigger.enabled = false;
					}
				}
				Vector3 vector = trn.position + trn.forward * this.ropeAttachTopOffsetZ;
				vector += trn.right * this.ropeAttachTopOffsetX;
				vector.y = componentInChildren2.transform.position.y - this.ropeAttachTopOffsetY;
				if (LocalPlayer.Inventory.Logs.HasLogs)
				{
					LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
					if (LocalPlayer.Inventory.Logs.HasLogs)
					{
						LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
					}
				}
				this.lastClimbDownPos = LocalPlayer.Transform.position.y;
				this._player.PM.FsmVariables.GetFsmVector3("attachPos").Value = vector;
				this._player.transform.position = vector;
				this._player.transform.rotation = trn.rotation;
				LocalPlayer.AnimControl.enterClimbMode();
				this._player.PM.FsmVariables.GetFsmGameObject("climbGo").Value = trn.gameObject;
				LocalPlayer.Animator.SetIntegerReflected("climbTypeInt", 0);
				this._player.PM.FsmVariables.GetFsmBool("climbTopBool").Value = true;
				LocalPlayer.Animator.SetBoolReflected("exitClimbTopBool", false);
				this._player.MemorizeItem(Item.EquipmentSlot.RightHand);
				this._player.StashEquipedWeapon(false);
				this._player.PM.SendEvent("toClimb");
				LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
				this.enteredTop = true;
				base.StartCoroutine("stickToRope", trn);
				if (trn.GetComponentInParent<CaveOptimizer>())
				{
					base.StartCoroutine(this.enableDoingClimbAndDelayCavePropsLoading());
				}
				else
				{
					base.Invoke("enableDoingClimb", 2.6f);
				}
				this._currentRopeRoot = trn.parent;
			}
		}

		
		private void setAttachPos()
		{
		}

		
		private void enableDoingClimb()
		{
			if (this._player.PM.FsmVariables.GetFsmBool("climbBool").Value)
			{
				this.doingClimb = true;
			}
		}

		
		private IEnumerator enableDoingClimbAndDelayCavePropsLoading()
		{
			CaveOptimizer.CanLoadOnRope = false;
			float timer = Time.time + 10f;
			while (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != this.climbIdleNameHash)
			{
				timer += Time.deltaTime;
				if (Time.time > timer)
				{
					break;
				}
				yield return null;
			}
			CaveOptimizer.CanLoadOnRope = true;
			if (this._player.PM.FsmVariables.GetFsmBool("climbBool").Value)
			{
				float finalClimbDist = this.lastClimbDownPos - LocalPlayer.Transform.position.y;
				Vector3 newPlayerPos = LocalPlayer.Transform.position;
				if ((double)finalClimbDist < 6.4)
				{
					newPlayerPos.y = this.lastClimbDownPos - 6.4823f;
					LocalPlayer.Transform.position = newPlayerPos;
				}
				this.doingClimb = true;
			}
			yield break;
		}

		
		private void exitClimbRopeTop(Transform trn)
		{
			if (this.doingClimb && this._currentRopeRoot == trn.parent)
			{
				LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
				LocalPlayer.Animator.SetBoolReflected("setClimbBool", false);
				LocalPlayer.Animator.SetBoolReflected("exitClimbTopBool", true);
				this._player.PM.FsmVariables.GetFsmBool("climbTopBool").Value = false;
				this._player.PM.FsmVariables.GetFsmBool("climbBool").Value = false;
				this._player.PM.SendEvent("toExitClimb");
				base.CancelInvoke("enableDoingClimb");
				base.StopCoroutine("stickToRope");
				this.resetTopExitTrigger();
				this._bottomTriggerTr = null;
			}
		}

		
		private void resetClimbRopeTop()
		{
			if (this.doingClimb)
			{
				LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
				LocalPlayer.Animator.SetBoolReflected("setClimbBool", false);
				LocalPlayer.Animator.SetBoolReflected("exitClimbTopBool", true);
				this._player.PM.FsmVariables.GetFsmBool("climbTopBool").Value = false;
				this._player.PM.FsmVariables.GetFsmBool("climbBool").Value = false;
				this._player.PM.SendEvent("toResetClimb");
				base.CancelInvoke("enableDoingClimb");
				base.StopCoroutine("stickToRope");
				this.resetTopExitTrigger();
			}
		}

		
		private void exitClimbRopeGround(bool reset)
		{
			if (this.doingClimb)
			{
				LocalPlayer.AnimControl.lockGravity = false;
				LocalPlayer.GameObject.GetComponent<Rigidbody>().useGravity = true;
				LocalPlayer.GameObject.GetComponent<Rigidbody>().isKinematic = false;
				LocalPlayer.Animator.SetBoolReflected("jumpBool", false);
				LocalPlayer.Animator.SetIntegerReflected("climbDirInt", -1);
				LocalPlayer.Animator.SetBoolReflected("setClimbBool", false);
				this._player.PM.FsmVariables.GetFsmBool("climbTopBool").Value = false;
				this._player.PM.FsmVariables.GetFsmBool("climbBool").Value = false;
				if (reset)
				{
					LocalPlayer.ScriptSetup.pmControl.SendEvent("toResetClimb");
				}
				else
				{
					LocalPlayer.ScriptSetup.pmControl.SendEvent("toExitClimb");
				}
				base.CancelInvoke("enableDoingClimb");
				base.StopCoroutine("stickToRope");
				this.resetTopExitTrigger();
				this._bottomTriggerTr = null;
			}
		}

		
		private void resetTopExitTrigger()
		{
			if (this.exitTopTrigger)
			{
				this.exitTopTrigger.enabled = true;
				this.exitTopTrigger = null;
			}
		}

		
		private void resetClimbRope()
		{
			base.StopCoroutine(this.enableDoingClimbAndDelayCavePropsLoading());
			this._currentRopeRoot = null;
			this.doingClimb = false;
			LocalPlayer.MainRotator.enabled = true;
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.CamFollowHead.followAnim = false;
			LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
			LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
			LocalPlayer.Inventory.ShowRightHand(false);
			base.StopCoroutine("stickToRope");
			this.resetTopExitTrigger();
			this._bottomTriggerTr = null;
		}

		
		private IEnumerator blockRopeGroundCheck()
		{
			float t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime;
				if (LocalPlayer.AnimControl)
				{
					LocalPlayer.AnimControl.blockHeightCheck = true;
				}
				yield return null;
			}
			LocalPlayer.AnimControl.blockHeightCheck = false;
			yield break;
		}

		
		private void fixClimbingPosition()
		{
			Vector3 position = LocalPlayer.Transform.position;
			position.y += 3.4f;
			this.onRopeAttachPos = position;
			LocalPlayer.Transform.position = position;
		}

		
		private IEnumerator stickToRope(Transform trn)
		{
			float terrainHeight = Terrain.activeTerrain.SampleHeight(trn.position);
			bool flag = !LocalPlayer.IsInCaves && !trn.GetComponentInParent<CaveOptimizer>();
			this.onRopeAttachPos = trn.position - trn.forward * this.onRopeOffset;
			this.onRopeAttachPos.y = this._player.transform.position.y;
			int fixedCount = 0;
			while (!(trn == null))
			{
				if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash == this.climbHash || LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash == this.climbIdleHash)
				{
					LocalPlayer.AnimControl.lockGravity = true;
					LocalPlayer.Rigidbody.isKinematic = true;
					this.onRopeAttachPos = trn.position - trn.forward * this.onRopeOffset;
					if (this.enteredTop)
					{
						if (this.exitTopTrigger)
						{
							this.onRopeAttachPos.y = this.exitTopTrigger.transform.position.y - 4f;
							fixedCount++;
							if (fixedCount > 6)
							{
								this.enteredTop = false;
							}
						}
					}
					else
					{
						this.onRopeAttachPos.y = this._player.transform.position.y;
						if (this.exitTopTrigger && !this.exitTopTrigger.enabled)
						{
							this.exitTopTrigger.enabled = true;
							this.exitTopTrigger = null;
						}
					}
					this._player.transform.position = this.onRopeAttachPos;
					this._player.transform.rotation = trn.rotation;
				}
				if (this._bottomTriggerTr)
				{
					float bottomDist = LocalPlayer.Transform.position.y - this._bottomTriggerTr.position.y;
					if (bottomDist < -3f)
					{
						this.exitClimbRopeGround(false);
						yield break;
					}
				}
				yield return null;
			}
			this.doingClimb = true;
			this.exitClimbRopeGround(false);
			yield break;
			yield break;
		}

		
		public PlayerInventory _player;

		
		public float onRopeOffset;

		
		public float ropeAttachOffset;

		
		public float ropeAttachTopOffsetZ;

		
		public float ropeAttachTopOffsetY;

		
		public float ropeAttachTopOffsetX;

		
		public bool doingClimb;

		
		private int climbHash;

		
		private int climbIdleHash;

		
		private int climbIdleNameHash = Animator.StringToHash("ropClimbidle");

		
		private Vector3 onRopeAttachPos;

		
		private float lastClimbDownPos;

		
		private Transform _currentRopeRoot;

		
		private Transform _bottomTriggerTr;

		
		private Collider exitTopTrigger;

		
		private bool enteredTop;
	}
}
