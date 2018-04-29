using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Material)]
	[Tooltip("Sets a named float in a game object's material.")]
	public class SetMaterialFloat : ComponentAction<Renderer>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.materialIndex = 0;
			this.material = null;
			this.namedFloat = string.Empty;
			this.floatValue = 0f;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetMaterialFloat();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetMaterialFloat();
		}

		
		private void DoSetMaterialFloat()
		{
			if (this.material.Value != null)
			{
				this.material.Value.SetFloat(this.namedFloat.Value, this.floatValue.Value);
				return;
			}
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (!base.UpdateCache(ownerDefaultTarget))
			{
				return;
			}
			if (base.renderer.material == null)
			{
				base.LogError("Missing Material!");
				return;
			}
			if (this.materialIndex.Value == 0)
			{
				base.renderer.material.SetFloat(this.namedFloat.Value, this.floatValue.Value);
			}
			else if (base.renderer.materials.Length > this.materialIndex.Value)
			{
				Material[] materials = base.renderer.materials;
				materials[this.materialIndex.Value].SetFloat(this.namedFloat.Value, this.floatValue.Value);
				base.renderer.materials = materials;
			}
		}

		
		[Tooltip("The GameObject that the material is applied to.")]
		[CheckForComponent(typeof(Renderer))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("GameObjects can have multiple materials. Specify an index to target a specific material.")]
		public FsmInt materialIndex;

		
		[Tooltip("Alternatively specify a Material instead of a GameObject and Index.")]
		public FsmMaterial material;

		
		[RequiredField]
		[Tooltip("A named float parameter in the shader.")]
		public FsmString namedFloat;

		
		[RequiredField]
		[Tooltip("Set the parameter value.")]
		public FsmFloat floatValue;

		
		[Tooltip("Repeat every frame. Useful if the value is animated.")]
		public bool everyFrame;
	}
}
