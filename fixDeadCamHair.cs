using System;
using UnityEngine;
using UnityEngine.Rendering;


public class fixDeadCamHair : MonoBehaviour
{
	
	private void OnEnable()
	{
		for (int i = 0; i < this.hair.Length; i++)
		{
			if (this.hair[i] != null)
			{
				this.hair[i].shadowCastingMode = ShadowCastingMode.On;
			}
		}
	}

	
	private void OnDisable()
	{
		for (int i = 0; i < this.hair.Length; i++)
		{
			if (this.hair[i] != null)
			{
				this.hair[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
			}
		}
	}

	
	public Renderer[] hair;
}
