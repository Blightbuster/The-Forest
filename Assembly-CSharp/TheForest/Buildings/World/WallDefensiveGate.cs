using System;
using System.Collections;
using TheForest.Buildings.Creation;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	public class WallDefensiveGate : WallDoor
	{
		protected override IEnumerator Start()
		{
			base.enabled = false;
			while (!(this._gate = base.GetComponentInParent<WallDefensiveGateArchitect>()))
			{
				yield return null;
			}
			if (WallDefensiveGate.AutoOpenDoor)
			{
				WallDefensiveGate.AutoOpenDoor = false;
				LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, 0.4f);
				this._currentAction = WallDoor.Actions.Openning;
				base.enabled = true;
			}
			else if (BoltNetwork.isClient)
			{
				this._currentAction = base.GetActiveDoorStatusAction();
				base.enabled = true;
			}
			else
			{
				this._currentAction = base.GetActiveDoorStatusAction();
				this.FinalizeDoorStatus();
			}
			if (BoltNetwork.isRunning)
			{
				while (!base.entity.isAttached)
				{
					yield return null;
				}
				base.FakeAttach();
			}
			yield break;
		}

		protected override void Update()
		{
			if (this._currentAction == WallDoor.Actions.Idle)
			{
				if (this.CanToggle() && TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (!BoltNetwork.isRunning)
					{
						WallDoor.Actions currentAction = base.ToggleDoorStatusAction(true);
						this._currentAction = currentAction;
						this._alpha = 0f;
					}
					else
					{
						WallDoor.Actions currentAction = base.ToggleDoorStatusAction(true);
					}
				}
				base.RefreshIcons();
			}
			else
			{
				this._alpha += Time.deltaTime;
				if (this._alpha < 1f)
				{
					if (this._currentAction == WallDoor.Actions.Openning)
					{
						if (this._gate.Addition == WallChunkArchitect.Additions.Door1)
						{
							this._target1.localRotation = Quaternion.Slerp(WallDefensiveGate.ClosedRotation1, WallDefensiveGate.OpenedRotation, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
							if (this._target2)
							{
								this._target2.localRotation = Quaternion.Slerp(WallDefensiveGate.ClosedRotation2, WallDefensiveGate.OpenedRotation, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
							}
						}
						else
						{
							this._target1.localRotation = Quaternion.Slerp(WallDefensiveGate.ClosedRotation2, WallDefensiveGate.OpenedRotation, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
							if (this._target2)
							{
								this._target2.localRotation = Quaternion.Slerp(WallDefensiveGate.ClosedRotation1, WallDefensiveGate.OpenedRotation, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
							}
						}
					}
					else if (this._gate.Addition == WallChunkArchitect.Additions.LockedDoor1)
					{
						this._target1.localRotation = Quaternion.Slerp(WallDefensiveGate.OpenedRotation, WallDefensiveGate.ClosedRotation1, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
						if (this._target2)
						{
							this._target2.localRotation = Quaternion.Slerp(WallDefensiveGate.OpenedRotation, WallDefensiveGate.ClosedRotation2, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
						}
					}
					else
					{
						this._target1.localRotation = Quaternion.Slerp(WallDefensiveGate.OpenedRotation, WallDefensiveGate.ClosedRotation2, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
						if (this._target2)
						{
							this._target2.localRotation = Quaternion.Slerp(WallDefensiveGate.OpenedRotation, WallDefensiveGate.ClosedRotation1, MathEx.Easing.EaseInOutQuad(this._alpha, 0f, 1f, 1f));
						}
					}
				}
				else
				{
					this.FinalizeDoorStatus();
					base.RefreshIcons();
					base.enabled = (Grabber.FocusedItemGO == base.gameObject);
				}
			}
		}

		protected override void PlaySfx(WallDoor.Actions action)
		{
			if (LocalPlayer.Sfx)
			{
				if (action == WallDoor.Actions.Closing)
				{
					LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, 0.4f);
				}
				else
				{
					LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, 0.4f);
				}
			}
		}

		protected override bool CanToggle()
		{
			return this._currentAction == WallDoor.Actions.Idle;
		}

		protected override void FinalizeDoorStatus()
		{
			if (this._gate.Addition == WallChunkArchitect.Additions.Door1 || this._gate.Addition == WallChunkArchitect.Additions.LockedDoor1)
			{
				this._target1.localRotation = ((this._currentAction != WallDoor.Actions.Openning) ? WallDefensiveGate.ClosedRotation1 : WallDefensiveGate.OpenedRotation);
				if (this._target2)
				{
					this._target2.localRotation = ((this._currentAction != WallDoor.Actions.Openning) ? WallDefensiveGate.ClosedRotation2 : WallDefensiveGate.OpenedRotation);
				}
			}
			else
			{
				this._target1.localRotation = ((this._currentAction != WallDoor.Actions.Openning) ? WallDefensiveGate.ClosedRotation2 : WallDefensiveGate.OpenedRotation);
				if (this._target2)
				{
					this._target2.localRotation = ((this._currentAction != WallDoor.Actions.Openning) ? WallDefensiveGate.ClosedRotation1 : WallDefensiveGate.OpenedRotation);
				}
			}
			this._currentAction = WallDoor.Actions.Idle;
			this._previousAction = this._currentAction;
			enableOnLoad componentInChildren = base.transform.GetComponentInChildren<enableOnLoad>();
			if (componentInChildren)
			{
				componentInChildren.StartCoroutine("doEnable");
			}
		}

		public Transform _target1;

		public Transform _target2;

		private WallDoor.Actions _previousAction;

		private WallDefensiveGateArchitect _gate;

		private static readonly Quaternion OpenedRotation = Quaternion.Euler(0f, -90f, 0f);

		private static readonly Quaternion ClosedRotation1 = Quaternion.Euler(0f, 0f, 0f);

		private static readonly Quaternion ClosedRotation2 = Quaternion.Euler(0f, -180f, 0f);

		public static bool AutoOpenDoor;
	}
}
