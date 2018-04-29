using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Makes the Game Object not be destroyed automatically when loading a new scene.")]
	[ActionCategory(ActionCategory.Level)]
	public class DontDestroyOnLoad : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
		}

		
		public override void OnEnter()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.Owner.transform.root.gameObject);
			base.Finish();
		}

		
		[Tooltip("GameObject to mark as DontDestroyOnLoad.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;
	}
}
