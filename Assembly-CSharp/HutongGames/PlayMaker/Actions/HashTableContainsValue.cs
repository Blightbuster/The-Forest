﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Check if a value exists in a PlayMaker HashTable Proxy component (PlayMakerHashTablePRoxy)")]
	public class HashTableContainsValue : HashTableActions
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.containsValue = null;
			this.valueFoundEvent = null;
			this.valueNotFoundEvent = null;
			this.variable = null;
		}

		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.doContainsValue();
			}
			base.Finish();
		}

		public void doContainsValue()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.containsValue.Value = this.proxy.hashTable.ContainsValue(PlayMakerUtils.GetValueFromFsmVar(base.Fsm, this.variable));
			if (this.containsValue.Value)
			{
				base.Fsm.Event(this.valueFoundEvent);
			}
			else
			{
				base.Fsm.Event(this.valueNotFoundEvent);
			}
		}

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component (necessary if several component coexists on the same GameObject)")]
		public FsmString reference;

		[Tooltip("The variable to check for.")]
		public FsmVar variable;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result of the test")]
		public FsmBool containsValue;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when value is found")]
		public FsmEvent valueFoundEvent;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when value is not found")]
		public FsmEvent valueNotFoundEvent;
	}
}
