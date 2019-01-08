using System;
using UnityEngine;

public static class AnimatorExtensions
{
	public static void CopyParamsFrom(this Animator self, Animator from)
	{
		AnimatorControllerData.ControllerData data = AnimatorControllerData.Instance.GetData(from.runtimeAnimatorController.name.GetHashCode());
		foreach (AnimatorControllerData.Parameter parameter in data.Parameters)
		{
			AnimatorControllerData.ParamType paramType = parameter.ParamType;
			if (paramType != AnimatorControllerData.ParamType.Bool)
			{
				if (paramType != AnimatorControllerData.ParamType.Int)
				{
					if (paramType == AnimatorControllerData.ParamType.Float)
					{
						self.SetFloatReflected(parameter.Name, from.GetFloat(parameter.Name));
					}
				}
				else
				{
					self.SetIntegerReflected(parameter.Name, from.GetInteger(parameter.Name));
				}
			}
			else
			{
				self.SetBoolReflected(parameter.Name, from.GetBool(parameter.Name));
			}
		}
	}
}
