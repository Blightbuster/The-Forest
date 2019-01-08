using System;
using UnityEngine;

namespace Ceto
{
	[DisallowMultipleComponent]
	public abstract class ReflectionBase : OceanComponent
	{
		public abstract void RenderReflection(GameObject go);

		public Func<GameObject, RenderTexture> RenderReflectionCustom;
	}
}
