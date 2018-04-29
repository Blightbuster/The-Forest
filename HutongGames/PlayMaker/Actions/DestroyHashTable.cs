using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Destroys a PlayMakerHashTableProxy Component of a Game Object.")]
	[ActionCategory("ArrayMaker/HashTable")]
	public class DestroyHashTable : HashTableActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.successEvent = null;
			this.notFoundEvent = null;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.SetUpHashTableProxyPointer(ownerDefaultTarget, this.reference.Value))
			{
				this.DoDestroyHashTable(ownerDefaultTarget);
			}
			else
			{
				base.Fsm.Event(this.notFoundEvent);
			}
			base.Finish();
		}

		
		private void DoDestroyHashTable(GameObject go)
		{
			PlayMakerHashTableProxy[] components = this.proxy.GetComponents<PlayMakerHashTableProxy>();
			foreach (PlayMakerHashTableProxy playMakerHashTableProxy in components)
			{
				if (playMakerHashTableProxy.referenceName == this.reference.Value)
				{
					UnityEngine.Object.Destroy(playMakerHashTableProxy);
					base.Fsm.Event(this.successEvent);
					return;
				}
			}
			base.Fsm.Event(this.notFoundEvent);
		}

		
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		[ActionSection("Set up")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker HashTable proxy component ( necessary if several component coexists on the same GameObject")]
		[UIHint(UIHint.FsmString)]
		public FsmString reference;

		
		[Tooltip("The event to trigger if the HashTable proxy component is destroyed")]
		[UIHint(UIHint.FsmEvent)]
		[ActionSection("Result")]
		public FsmEvent successEvent;

		
		[Tooltip("The event to trigger if the HashTable proxy component was not found")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent notFoundEvent;
	}
}
