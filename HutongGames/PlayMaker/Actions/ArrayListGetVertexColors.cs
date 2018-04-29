using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Store a mesh vertex colors into an arrayList")]
	public class ArrayListGetVertexColors : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.mesh = null;
		}

		
		public override void OnEnter()
		{
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.getVertexColors();
			}
			base.Finish();
		}

		
		public void getVertexColors()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this.proxy.arrayList.Clear();
			GameObject value = this.mesh.Value;
			if (value == null)
			{
				return;
			}
			MeshFilter component = value.GetComponent<MeshFilter>();
			if (component == null)
			{
				return;
			}
			this.proxy.arrayList.InsertRange(0, component.mesh.colors);
		}

		
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[RequiredField]
		[ActionSection("Set up")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[CheckForComponent(typeof(MeshFilter))]
		[ActionSection("Source")]
		[Tooltip("the GameObject to get the mesh from")]
		public FsmGameObject mesh;
	}
}
