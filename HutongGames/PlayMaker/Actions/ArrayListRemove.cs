using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Remove an element of a PlayMaker Array List Proxy component")]
	[ActionCategory("ArrayMaker/ArrayList")]
	public class ArrayListRemove : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.notFoundEvent = null;
			this.variable = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.DoRemoveFromArrayList();
			}
			base.Finish();
		}

		
		public void DoRemoveFromArrayList()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			if (!this.proxy.Remove(PlayMakerUtils.GetValueFromFsmVar(base.Fsm, this.variable), this.variable.Type.ToString(), false))
			{
				base.Fsm.Event(this.notFoundEvent);
			}
		}

		
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[RequiredField]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component (necessary if several component coexists on the same GameObject)")]
		[UIHint(UIHint.FsmString)]
		public FsmString reference;

		
		[ActionSection("Data")]
		[Tooltip("The type of Variable to remove.")]
		public FsmVar variable;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("Event sent if this arraList does not contains that element ( described below)")]
		[ActionSection("Result")]
		public FsmEvent notFoundEvent;
	}
}
