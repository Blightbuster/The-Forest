using System;
using System.Collections;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Set a mesh vertex colors based on colors found in an arrayList")]
	public class ArrayListSetVertexColors : ArrayListActions
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
				this.SetVertexColors();
			}
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.SetVertexColors();
		}

		
		public void SetVertexColors()
		{
			if (!base.isProxyValid())
			{
				return;
			}
			this._colors = new Color[this.proxy.arrayList.Count];
			int num = 0;
			IEnumerator enumerator = this.proxy.arrayList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Color color = (Color)obj;
					this._colors[num] = color;
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
			this._mesh.colors = this._colors;
		}

		
		[ActionSection("Set up")]
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		
		[ActionSection("Target")]
		[Tooltip("The GameObject to set the mesh colors to")]
		[CheckForComponent(typeof(MeshFilter))]
		public FsmGameObject mesh;

		
		public bool everyFrame;

		
		private Mesh _mesh;

		
		private Color[] _colors;
	}
}
