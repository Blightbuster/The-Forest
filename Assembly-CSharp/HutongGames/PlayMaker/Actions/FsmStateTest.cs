using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Logic)]
	[Tooltip("Tests if an FSM is in the specified State.")]
	public class FsmStateTest : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = null;
			this.stateName = null;
			this.trueEvent = null;
			this.falseEvent = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		public override void OnEnter()
		{
			this.DoFsmStateTest();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoFsmStateTest();
		}

		private void DoFsmStateTest()
		{
			GameObject value = this.gameObject.Value;
			if (value == null)
			{
				return;
			}
			if (value != this.previousGo)
			{
				this.fsm = ActionHelpers.GetGameObjectFsm(value, this.fsmName.Value);
				this.previousGo = value;
			}
			if (this.fsm == null)
			{
				return;
			}
			bool value2 = false;
			if (this.fsm.ActiveStateName == this.stateName.Value)
			{
				base.Fsm.Event(this.trueEvent);
				value2 = true;
			}
			else
			{
				base.Fsm.Event(this.falseEvent);
			}
			this.storeResult.Value = value2;
		}

		[RequiredField]
		[Tooltip("The GameObject that owns the FSM.")]
		public FsmGameObject gameObject;

		[UIHint(UIHint.FsmName)]
		[Tooltip("Optional name of Fsm on Game Object. Useful if there is more than one FSM on the GameObject.")]
		public FsmString fsmName;

		[RequiredField]
		[Tooltip("Check to see if the FSM is in this state.")]
		public FsmString stateName;

		[Tooltip("Event to send if the FSM is in the specified state.")]
		public FsmEvent trueEvent;

		[Tooltip("Event to send if the FSM is NOT in the specified state.")]
		public FsmEvent falseEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result of this test in a bool variable. Useful if other actions depend on this test.")]
		public FsmBool storeResult;

		[Tooltip("Repeat every frame. Useful if you're waiting for a particular state.")]
		public bool everyFrame;

		private GameObject previousGo;

		private PlayMakerFSM fsm;
	}
}
