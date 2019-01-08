using System;
using System.Collections.Generic;
using UnityEngine;

public class BlendWeightConnector : MonoBehaviour
{
	private void Start()
	{
	}

	private const string BLEND_SHAPE_ANIM_PREFIX = "BlendWeight_";

	public float scaleFactor;

	private List<BlendWeightConnector.BlendWeightLink> blendWeightLinks;

	private class BlendWeightLink
	{
	}
}
