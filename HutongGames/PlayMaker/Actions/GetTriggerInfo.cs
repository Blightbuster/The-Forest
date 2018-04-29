using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets info on the last Trigger event and store in variables.")]
	[ActionCategory(ActionCategory.Physics)]
	public class GetTriggerInfo : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObjectHit = null;
			this.physicsMaterialName = null;
		}

		
		private void StoreTriggerInfo()
		{
			if (base.Fsm.TriggerCollider == null)
			{
				return;
			}
			this.gameObjectHit.Value = base.Fsm.TriggerCollider.gameObject;
			this.physicsMaterialName.Value = base.Fsm.TriggerCollider.material.name;
		}

		
		public override void OnEnter()
		{
			this.StoreTriggerInfo();
			base.Finish();
		}

		
		[UIHint(UIHint.Variable)]
		public FsmGameObject gameObjectHit;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Useful for triggering different effects. Audio, particles...")]
		public FsmString physicsMaterialName;
	}
}
