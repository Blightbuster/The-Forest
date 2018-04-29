using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Count items from a PlayMaker ArrayList Proxy component")]
	[ActionCategory("ArrayMaker/ArrayList")]
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

		
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[ActionSection("Set up")]
		[RequiredField]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[Tooltip("Store the count value")]
		[ActionSection("Result")]
		[UIHint(UIHint.FsmInt)]
		public FsmInt count;
	}
}
