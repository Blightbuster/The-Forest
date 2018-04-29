using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Gets an item from a PlayMaker ArrayList Proxy component")]
	public class ArrayListGet : ArrayListActions
	{
		
		public override void Reset()
		{
			this.atIndex = null;
			this.gameObject = null;
			this.failureEvent = null;
			this.result = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.GetItemAtIndex();
			}
			base.Finish();
		}

		
		public void GetItemAtIndex()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			object value = null;
			try
			{
				value = this.proxy.arrayList[this.atIndex.Value];
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
				base.Fsm.Event(this.failureEvent);
				return;
			}
			PlayMakerUtils.ApplyValueToFsmVar(base.Fsm, this.result, value);
		}

		
		[RequiredField]
		[ActionSection("Set up")]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[UIHint(UIHint.FsmInt)]
		[Tooltip("The index to retrieve the item from")]
		public FsmInt atIndex;

		
		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmVar result;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger if the action fails ( likely and index is out of range exception)")]
		public FsmEvent failureEvent;
	}
}
