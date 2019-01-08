using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Gets an item from a PlayMaker HashTable Proxy component")]
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

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("The Key value for that hash set")]
		public FsmString key;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmVar result;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is found")]
		public FsmEvent KeyFoundEvent;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is not found")]
		public FsmEvent KeyNotFoundEvent;
	}
}
