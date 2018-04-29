using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class animClipMemoryManager : MonoBehaviour
{
	
	private void Start()
	{
		this.animationLoadManager = base.GetComponent<AnimationLoadManager>();
		if (!BoltNetwork.isRunning)
		{
			this.RemoveEndBossMecanimEvents();
			this.UnloadEndGameAnimation();
		}
	}

	
	private IEnumerator LoadEndGameAnimationDelayed()
	{
		yield return YieldPresets.WaitThreeSeconds;
		this.LoadEndGameAnimation();
		yield break;
	}

	
	private void LoadEndGameAnimation()
	{
		this.animationLoadManager.LoadAnimation("putTimmyOnMachine", null);
		this.animationLoadManager.LoadAnimation("operatePanel", null);
		this.animationLoadManager.LoadAnimation("girlTransformReaction", null);
	}

	
	public void loadCustomAnimation(string anim)
	{
		this.animationLoadManager.LoadAnimation(anim, null);
	}

	
	public void loadOperatePanelAlternate()
	{
	}

	
	public void unloadCustomAnimation(string anim)
	{
		this.animationLoadManager.StartCoroutine(this.animationLoadManager.UnloadAnimation(anim, true));
	}

	
	private void RemoveEndBossMecanimEvents()
	{
		this.mecanimEventsGo = Scene.mecanimEvents;
		if (this.mecanimEventsGo)
		{
			MecanimEventSetupHelper component = this.mecanimEventsGo.GetComponent<MecanimEventSetupHelper>();
			if (component)
			{
				component.dataSources[19] = null;
			}
		}
	}

	
	private void UnloadEndGameAnimation()
	{
		this.animationLoadManager.StartCoroutine(this.animationLoadManager.UnloadAnimation("putTimmyOnMachine", false));
		this.animationLoadManager.StartCoroutine(this.animationLoadManager.UnloadAnimation("operatePanel", false));
		this.animationLoadManager.StartCoroutine(this.animationLoadManager.UnloadAnimation("girlTransformReaction", false));
		Resources.UnloadUnusedAssets();
	}

	
	private AnimationLoadManager animationLoadManager;

	
	private GameObject mecanimEventsGo;
}
