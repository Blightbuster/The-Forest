using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Check if a key exists in a PlayMaker HashTable Proxy component (PlayMakerHashTablePRoxy)")]
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

		
		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("The Key value to check for")]
		public FsmString key;

		
		[UIHint(UIHint.FsmBool)]
		[Tooltip("Store the result of the test")]
		public FsmBool containsKey;

		
		[ActionSection("Result")]
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is found")]
		public FsmEvent keyFoundEvent;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is not found")]
		public FsmEvent keyNotFoundEvent;
	}
}
