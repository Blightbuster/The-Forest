﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Set key/value pairs to a PlayMaker HashTable Proxy component (PlayMakerHashTableProxy)")]
	public class HashTableSetMany : HashTableActions
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.keys = null;
			this.variables = null;
		}

		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.SetHashTable();
			}
			base.Finish();
		}

		public void SetHashTable()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			for (int i = 0; i < this.keys.Length; i++)
			{
				this.proxy.hashTable[this.keys[i].Value] = PlayMakerUtils.GetValueFromFsmVar(base.Fsm, this.variables[i]);
			}
		}

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[ActionSection("Data")]
		[CompoundArray("Count", "Key", "Value")]
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("The Key values for that hash set")]
		public FsmString[] keys;

		[Tooltip("The variable to set.")]
		public FsmVar[] variables;
	}
}
