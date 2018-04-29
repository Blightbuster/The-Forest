using System;
using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Concat joins two or more arrayList proxy components. if a target is specified, the method use the target store the concatenation, else the ")]
	[ActionCategory("ArrayMaker/ArrayList")]
	public class ArrayListConcat : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.arrayListGameObjectTargets = null;
			this.referenceTargets = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.DoArrayListConcat(this.proxy.arrayList);
			}
			base.Finish();
		}

		
		public void DoArrayListConcat(ArrayList source)
		{
			if (!base.isProxyValid())
			{
				return;
			}
			for (int i = 0; i < this.arrayListGameObjectTargets.Length; i++)
			{
				if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.arrayListGameObjectTargets[i]), this.referenceTargets[i].Value) && base.isProxyValid())
				{
					foreach (object value in this.proxy.arrayList)
					{
						source.Add(value);
						Debug.Log("count " + source.Count);
					}
				}
			}
		}

		
		[ActionSection("Storage")]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[RequiredField]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component to store the concatenation ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[ObjectType(typeof(PlayMakerArrayListProxy))]
		[ActionSection("ArrayLists to concatenate")]
		[CompoundArray("ArrayLists", "ArrayList GameObject", "Reference")]
		[RequiredField]
		[Tooltip("The GameObject with the PlayMaker ArrayList Proxy component to copy to")]
		public FsmOwnerDefault[] arrayListGameObjectTargets;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component to copy to ( necessary if several component coexists on the same GameObject")]
		public FsmString[] referenceTargets;
	}
}
