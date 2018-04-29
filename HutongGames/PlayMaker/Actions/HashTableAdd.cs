using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Add an key/value pair to a PlayMaker HashTable Proxy component (PlayMakerHashTableProxy).")]
	[ActionCategory("ArrayMaker/HashTable")]
	public class HashTableAdd : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.key = null;
			this.variable = null;
			this.successEvent = null;
			this.keyExistsAlreadyEvent = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				if (this.proxy.hashTable.ContainsKey(this.key.Value))
				{
					base.Fsm.Event(this.keyExistsAlreadyEvent);
				}
				else
				{
					this.AddToHashTable();
					base.Fsm.Event(this.successEvent);
				}
			}
			base.Finish();
		}

		
		public void AddToHashTable()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.proxy.hashTable.Add(this.key.Value, PlayMakerUtils.GetValueFromFsmVar(base.Fsm, this.variable));
		}

		
		[ActionSection("Set up")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[Tooltip("The Key value for that hash set")]
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[ActionSection("Data")]
		public FsmString key;

		
		[Tooltip("The variable to add.")]
		[RequiredField]
		public FsmVar variable;

		
		[Tooltip("The event to trigger when element is added")]
		[ActionSection("Result")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent successEvent;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when element exists already")]
		public FsmEvent keyExistsAlreadyEvent;
	}
}
