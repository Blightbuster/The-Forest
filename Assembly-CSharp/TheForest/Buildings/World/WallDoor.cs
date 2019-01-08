using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	public class WallDoor : EntityBehaviour<IWallChunkBuildingState>
	{
		protected virtual IEnumerator Start()
		{
			base.enabled = false;
			while (!base.GetComponentInParent<WallChunkArchitect>())
			{
				yield return null;
			}
			this._currentAction = this.GetActiveDoorStatusAction();
			this.FinalizeDoorStatus();
			if (BoltNetwork.isRunning)
			{
				while (!base.entity.isAttached)
				{
					yield return null;
				}
				this.FakeAttach();
			}
			yield break;
		}

		protected virtual void Update()
		{
			if (this._currentAction == WallDoor.Actions.Idle)
			{
				if (this.CanToggle() && TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (!BoltNetwork.isRunning)
					{
						WallDoor.Actions currentAction = this.ToggleDoorStatusAction(true);
						this._currentAction = currentAction;
						this._targetRb.isKinematic = (this._currentAction == WallDoor.Actions.Closing);
						if (this._targetRb.isKinematic)
						{
							this._targetRb.transform.localRotation = Quaternion.identity;
						}
						this._alpha = 0f;
					}
					else
					{
						WallDoor.Actions currentAction = this.ToggleDoorStatusAction(true);
					}
				}
				this.RefreshIcons();
			}
			else
			{
				this._alpha += Time.deltaTime * 3f;
				if (this._alpha < 1f)
				{
					if (this._currentAction == WallDoor.Actions.Openning)
					{
						this._target.localRotation = Quaternion.Slerp(WallDoor.ClosedRotation, WallDoor.OpenedRotation, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
					}
					else
					{
						this._alpha += Time.deltaTime * 3f;
						this._target.localPosition = Vector3.Slerp(WallDoor.OpenedPosition, WallDoor.ClosedPosition, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
					}
				}
				else if (this._alpha < 2f)
				{
					if (this._currentAction == WallDoor.Actions.Openning)
					{
						this._alpha += Time.deltaTime * 3f;
						this._target.localPosition = Vector3.Slerp(WallDoor.ClosedPosition, WallDoor.OpenedPosition, MathEx.Easing.EaseInOutQuad(this._alpha - 1f, 0f, 1f, 1f));
					}
					else
					{
						this._target.localRotation = Quaternion.Slerp(WallDoor.OpenedRotation, WallDoor.ClosedRotation, MathEx.Easing.EaseInOutQuad(this._alpha - 1f, 0f, 1f, 1f));
					}
				}
				else
				{
					this.FinalizeDoorStatus();
					this.RefreshIcons();
					base.enabled = (Grabber.FocusedItemGO == base.gameObject);
				}
			}
		}

		private void OnDestroy()
		{
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
			{
				base.state.RemoveCallback("Addition", new PropertyCallbackSimple(this.OnAdditionChange));
			}
		}

		private void GrabEnter()
		{
			base.enabled = true;
			this.RefreshIcons();
		}

		private void GrabExit()
		{
			base.enabled = (this._currentAction != WallDoor.Actions.Idle);
			this.RefreshIcons();
		}

		protected virtual void PlaySfx(WallDoor.Actions action)
		{
			if (LocalPlayer.Sfx)
			{
				if (action == WallDoor.Actions.Closing)
				{
					LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, 0.008f);
				}
				else
				{
					LocalPlayer.Sfx.PlayBreakWood(base.gameObject);
				}
			}
		}

		protected void RefreshIcons()
		{
			bool flag = base.enabled && Grabber.FocusedItemGO == base.gameObject && this.CanToggle();
			bool flag2 = base.GetComponentInParent<WallChunkArchitect>().Addition >= WallChunkArchitect.Additions.LockedDoor1;
			if (this._sheenIcon.activeSelf != (!flag && !flag2))
			{
				this._sheenIcon.SetActive(!flag && !flag2);
			}
			if (this._pickUpIcon.activeSelf != (flag && !flag2))
			{
				this._pickUpIcon.SetActive(flag && !flag2);
			}
			if (this._sheenIconUnlock.activeSelf != (!flag && flag2))
			{
				this._sheenIconUnlock.SetActive(!flag && flag2);
			}
			if (this._pickUpIconUnlock.activeSelf != (flag && flag2))
			{
				this._pickUpIconUnlock.SetActive(flag && flag2);
			}
		}

		protected virtual bool CanToggle()
		{
			float y = this._targetRb.transform.localEulerAngles.y;
			bool flag = base.transform.InverseTransformPoint(LocalPlayer.Transform.position).x > 0f;
			return this._currentAction == WallDoor.Actions.Idle && flag && Mathf.Approximately(this._targetRb.angularVelocity.y, 0f) && (y > 355f || y < 5f);
		}

		public WallDoor.Actions ToggleDoorStatusAction(bool mp)
		{
			WallChunkArchitect componentInParent = base.GetComponentInParent<WallChunkArchitect>();
			if (mp && BoltNetwork.isRunning && base.entity.isAttached)
			{
				ToggleWallDoor toggleWallDoor = ToggleWallDoor.Create(GlobalTargets.OnlyServer);
				toggleWallDoor.Entity = base.entity;
				toggleWallDoor.Send();
			}
			else
			{
				if (componentInParent.Addition >= WallChunkArchitect.Additions.LockedDoor1)
				{
					componentInParent.Addition = ((componentInParent.Addition != WallChunkArchitect.Additions.LockedDoor1) ? WallChunkArchitect.Additions.Door2 : WallChunkArchitect.Additions.Door1);
				}
				else
				{
					componentInParent.Addition = ((componentInParent.Addition != WallChunkArchitect.Additions.Door1) ? WallChunkArchitect.Additions.LockedDoor2 : WallChunkArchitect.Additions.LockedDoor1);
				}
				if (BoltNetwork.isServer)
				{
					base.state.Addition = (int)componentInParent.Addition;
					if (base.entity.isOwner)
					{
						((CoopWallChunkToken)base.entity.attachToken).Additions = componentInParent.Addition;
					}
				}
				this.PlaySfx((componentInParent.Addition < WallChunkArchitect.Additions.LockedDoor1) ? WallDoor.Actions.Openning : WallDoor.Actions.Closing);
			}
			return (componentInParent.Addition < WallChunkArchitect.Additions.LockedDoor1) ? WallDoor.Actions.Openning : WallDoor.Actions.Closing;
		}

		protected WallDoor.Actions GetActiveDoorStatusAction()
		{
			return (base.GetComponentInParent<WallChunkArchitect>().Addition < WallChunkArchitect.Additions.LockedDoor1) ? WallDoor.Actions.Openning : WallDoor.Actions.Closing;
		}

		protected virtual void FinalizeDoorStatus()
		{
			this._target.localPosition = ((this._currentAction != WallDoor.Actions.Openning) ? WallDoor.ClosedPosition : WallDoor.OpenedPosition);
			this._target.localRotation = ((this._currentAction != WallDoor.Actions.Openning) ? WallDoor.ClosedRotation : WallDoor.OpenedRotation);
			this._targetRb.isKinematic = (this._currentAction == WallDoor.Actions.Closing);
			this._currentAction = WallDoor.Actions.Idle;
		}

		protected void FakeAttach()
		{
			base.state.AddCallback("Addition", new PropertyCallbackSimple(this.OnAdditionChange));
			if (base.entity.isOwner)
			{
				base.state.Addition = (int)base.GetComponentInParent<WallChunkArchitect>().Addition;
			}
		}

		protected void OnAdditionChange()
		{
			base.GetComponentInParent<WallChunkArchitect>().Addition = (WallChunkArchitect.Additions)base.state.Addition;
			WallDoor.Actions activeDoorStatusAction = this.GetActiveDoorStatusAction();
			if (activeDoorStatusAction != this._currentAction)
			{
				base.enabled = true;
				if (this._currentAction == WallDoor.Actions.Idle)
				{
					this._alpha = 0f;
				}
				else
				{
					this._alpha = 1f - this._alpha;
				}
				this._currentAction = activeDoorStatusAction;
				if (this._targetRb)
				{
					this._targetRb.isKinematic = (this._currentAction == WallDoor.Actions.Closing);
					if (this._targetRb.isKinematic)
					{
						this._targetRb.transform.localRotation = Quaternion.identity;
					}
				}
				this.PlaySfx(activeDoorStatusAction);
			}
		}

		public Transform _target;

		public Rigidbody _targetRb;

		public GameObject _sheenIcon;

		public GameObject _pickUpIcon;

		public GameObject _sheenIconUnlock;

		public GameObject _pickUpIconUnlock;

		protected float _alpha;

		protected WallDoor.Actions _currentAction;

		private static readonly Vector3 OpenedPosition = new Vector3(0f, 0f, 0f);

		private static readonly Vector3 ClosedPosition = new Vector3(0f, -0.61f, 0f);

		private static readonly Quaternion OpenedRotation = Quaternion.Euler(0f, 0f, 0f);

		private static readonly Quaternion ClosedRotation = Quaternion.Euler(0f, -85f, 0f);

		public enum Actions
		{
			Idle,
			Openning,
			Closing
		}
	}
}
