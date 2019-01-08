using System;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Count items from a PlayMaker ArrayList Proxy component")]
	public class ArrayListCount : ArrayListActions
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.count = null;
		}

		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.getArrayListCount();
			}
			base.Finish();
		}

		public void getArrayListCount()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			int value = this.proxy.arrayList.Count;
			this.count.Value = value;
		}

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[ActionSection("Result")]
		[UIHint(UIHint.FsmInt)]
		[Tooltip("Store the count value")]
		public FsmInt count;
	}
}
