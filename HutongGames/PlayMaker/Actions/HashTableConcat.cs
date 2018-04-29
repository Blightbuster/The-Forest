using System;
using System.Collections;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Concat joins two or more hashTable proxy components. if a target is specified, the method use the target store the concatenation, else the ")]
	[ActionCategory("ArrayMaker/HashTable")]
	public class HashTableConcat : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.hashTableGameObjectTargets = null;
			this.referenceTargets = null;
			this.overwriteExistingKey = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.DoHashTableConcat(this.proxy.hashTable);
			}
			base.Finish();
		}

		
		public void DoHashTableConcat(Hashtable source)
		{
			if (!base.isProxyValid())
			{
				return;
			}
			for (int i = 0; i < this.hashTableGameObjectTargets.Length; i++)
			{
				if (base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.hashTableGameObjectTargets[i]), this.referenceTargets[i].Value) && base.isProxyValid())
				{
					foreach (object key in this.proxy.hashTable.Keys)
					{
						if (source.ContainsKey(key))
						{
							if (this.overwriteExistingKey.Value)
							{
								source[key] = this.proxy.hashTable[key];
							}
						}
						else
						{
							source[key] = this.proxy.hashTable[key];
						}
					}
				}
			}
		}

		
		[ActionSection("Storage")]
		[RequiredField]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component to store the concatenation ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[CompoundArray("HashTables", "HashTable GameObject", "Reference")]
		[ActionSection("HashTables to concatenate")]
		[RequiredField]
		[Tooltip("The GameObject with the PlayMaker HashTable Proxy component to copy to")]
		[ObjectType(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault[] hashTableGameObjectTargets;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component to copy to ( necessary if several component coexists on the same GameObject")]
		public FsmString[] referenceTargets;

		
		[Tooltip("Overwrite existing key with new values")]
		[UIHint(UIHint.FsmBool)]
		public FsmBool overwriteExistingKey;
	}
}
