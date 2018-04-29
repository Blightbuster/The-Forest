using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Gets a random item from a PlayMaker ArrayList Proxy component")]
	public class ArrayListGetRandom : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.failureEvent = null;
			this.randomItem = null;
			this.randomIndex = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.GetRandomItem();
			}
			base.Finish();
		}

		
		public void GetRandomItem()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			int num = UnityEngine.Random.Range(0, this.proxy.arrayList.Count);
			object value = null;
			try
			{
				value = this.proxy.arrayList[num];
			}
			catch (Exception ex)
			{
				Debug.LogWarning(ex.Message);
				base.Fsm.Event(this.failureEvent);
				return;
			}
			this.randomIndex.Value = num;
			if (!PlayMakerUtils.ApplyValueToFsmVar(base.Fsm, this.randomItem, value))
			{
				Debug.LogWarning("ApplyValueToFsmVar failed");
				base.Fsm.Event(this.failureEvent);
				return;
			}
		}

		
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[RequiredField]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[Tooltip("The random item data picked from the array")]
		[UIHint(UIHint.Variable)]
		[ActionSection("Result")]
		public FsmVar randomItem;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("The random item index picked from the array")]
		public FsmInt randomIndex;

		
		[Tooltip("The event to trigger if the action fails ( likely and index is out of range exception)")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent failureEvent;
	}
}
