﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Add several items to a PlayMaker Array List Proxy component")]
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

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component (necessary if several component coexists on the same GameObject)")]
		[UIHint(UIHint.FsmString)]
		public FsmString reference;

		[ActionSection("Data")]
		[RequiredField]
		[Tooltip("The variables to add.")]
		public FsmVar[] variables;
	}
}
