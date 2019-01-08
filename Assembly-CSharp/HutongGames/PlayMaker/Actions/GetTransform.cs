using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Gets a Game Object's Transform and stores it in an Object Variable.")]
	public class GetTransform : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = new FsmGameObject
			{
				UseVariable = true
			};
			this.storeTransform = null;
			this.everyFrame = false;
		}

		public override void OnEnter()
		{
			this.DoGetGameObjectName();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoGetGameObjectName();
		}

		private void DoGetGameObjectName()
		{
			GameObject value = this.gameObject.Value;
			this.storeTransform.Value = ((!(value != null)) ? null : value.transform);
		}

		[RequiredField]
		public FsmGameObject gameObject;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[ObjectType(typeof(Transform))]
		public FsmObject storeTransform;

		public bool everyFrame;
	}
}
