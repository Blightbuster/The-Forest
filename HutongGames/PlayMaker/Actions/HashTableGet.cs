using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets an item from a PlayMaker HashTable Proxy component")]
	[ActionCategory("ArrayMaker/HashTable")]
	public class HashTableGet : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.key = null;
			this.KeyFoundEvent = null;
			this.KeyNotFoundEvent = null;
			this.result = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.Get();
			}
			base.Finish();
		}

		
		public void Get()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			if (!this.proxy.hashTable.ContainsKey(this.key.Value))
			{
				base.Fsm.Event(this.KeyNotFoundEvent);
				return;
			}
			PlayMakerUtils.ApplyValueToFsmVar(base.Fsm, this.result, this.proxy.hashTable[this.key.Value]);
			base.Fsm.Event(this.KeyFoundEvent);
		}

		
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("The Key value for that hash set")]
		public FsmString key;

		
		[UIHint(UIHint.Variable)]
		[ActionSection("Result")]
		public FsmVar result;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is found")]
		public FsmEvent KeyFoundEvent;

		
		[Tooltip("The event to trigger when key is not found")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent KeyNotFoundEvent;
	}
}
