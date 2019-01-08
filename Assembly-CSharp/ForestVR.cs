using System;
using UnityEngine;

public class ForestVR : MonoBehaviour
{
	public static bool Enabled
	{
		get
		{
			return ForestVR._enabled;
		}
		set
		{
			if (ForestVR._enabled == value)
			{
				return;
			}
			if (value)
			{
				Shader.EnableKeyword("VR_SHADER_ENABLED");
			}
			else
			{
				Shader.DisableKeyword("VR_SHADER_ENABLED");
			}
			ForestVR._enabled = value;
		}
	}

	private void Awake()
	{
		ForestVR.Prototype = this.MakeThisScenePrototype;
	}

	public static bool Prototype;

	private static bool _enabled;

	[Tooltip("Is VR prototype behaviour enabled on this scene (stubs out game code so can run in testbed)")]
	public bool MakeThisScenePrototype = true;
}
