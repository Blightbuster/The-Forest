using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Set a named color property in a Substance material. NOTE: Use Rebuild Textures after setting Substance properties.")]
	[ActionCategory("Substance")]
	public class SetProceduralColor : FsmStateAction
	{
		
		public override void Reset()
		{
			this.substanceMaterial = null;
			this.colorProperty = string.Empty;
			this.colorValue = Color.white;
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
			proceduralMaterial.SetProceduralColor(this.colorProperty.Value, this.colorValue.Value);
		}

		
		[RequiredField]
		public FsmMaterial substanceMaterial;

		
		[RequiredField]
		public FsmString colorProperty;

		
		[RequiredField]
		public FsmColor colorValue;

		
		[Tooltip("NOTE: Updating procedural materials every frame can be very slow!")]
		public bool everyFrame;
	}
}
