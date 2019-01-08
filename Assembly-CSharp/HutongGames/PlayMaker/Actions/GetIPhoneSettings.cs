using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Get various iPhone settings.")]
	public class GetIPhoneSettings : FsmStateAction
	{
		public override void Reset()
		{
			this.getScreenCanDarken = null;
			this.getUniqueIdentifier = null;
			this.getName = null;
			this.getModel = null;
			this.getSystemName = null;
			this.getGeneration = null;
		}

		public override void OnEnter()
		{
			base.Finish();
		}

		[UIHint(UIHint.Variable)]
		[Tooltip("Allows device to fall into 'sleep' state with screen being dim if no touches occurred. Default value is true.")]
		public FsmBool getScreenCanDarken;

		[UIHint(UIHint.Variable)]
		[Tooltip("A unique device identifier string. It is guaranteed to be unique for every device (Read Only).")]
		public FsmString getUniqueIdentifier;

		[UIHint(UIHint.Variable)]
		[Tooltip("The user defined name of the device (Read Only).")]
		public FsmString getName;

		[UIHint(UIHint.Variable)]
		[Tooltip("The model of the device (Read Only).")]
		public FsmString getModel;

		[UIHint(UIHint.Variable)]
		[Tooltip("The name of the operating system running on the device (Read Only).")]
		public FsmString getSystemName;

		[UIHint(UIHint.Variable)]
		[Tooltip("The generation of the device (Read Only).")]
		public FsmString getGeneration;
	}
}
