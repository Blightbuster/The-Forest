using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Store all resolutions")]
	[ActionCategory("ArrayMaker/ArrayList")]
	public class ArrayListGetScreenResolutions : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.getResolutions();
			}
			base.Finish();
		}

		
		public void getResolutions()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.proxy.arrayList.Clear();
			Resolution[] resolutions = Screen.resolutions;
			foreach (Resolution resolution in resolutions)
			{
				this.proxy.arrayList.Add(new Vector3((float)resolution.width, (float)resolution.height, (float)resolution.refreshRate));
			}
		}

		
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[ActionSection("Set up")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;
	}
}
