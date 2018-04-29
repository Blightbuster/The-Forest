﻿using System;
using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Each time this action is called it gets the next item from a PlayMaker HashTable Proxy component. \nThis lets you quickly loop through all the children of an object to perform actions on them.\nNOTE: To get to specific item use HashTableGet instead.")]
	public class HashTableGetNext : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.startIndex = null;
			this.endIndex = null;
			this.loopEvent = null;
			this.finishedEvent = null;
			this.failureEvent = null;
			this.result = null;
		}

		
		public override void OnEnter()
		{
			if (this.nextItemIndex == 0)
			{
				if (!base.SetUpHashTableProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
				{
					base.Fsm.Event(this.failureEvent);
					base.Finish();
				}
				this._keys = new ArrayList(this.proxy.hashTable.Keys);
				if (this.startIndex.Value > 0)
				{
					this.nextItemIndex = this.startIndex.Value;
				}
			}
			this.DoGetNextItem();
			base.Finish();
		}

		
		private void DoGetNextItem()
		{
			if (this.nextItemIndex >= this._keys.Count)
			{
				this.nextItemIndex = 0;
				base.Fsm.Event(this.finishedEvent);
				return;
			}
			this.GetItemAtIndex();
			if (this.nextItemIndex >= this._keys.Count)
			{
				this.nextItemIndex = 0;
				base.Fsm.Event(this.finishedEvent);
				return;
			}
			if (this.endIndex.Value > 0 && this.nextItemIndex >= this.endIndex.Value)
			{
				this.nextItemIndex = 0;
				base.Fsm.Event(this.finishedEvent);
				return;
			}
			this.nextItemIndex++;
			if (this.loopEvent != null)
			{
				base.Fsm.Event(this.loopEvent);
			}
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
				value = this.proxy.hashTable[this._keys[this.nextItemIndex]];
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				base.Fsm.Event(this.failureEvent);
				return;
			}
			this.key.Value = (string)this._keys[this.nextItemIndex];
			PlayMakerUtils.ApplyValueToFsmVar(base.Fsm, this.result, value);
		}

		
		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[Tooltip("From where to start iteration, leave to 0 to start from the beginning")]
		public FsmInt startIndex;

		
		[Tooltip("When to end iteration, leave to 0 to iterate until the end")]
		public FsmInt endIndex;

		
		[Tooltip("Event to send to get the next item.")]
		public FsmEvent loopEvent;

		
		[Tooltip("Event to send when there are no more items.")]
		public FsmEvent finishedEvent;

		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger if the action fails ( likely and index is out of range exception)")]
		public FsmEvent failureEvent;

		
		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmString key;

		
		[UIHint(UIHint.Variable)]
		public FsmVar result;

		
		private ArrayList _keys;

		
		private int nextItemIndex;
	}
}
