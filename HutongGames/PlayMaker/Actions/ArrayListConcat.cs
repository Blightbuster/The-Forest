using System;
using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Concat joins two or more arrayList proxy components. if a target is specified, the method use the target store the concatenation, else the ")]
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
					IEnumerator enumerator = this.proxy.arrayList.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							object value = enumerator.Current;
							source.Add(value);
							Debug.Log("count " + source.Count);
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}

		
		[ActionSection("Storage")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component to store the concatenation ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[ActionSection("ArrayLists to concatenate")]
		[CompoundArray("ArrayLists", "ArrayList GameObject", "Reference")]
		[RequiredField]
		[Tooltip("The GameObject with the PlayMaker ArrayList Proxy component to copy to")]
		[ObjectType(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault[] arrayListGameObjectTargets;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component to copy to ( necessary if several component coexists on the same GameObject")]
		public FsmString[] referenceTargets;
	}
}
