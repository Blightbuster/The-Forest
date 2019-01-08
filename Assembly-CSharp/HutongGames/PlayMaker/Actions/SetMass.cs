using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Physics)]
	[Tooltip("Sets the Mass of a Game Object's Rigid Body.")]
	public class SetMass : ComponentAction<Rigidbody>
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.mass = 1f;
		}

		public override void OnEnter()
		{
			this.DoSetMass();
			base.Finish();
		}

		private void DoSetMass()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.rigidbody.mass = this.mass.Value;
			}
		}

		[RequiredField]
		[CheckForComponent(typeof(Rigidbody))]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[HasFloatSlider(0.1f, 10f)]
		public FsmFloat mass;
	}
}
