using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Logic)]
	[Tooltip("Tests if the value of a GameObject variable changed. Use this to send an event on change, or store a bool that can be used in other operations.")]
	public class GameObjectChanged : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObjectVariable = null;
			this.changedEvent = null;
			this.storeResult = null;
		}

		
		public override void OnEnter()
		{
			if (this.gameObjectVariable.IsNone)
			{
				base.Finish();
				return;
			}
			this.previousValue = this.gameObjectVariable.Value;
		}

		
		public override void OnUpdate()
		{
			this.storeResult.Value = false;
			if (this.gameObjectVariable.Value != this.previousValue)
			{
				this.storeResult.Value = true;
				base.Fsm.Event(this.changedEvent);
			}
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The GameObject variable to watch for a change.")]
		public FsmGameObject gameObjectVariable;

		
		[Tooltip("Event to send if the variable changes.")]
		public FsmEvent changedEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Set to True if the variable changes.")]
		public FsmBool storeResult;

		
		private GameObject previousValue;
	}
}
