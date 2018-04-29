using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Count the number of items ( key/value pairs) in a PlayMaker HashTable Proxy component (PlayMakerHashTableProxy).")]
	[ActionCategory("ArrayMaker/HashTable")]
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

		
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[RequiredField]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[RequiredField]
		[Tooltip("The number of items in that hashTable")]
		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmInt count;
	}
}
