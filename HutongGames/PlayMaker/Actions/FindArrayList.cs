using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Finds an ArrayList by reference. Warning: this function can be very slow.")]
	[ActionCategory("ArrayMaker/ArrayList")]
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

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component")]
		[ActionSection("Set up")]
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		public FsmString ArrayListReference;

		
		[Tooltip("Store the GameObject hosting the PlayMaker ArrayList Proxy component here")]
		[ActionSection("Result")]
		[RequiredField]
		public FsmGameObject store;

		
		public FsmEvent foundEvent;

		
		public FsmEvent notFoundEvent;
	}
}
