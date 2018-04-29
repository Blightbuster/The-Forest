using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Application)]
	[Tooltip("Get the current resolution")]
	public class GetCurrentResolution : FsmStateAction
	{
		
		public override void Reset()
		{
			this.width = null;
			this.height = null;
			this.refreshRate = null;
			this.currentResolution = null;
		}

		
		public override void OnEnter()
		{
			this.width.Value = (float)Screen.currentResolution.width;
			this.height.Value = (float)Screen.currentResolution.height;
			this.refreshRate.Value = (float)Screen.currentResolution.refreshRate;
			this.currentResolution.Value = new Vector3((float)Screen.currentResolution.width, (float)Screen.currentResolution.height, (float)Screen.currentResolution.refreshRate);
			base.Finish();
		}

		
		[Tooltip("The current resolution width")]
		[UIHint(UIHint.Variable)]
		public FsmFloat width;

		
		[Tooltip("The current resolution height")]
		[UIHint(UIHint.Variable)]
		public FsmFloat height;

		
		[Tooltip("The current resolution refrehs rate")]
		[UIHint(UIHint.Variable)]
		public FsmFloat refreshRate;

		
		[Tooltip("The current resolution ( width, height, refreshRate )")]
		[UIHint(UIHint.Variable)]
		public FsmVector3 currentResolution;
	}
}
