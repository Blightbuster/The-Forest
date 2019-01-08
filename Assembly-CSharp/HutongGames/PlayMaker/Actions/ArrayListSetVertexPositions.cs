using System;
using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Set mesh vertex positions based on vector3 found in an arrayList")]
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
			IEnumerator enumerator = this.proxy.arrayList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Vector3 vector = (Vector3)obj;
					this._vertices[num] = vector;
					num++;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			this._mesh.vertices = this._vertices;
		}

		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[ActionSection("Target")]
		[Tooltip("The GameObject to set the mesh vertex positions to")]
		[CheckForComponent(typeof(MeshFilter))]
		public FsmGameObject mesh;

		public bool everyFrame;

		private Mesh _mesh;

		private Vector3[] _vertices;
	}
}
