using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Finds an ArrayList by reference. Warning: this function can be very slow.")]
	public class FindArrayList : CollectionsActions
	{
		
		public override void Reset()
		{
			this.ArrayListReference = string.Empty;
			this.store = null;
			this.foundEvent = null;
			this.notFoundEvent = null;
		}

		
		public override void OnEnter()
		{
			PlayMakerArrayListProxy[] array = UnityEngine.Object.FindObjectsOfType(typeof(PlayMakerArrayListProxy)) as PlayMakerArrayListProxy[];
			foreach (PlayMakerArrayListProxy playMakerArrayListProxy in array)
			{
				if (playMakerArrayListProxy.referenceName == this.ArrayListReference.Value)
				{
					this.store.Value = playMakerArrayListProxy.gameObject;
					base.Fsm.Event(this.foundEvent);
					return;
				}
			}
			this.store.Value = null;
			base.Fsm.Event(this.notFoundEvent);
			base.Finish();
		}

		
		[ActionSection("Set up")]
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component")]
		public FsmString ArrayListReference;

		
		[ActionSection("Result")]
		[RequiredField]
		[Tooltip("Store the GameObject hosting the PlayMaker ArrayList Proxy component here")]
		public FsmGameObject store;

		
		public FsmEvent foundEvent;

		
		public FsmEvent notFoundEvent;
	}
}
