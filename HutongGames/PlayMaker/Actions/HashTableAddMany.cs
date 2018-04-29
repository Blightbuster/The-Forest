using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Add key/value pairs to a PlayMaker HashTable Proxy component (PlayMakerHashTableProxy).")]
	public class HashTableAddMany : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.keys = null;
			this.variables = null;
			this.successEvent = null;
			this.keyExistsAlreadyEvent = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				if (this.keyExistsAlreadyEvent != null)
				{
					foreach (FsmString fsmString in this.keys)
					{
						if (this.proxy.hashTable.ContainsKey(fsmString.Value))
						{
							base.Fsm.Event(this.keyExistsAlreadyEvent);
							base.Finish();
						}
					}
				}
				this.AddToHashTable();
				base.Fsm.Event(this.successEvent);
			}
			base.Finish();
		}

		
		public void AddToHashTable()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			for (int i = 0; i < this.keys.Length; i++)
			{
				this.proxy.hashTable.Add(this.keys[i].Value, PlayMakerUtils.GetValueFromFsmVar(base.Fsm, this.variables[i]));
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[CompoundArray("Count", "Key", "Value")]
		[ActionSection("Data")]
		[Tooltip("The Key")]
		public FsmString[] keys;

		
		[RequiredField]
		[Tooltip("The value for that key")]
		public FsmVar[] variables;

		
		[Tooltip("The event to trigger when elements are added")]
		[ActionSection("Result")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent successEvent;

		
		[Tooltip("The event to trigger when elements exists already")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent keyExistsAlreadyEvent;
	}
}
