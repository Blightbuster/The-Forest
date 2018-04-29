using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Destroys a PlayMakerArrayListProxy Component of a Game Object.")]
	public class DestroyArrayList : ArrayListActions
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
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.DoDestroyArrayList();
			}
			else
			{
				base.Fsm.Event(this.notFoundEvent);
			}
			base.Finish();
		}

		
		private void DoDestroyArrayList()
		{
			PlayMakerArrayListProxy[] components = this.proxy.GetComponents<PlayMakerArrayListProxy>();
			foreach (PlayMakerArrayListProxy playMakerArrayListProxy in components)
			{
				if (playMakerArrayListProxy.referenceName == this.reference.Value)
				{
					UnityEngine.Object.Destroy(playMakerArrayListProxy);
					base.Fsm.Event(this.successEvent);
					return;
				}
			}
			base.Fsm.Event(this.notFoundEvent);
		}

		
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[RequiredField]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList proxy component ( necessary if several component coexists on the same GameObject")]
		[UIHint(UIHint.FsmString)]
		public FsmString reference;

		
		[UIHint(UIHint.FsmEvent)]
		[ActionSection("Result")]
		[Tooltip("The event to trigger if the ArrayList proxy component is destroyed")]
		public FsmEvent successEvent;

		
		[Tooltip("The event to trigger if the ArrayList proxy component was not found")]
		[UIHint(UIHint.FsmEvent)]
		public FsmEvent notFoundEvent;
	}
}
