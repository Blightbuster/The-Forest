using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Set mesh vertex positions based on vector3 found in an arrayList")]
	[ActionCategory("ArrayMaker/ArrayList")]
	public class ArrayListSetVertexPositions : ArrayListActions
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.reference = null;
			this.mesh = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			GameObject value = this.mesh.Value;
			if (value == null)
			{
				base.Finish();
				return;
			}
			MeshFilter component = value.GetComponent<MeshFilter>();
			if (component == null)
			{
				base.Finish();
				return;
			}
			this._mesh = component.mesh;
			if (base.SetUpArrayListProxyPointer(base.Fsm.GetOwnerDefaultTarget(this.gameObject), this.reference.Value))
			{
				this.SetVertexPositions();
			}
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.SetVertexPositions();
		}

		
		public void SetVertexPositions()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this._vertices = new Vector3[this.proxy.arrayList.Count];
			int num = 0;
			foreach (object obj in this.proxy.arrayList)
			{
				Vector3 vector = (Vector3)obj;
				this._vertices[num] = vector;
				num++;
			}
			this._mesh.vertices = this._vertices;
		}

		
		[RequiredField]
		[ActionSection("Set up")]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[Tooltip("The GameObject to set the mesh vertex positions to")]
		[CheckForComponent(typeof(MeshFilter))]
		[ActionSection("Target")]
		public FsmGameObject mesh;

		
		public bool everyFrame;

		
		private Mesh _mesh;

		
		private Vector3[] _vertices;
	}
}
