using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Logic)]
	[Tooltip("Tests if a GameObject Variable is active.")]
	public class GameObjectIsActiveInHierarchy : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.isActive = null;
			this.isNotActive = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoIsGameObjectActive();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoIsGameObjectActive();
		}

		
		private void DoIsGameObjectActive()
		{
			bool flag = this.gameObject.Value == null;
			bool flag2 = false;
			if (!flag)
			{
				flag2 = this.gameObject.Value.activeInHierarchy;
			}
			if (this.storeResult != null)
			{
				this.storeResult.Value = flag2;
			}
			base.Fsm.Event((!flag2) ? this.isNotActive : this.isActive);
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The GameObject variable to test.")]
		public FsmGameObject gameObject;

		
		[Tooltip("Event to send if the GamObject is active.")]
		public FsmEvent isActive;

		
		[Tooltip("Event to send if the GamObject is NOT active.")]
		public FsmEvent isNotActive;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a bool variable.")]
		public FsmBool storeResult;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;
	}
}
