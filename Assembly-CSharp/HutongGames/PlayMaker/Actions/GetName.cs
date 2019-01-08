using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Gets the name of a Game Object and stores it in a String Variable.")]
	public class GetName : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = new FsmGameObject
			{
				UseVariable = true
			};
			this.storeName = null;
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
			this.storeName.Value = ((!(value != null)) ? string.Empty : value.name);
		}

		[RequiredField]
		public FsmGameObject gameObject;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmString storeName;

		public bool everyFrame;
	}
}
