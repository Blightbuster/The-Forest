using System;
using UnityEngine;

namespace TheForest.World.Areas
{
	public class AreaMembers : MonoBehaviour
	{
		public void TurnOnMembers()
		{
			this.ToggleMembers(true);
		}

		public void TurnOffMembers()
		{
			this.ToggleMembers(false);
		}

		public void ToggleMembers(bool on)
		{
			if (this._renderers != null)
			{
				foreach (Renderer renderer in this._renderers)
				{
					if (renderer)
					{
						renderer.enabled = on;
					}
				}
			}
			if (this._lights != null)
			{
				foreach (Light light in this._lights)
				{
					if (light)
					{
						light.enabled = on;
					}
				}
			}
			if (this._reflectionProbes != null)
			{
				foreach (ReflectionProbe reflectionProbe in this._reflectionProbes)
				{
					if (reflectionProbe)
					{
						reflectionProbe.enabled = on;
					}
				}
			}
		}

		public Renderer[] _renderers;

		public Light[] _lights;

		public ReflectionProbe[] _reflectionProbes;
	}
}
