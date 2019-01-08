using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Count the number of items ( key/value pairs) in a PlayMaker HashTable Proxy component (PlayMakerHashTableProxy).")]
	public class HashTableCount : HashTableActions
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.count = null;
		}

		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.doHashTableCount();
			}
			base.Finish();
		}

		public void doHashTableCount()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.count.Value = this.proxy.hashTable.Count;
		}

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The number of items in that hashTable")]
		public FsmInt count;
	}
}
