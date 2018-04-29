using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Compares 2 Game Objects and sends Events based on the result.")]
	[ActionCategory(ActionCategory.Logic)]
	public class GameObjectCompare : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObjectVariable = null;
			this.compareTo = null;
			this.equalEvent = null;
			this.notEqualEvent = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoGameObjectCompare();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoGameObjectCompare();
		}

		
		private void DoGameObjectCompare()
		{
			bool flag = base.Fsm.GetOwnerDefaultTarget(this.gameObjectVariable) == this.compareTo.Value;
			this.storeResult.Value = flag;
			if (flag && this.equalEvent != null)
			{
				base.Fsm.Event(this.equalEvent);
			}
			else if (!flag && this.notEqualEvent != null)
			{
				base.Fsm.Event(this.notEqualEvent);
			}
		}

		
		[UIHint(UIHint.Variable)]
		[Tooltip("A Game Object variable to compare.")]
		[RequiredField]
		[Title("Game Object")]
		public FsmOwnerDefault gameObjectVariable;

		
		[Tooltip("Compare the variable with this Game Object")]
		[RequiredField]
		public FsmGameObject compareTo;

		
		[Tooltip("Send this event if Game Objects are equal")]
		public FsmEvent equalEvent;

		
		[Tooltip("Send this event if Game Objects are not equal")]
		public FsmEvent notEqualEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result of the check in a Bool Variable. (True if equal, false if not equal).")]
		public FsmBool storeResult;

		
		[Tooltip("Repeat every frame. Useful if you're waiting for a true or false result.")]
		public bool everyFrame;
	}
}
