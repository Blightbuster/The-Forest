using System;
using UnityEngine;


public static class AnimatorExtensions
{
	
	public static void CopyParamsFrom(this Animator self, Animator from)
	{
		AnimatorControllerData.ControllerData data = AnimatorControllerData.Instance.GetData(from.runtimeAnimatorController.name.GetHashCode());
		foreach (AnimatorControllerData.Parameter parameter in data.Parameters)
		{
			switch (parameter.ParamType)
			{
			case AnimatorControllerData.ParamType.Bool:
				self.SetBoolReflected(parameter.Name, from.GetBool(parameter.Name));
				break;
			case AnimatorControllerData.ParamType.Int:
				self.SetIntegerReflected(parameter.Name, from.GetInteger(parameter.Name));
				break;
			case AnimatorControllerData.ParamType.Float:
				self.SetFloatReflected(parameter.Name, from.GetFloat(parameter.Name));
				break;
			}
		}
	}
}
