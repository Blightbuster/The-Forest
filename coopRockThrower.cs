using System;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Utils;
using UnityEngine;


public class coopRockThrower : EntityEventListener<IRockThrowerState>
{
	
	public override void Attached()
	{
		if (Scene.SceneTracker && !Scene.SceneTracker.builtRockThrowers.Contains(base.transform))
		{
			Scene.SceneTracker.builtRockThrowers.Add(base.transform);
		}
	}

	
	private void OnDestroy()
	{
		if (Scene.SceneTracker && Scene.SceneTracker.builtRockThrowers.Contains(base.transform))
		{
			Scene.SceneTracker.builtRockThrowers.Remove(base.transform);
		}
	}

	
	private void disableTrigger()
	{
		this.triggerGo.SetActive(false);
	}

	
	private void enableTrigger()
	{
		this.triggerGo.SetActive(true);
	}

	
	public void setAnimator(int var, bool onoff)
	{
		if (var == 0)
		{
			this.TargetAnimator.SetBoolReflected("load", onoff);
		}
		else if (var == 1)
		{
			this.TargetAnimator.SetBoolReflected("release", onoff);
		}
	}

	
	public MultiThrowerItemHolder Holder;

	
	public rockThrowerAnimEvents Anim;

	
	public Animator TargetAnimator;

	
	public GameObject triggerGo;

	
	public Transform leverRotateTr;

	
	public float leverRotateValue;
}
