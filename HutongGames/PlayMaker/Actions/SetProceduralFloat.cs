using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Set a named float property in a Substance material. NOTE: Use Rebuild Textures after setting Substance properties.")]
	[ActionCategory("Substance")]
	public class SetProceduralFloat : FsmStateAction
	{
		
		public override void Reset()
		{
			this.substanceMaterial = null;
			this.floatProperty = string.Empty;
			this.floatValue = 0f;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetProceduralFloat();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetProceduralFloat();
		}

		
		private void DoSetProceduralFloat()
		{
			ProceduralMaterial proceduralMaterial = this.substanceMaterial.Value as ProceduralMaterial;
			if (proceduralMaterial == null)
			{
				this.LogError("Not a substance material!");
				return;
			}
			proceduralMaterial.SetProceduralFloat(this.floatProperty.Value, this.floatValue.Value);
		}

		
		[RequiredField]
		public FsmMaterial substanceMaterial;

		
		[RequiredField]
		public FsmString floatProperty;

		
		[RequiredField]
		public FsmFloat floatValue;

		
		[Tooltip("NOTE: Updating procedural materials every frame can be very slow!")]
		public bool everyFrame;
	}
}
