﻿using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Remove an item by key ( key/value pairs) in a PlayMaker HashTable Proxy component (PlayMakerHashTableProxy).")]
	[ActionCategory("ArrayMaker/HashTable")]
	public class HashTableRemove : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.key = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.doHashTableRemove();
			}
			base.Finish();
		}

		
		public void doHashTableRemove()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.proxy.hashTable.Remove(this.key.Value);
		}

		
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[RequiredField]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[RequiredField]
		[Tooltip("The item key in that hashTable")]
		public FsmString key;
	}
}
