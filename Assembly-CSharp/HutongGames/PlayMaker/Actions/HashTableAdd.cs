using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Add an key/value pair to a PlayMaker HashTable Proxy component (PlayMakerHashTableProxy).")]
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
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[ActionSection("Data")]
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("The Key value for that hash set")]
		public FsmString key;

		[RequiredField]
		[Tooltip("The variable to add.")]
		public FsmVar variable;

		[ActionSection("Result")]
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when element is added")]
		public FsmEvent successEvent;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when element exists already")]
		public FsmEvent keyExistsAlreadyEvent;
	}
}
