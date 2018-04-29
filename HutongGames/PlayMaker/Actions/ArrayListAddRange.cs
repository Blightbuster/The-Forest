using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Add several items to a PlayMaker Array List Proxy component")]
	[ActionCategory("ArrayMaker/ArrayList")]
	public class ArrayListAddRange : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.variables = new FsmVar[2];
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.DoArrayListAddRange();
			}
			base.Finish();
		}

		
		public void DoArrayListAddRange()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			foreach (FsmVar fsmVar in this.variables)
			{
				this.proxy.Add(PlayMakerUtils.GetValueFromFsmVar(base.Fsm, fsmVar), fsmVar.Type.ToString(), true);
			}
		}

		
		[RequiredField]
		[ActionSection("Set up")]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.FsmString)]
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component (necessary if several component coexists on the same GameObject)")]
		public FsmString reference;

		
		[ActionSection("Data")]
		[RequiredField]
		[Tooltip("The variables to add.")]
		public FsmVar[] variables;
	}
}
