using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class AnimationLoadManager : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.GetComponent<Animator>();
		this.overrideController = (this.animator.runtimeAnimatorController as AnimatorOverrideController);
	}

	
	private IEnumerator LoadAnimClip(string clipName, string boolName)
	{
		LocalPlayer.AnimControl.loadingAnimation = true;
		ResourceRequest request = Resources.LoadAsync("CutScene/" + clipName);
		yield return request;
		AnimationClip animClip = request.asset as AnimationClip;
		this.overrideController[clipName + " Empty"] = animClip;
		this.animator.Update(0f);
		for (int i = 0; i < this.animator.layerCount; i++)
		{
			this.animator.CrossFade(this.layerInfo[i].nameHash, 0f, i, this.layerInfo[i].normalizedTime);
			this.animator.SetLayerWeight(i, this.layerWeights[i]);
		}
		if (this.flashLightHeld)
		{
			this.animator.SetBool("flashLightHeld", true);
		}
		if (this.lighterHeld)
		{
			this.animator.SetBool("lighterHeld", true);
		}
		if (this.pedHeld)
		{
			this.animator.SetBool("pedHeld", true);
		}
		this.animator.Update(0f);
		LocalPlayer.AnimControl.loadingAnimation = false;
		yield break;
	}

	
	public void LoadAnimation(string animationName, string animationBoolName)
	{
		this.animator = base.GetComponent<Animator>();
		this.layerInfo = new AnimatorStateInfo[this.animator.layerCount];
		this.layerWeights = new float[this.animator.layerCount];
		for (int i = 0; i < this.animator.layerCount; i++)
		{
			this.layerInfo[i] = this.animator.GetCurrentAnimatorStateInfo(i);
			this.layerWeights[i] = this.animator.GetLayerWeight(i);
		}
		this.flashLightHeld = this.animator.GetBool("flashLightHeld");
		this.lighterHeld = this.animator.GetBool("lighterHeld");
		this.pedHeld = this.animator.GetBool("pedHeld");
		base.StartCoroutine(this.LoadAnimClip(animationName, animationBoolName));
	}

	
	public IEnumerator UnloadAnimation(string clipName, bool refreshAssets)
	{
		this.animator = base.GetComponent<Animator>();
		LocalPlayer.AnimControl.loadingAnimation = true;
		this.layerInfo = new AnimatorStateInfo[this.animator.layerCount];
		this.layerWeights = new float[this.animator.layerCount];
		this.flashLightHeld = this.animator.GetBool("flashLightHeld");
		this.lighterHeld = this.animator.GetBool("lighterHeld");
		this.pedHeld = this.animator.GetBool("pedHeld");
		for (int i = 0; i < this.animator.layerCount; i++)
		{
			this.layerInfo[i] = this.animator.GetCurrentAnimatorStateInfo(i);
			this.layerWeights[i] = this.animator.GetLayerWeight(i);
		}
		ResourceRequest request = Resources.LoadAsync("CutScene/" + clipName + " Empty");
		yield return request;
		AnimationClip animClip = request.asset as AnimationClip;
		this.overrideController[clipName + " Empty"] = animClip;
		for (int j = 0; j < this.animator.layerCount; j++)
		{
			this.animator.CrossFade(this.layerInfo[j].nameHash, 0f, j, this.layerInfo[j].normalizedTime);
			this.animator.SetLayerWeight(j, this.layerWeights[j]);
		}
		if (refreshAssets)
		{
			Resources.UnloadUnusedAssets();
		}
		if (this.flashLightHeld)
		{
			this.animator.SetBool("flashLightHeld", true);
		}
		if (this.lighterHeld)
		{
			this.animator.SetBool("lighterHeld", true);
		}
		if (this.pedHeld)
		{
			this.animator.SetBool("pedHeld", true);
		}
		this.animator.Update(0f);
		LocalPlayer.AnimControl.loadingAnimation = false;
		yield break;
	}

	
	private AnimatorOverrideController overrideController;

	
	private Animator animator;

	
	private AnimatorStateInfo[] layerInfo;

	
	private float[] layerWeights;

	
	private bool flashLightHeld;

	
	private bool lighterHeld;

	
	private bool pedHeld;
}
