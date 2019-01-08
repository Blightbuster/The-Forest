using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Destroys a PlayMakerHashTableProxy Component of a Game Object.")]
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

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker HashTable proxy component ( necessary if several component coexists on the same GameObject")]
		[UIHint(UIHint.FsmString)]
		public FsmString reference;

		[ActionSection("Result")]
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger if the HashTable proxy component is destroyed")]
		public FsmEvent successEvent;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger if the HashTable proxy component was not found")]
		public FsmEvent notFoundEvent;
	}
}
