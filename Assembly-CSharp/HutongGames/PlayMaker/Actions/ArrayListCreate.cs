using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Adds a PlayMakerArrayList Component to a Game Object. Use this to create arrayList on the fly instead of during authoring.\n Optionally remove the ArrayList component on exiting the state.\n Simply point to existing if the reference exists already.")]
	public class ArrayListCreate : ArrayListActions
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.alreadyExistsEvent = null;
		}

		public override void OnEnter()
		{
			this.DoAddPlayMakerArrayList();
			base.Finish();
		}

		public override void OnExit()
		{
			if (this.removeOnExit.Value && this.addedComponent != null)
			{
				UnityEngine.Object.Destroy(this.addedComponent);
			}
		}

		private void DoAddPlayMakerArrayList()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			PlayMakerArrayListProxy arrayListProxyPointer = base.GetArrayListProxyPointer(ownerDefaultTarget, this.reference.Value, true);
			if (arrayListProxyPointer != null)
			{
				base.Fsm.Event(this.alreadyExistsEvent);
			}
			else
			{
				this.addedComponent = ownerDefaultTarget.AddComponent<PlayMakerArrayListProxy>();
				if (this.addedComponent == null)
				{
					base.LogError("Can't add PlayMakerArrayListProxy");
				}
				else
				{
					this.addedComponent.referenceName = this.reference.Value;
				}
			}
		}

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject to add the PlayMaker ArrayList Proxy component to")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker arrayList proxy component ( necessary if several component coexists on the same GameObject")]
		[UIHint(UIHint.FsmString)]
		public FsmString reference;

		[Tooltip("Remove the Component when this State is exited.")]
		public FsmBool removeOnExit;

		[ActionSection("Result")]
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger if the arrayList exists already")]
		public FsmEvent alreadyExistsEvent;

		private PlayMakerArrayListProxy addedComponent;
	}
}
