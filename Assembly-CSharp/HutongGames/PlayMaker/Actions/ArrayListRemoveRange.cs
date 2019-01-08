using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Remove the specified range of elements from a PlayMaker ArrayList Proxy component")]
	public class ArrayListRemoveRange : ArrayListActions
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.index = null;
			this.count = null;
			this.failureEvent = null;
		}

		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.doArrayListRemoveRange();
			}
			base.Finish();
		}

		public void doArrayListRemoveRange()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			try
			{
				this.proxy.arrayList.RemoveRange(this.index.Value, this.count.Value);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				base.Fsm.Event(this.failureEvent);
			}
		}

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[UIHint(UIHint.FsmInt)]
		[Tooltip("The zero-based index of the first element of the range of elements to remove. This value is between 0 and the array.count minus count (inclusive)")]
		public FsmInt index;

		[UIHint(UIHint.FsmInt)]
		[Tooltip("The number of elements to remove. This value is between 0 and the difference between the array.count minus the index ( inclusive )")]
		public FsmInt count;

		[ActionSection("Result")]
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger if the removeRange throw errors")]
		public FsmEvent failureEvent;
	}
}
