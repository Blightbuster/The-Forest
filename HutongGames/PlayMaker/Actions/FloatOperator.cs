using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Performs math operations on 2 Floats: Add, Subtract, Multiply, Divide, Min, Max.")]
	[ActionCategory(ActionCategory.Math)]
	public class FloatOperator : FsmStateAction
	{
		
		public override void Reset()
		{
			this.float1 = null;
			this.float2 = null;
			this.operation = FloatOperator.Operation.Add;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoFloatOperator();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoFloatOperator();
		}

		
		private void DoFloatOperator()
		{
			float value = this.float1.Value;
			float value2 = this.float2.Value;
			switch (this.operation)
			{
			case FloatOperator.Operation.Add:
				this.storeResult.Value = value + value2;
				break;
			case FloatOperator.Operation.Subtract:
				this.storeResult.Value = value - value2;
				break;
			case FloatOperator.Operation.Multiply:
				this.storeResult.Value = value * value2;
				break;
			case FloatOperator.Operation.Divide:
				this.storeResult.Value = value / value2;
				break;
			case FloatOperator.Operation.Min:
				this.storeResult.Value = Mathf.Min(value, value2);
				break;
			case FloatOperator.Operation.Max:
				this.storeResult.Value = Mathf.Max(value, value2);
				break;
			}
		}

		
		[RequiredField]
		[Tooltip("The first float.")]
		public FsmFloat float1;

		
		[Tooltip("The second float.")]
		[RequiredField]
		public FsmFloat float2;

		
		[Tooltip("The math operation to perform on the floats.")]
		public FloatOperator.Operation operation;

		
		[Tooltip("Store the result of the operation in a float variable.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmFloat storeResult;

		
		[Tooltip("Repeat every frame. Useful if the variables are changing.")]
		public bool everyFrame;

		
		public enum Operation
		{
			
			Add,
			
			Subtract,
			
			Multiply,
			
			Divide,
			
			Min,
			
			Max
		}
	}
}
