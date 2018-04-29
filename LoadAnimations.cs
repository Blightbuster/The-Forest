using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;


[AddComponentMenu("System/Load Animations")]
public class LoadAnimations : MonoBehaviour
{
	
	private void Awake()
	{
		IEnumerable<AnimationClip> enumerable = Resources.LoadAll("Animations/" + this.name, typeof(AnimationClip)).Cast<AnimationClip>();
		foreach (AnimationClip animationClip in enumerable)
		{
			base.GetComponent<Animation>().AddClip(animationClip, (!animationClip.name.Contains("@")) ? animationClip.name : animationClip.name.Substring(animationClip.name.LastIndexOf("@") + 1));
		}
		foreach (AnimationState animationState in base.GetComponent<Animation>().Cast<AnimationState>())
		{
			animationState.enabled = true;
		}
	}

	
	public new string name;
}
