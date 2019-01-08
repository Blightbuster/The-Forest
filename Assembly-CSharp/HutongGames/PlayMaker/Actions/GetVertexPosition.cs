using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Mesh")]
	[Tooltip("Gets the position of a vertex in a GameObject's mesh. Hint: Use GetVertexCount to get the number of vertices in a mesh.")]
	public class GetVertexPosition : FsmStateAction
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.space = Space.World;
			this.storePosition = null;
			this.everyFrame = false;
		}

		public override void OnEnter()
		{
			this.DoGetVertexPosition();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoGetVertexPosition();
		}

		private void DoGetVertexPosition()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget != null)
			{
				MeshFilter component = ownerDefaultTarget.GetComponent<MeshFilter>();
				if (component == null)
				{
					base.LogError("Missing MeshFilter!");
					return;
				}
				Space space = this.space;
				if (space != Space.World)
				{
					if (space == Space.Self)
					{
						this.storePosition.Value = component.mesh.vertices[this.vertexIndex.Value];
					}
				}
				else
				{
					Vector3 position = component.mesh.vertices[this.vertexIndex.Value];
					this.storePosition.Value = ownerDefaultTarget.transform.TransformPoint(position);
				}
			}
		}

		[RequiredField]
		[CheckForComponent(typeof(MeshFilter))]
		[Tooltip("The GameObject to check.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The index of the vertex.")]
		public FsmInt vertexIndex;

		[Tooltip("Coordinate system to use.")]
		public Space space;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the vertex position in a variable.")]
		public FsmVector3 storePosition;

		[Tooltip("Repeat every frame. Useful if the mesh is animated.")]
		public bool everyFrame;
	}
}
