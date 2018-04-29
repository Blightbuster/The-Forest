using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Return the index of an item from a PlayMaker Array List Proxy component. Can search within a range")]
	[ActionCategory("ArrayMaker/ArrayList")]
	public class ArrayListIndexOf : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.startIndex = null;
			this.count = null;
			this.itemFound = null;
			this.itemNotFound = null;
			this.variable = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.DoArrayListIndexOf();
			}
			base.Finish();
		}

		
		public void DoArrayListIndexOf()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			object valueFromFsmVar = PlayMakerUtils.GetValueFromFsmVar(base.Fsm, this.variable);
			int num = -1;
			try
			{
				if (this.startIndex == null)
				{
					num = this.proxy.arrayList.IndexOf(valueFromFsmVar);
				}
				else if (this.count == null || this.count.Value == 0)
				{
					if (this.startIndex.Value < 0 || this.startIndex.Value >= this.proxy.arrayList.Count)
					{
						this.LogError("start index out of range");
						return;
					}
					num = this.proxy.arrayList.IndexOf(valueFromFsmVar, this.startIndex.Value);
				}
				else
				{
					if (this.startIndex.Value < 0 || this.startIndex.Value >= this.proxy.arrayList.Count - this.count.Value)
					{
						this.LogError("start index and count out of range");
						return;
					}
					num = this.proxy.arrayList.IndexOf(valueFromFsmVar, this.startIndex.Value, this.count.Value);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				base.Fsm.Event(this.failureEvent);
				return;
			}
			this.indexOf.Value = num;
			if (num == -1)
			{
				base.Fsm.Event(this.itemNotFound);
			}
			else
			{
				base.Fsm.Event(this.itemFound);
			}
		}

		
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		[RequiredField]
		[ActionSection("Set up")]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component (necessary if several component coexists on the same GameObject)")]
		[UIHint(UIHint.FsmString)]
		public FsmString reference;

		
		[UIHint(UIHint.FsmInt)]
		[Tooltip("Optional start index to search from: set to 0 to ignore")]
		public FsmInt startIndex;

		
		[Tooltip("Optional amount of elements to search within: set to 0 to ignore")]
		[UIHint(UIHint.FsmInt)]
		public FsmInt count;

		
		[RequiredField]
		[ActionSection("Data")]
		[Tooltip("The variable to get the index of.")]
		public FsmVar variable;

		
		[Tooltip("The index of the item described below")]
		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmInt indexOf;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("Optional Event sent if this arrayList contains that element ( described below)")]
		public FsmEvent itemFound;

		
		[Tooltip("Optional Event sent if this arrayList does not contains that element ( described below)")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent itemNotFound;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("Optional Event to trigger if the action fails ( likely an out of range exception when using wrong values for index and/or count)")]
		public FsmEvent failureEvent;
	}
}
