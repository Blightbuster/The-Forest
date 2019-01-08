﻿using System;
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

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[ActionSection("Result")]
		[Tooltip("The random item data picked from the array")]
		[UIHint(UIHint.Variable)]
		public FsmVar randomItem;

		[Tooltip("The random item index picked from the array")]
		[UIHint(UIHint.Variable)]
		public FsmInt randomIndex;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger if the action fails ( likely and index is out of range exception)")]
		public FsmEvent failureEvent;
	}
}
