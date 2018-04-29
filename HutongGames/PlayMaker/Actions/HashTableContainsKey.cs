using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Check if a key exists in a PlayMaker HashTable Proxy component (PlayMakerHashTablePRoxy)")]
	[ActionCategory("ArrayMaker/HashTable")]
	public class HashTableContainsKey : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.key = null;
			this.containsKey = null;
			this.keyFoundEvent = null;
			this.keyNotFoundEvent = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.checkIfContainsKey();
			}
			base.Finish();
		}

		
		public void checkIfContainsKey()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.containsKey.Value = this.proxy.hashTable.ContainsKey(this.key.Value);
			if (this.containsKey.Value)
			{
				base.Fsm.Event(this.keyFoundEvent);
			}
			else
			{
				base.Fsm.Event(this.keyNotFoundEvent);
			}
		}

		
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[Tooltip("The Key value to check for")]
		[UIHint(UIHint.FsmString)]
		[RequiredField]
		public FsmString key;

		
		[UIHint(UIHint.FsmBool)]
		[Tooltip("Store the result of the test")]
		public FsmBool containsKey;

		
		[Tooltip("The event to trigger when key is found")]
		[UIHint(UIHint.FsmEvent)]
		[ActionSection("Result")]
		public FsmEvent keyFoundEvent;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is not found")]
		public FsmEvent keyNotFoundEvent;
	}
}
