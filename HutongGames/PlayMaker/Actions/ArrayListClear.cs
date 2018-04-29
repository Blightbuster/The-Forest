using System;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Sets element to zero, false or null ( depending on the elements) of a PlayMaker ArrayList Proxy component")]
	public class ArrayListClear : ArrayListActions
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
				this.ClearArrayList();
			}
			base.Finish();
		}

		
		public void ClearArrayList()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.proxy.arrayList.Clear();
		}

		
		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;
	}
}
