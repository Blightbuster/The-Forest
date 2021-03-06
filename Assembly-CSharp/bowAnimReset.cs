﻿using System;
using UnityEngine;

public class bowAnimReset : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!animator.GetBool("drawBowBool"))
		{
			if (!this.playerEvents)
			{
				this.playerEvents = animator.transform.GetComponent<animEventsManager>();
			}
			if (this.playerEvents)
			{
				this.playerEvents.enableSpine();
			}
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.IsTag("held"))
		{
			if (!this.playerEvents)
			{
				this.playerEvents = animator.transform.GetComponent<animEventsManager>();
			}
			if (this.playerEvents)
			{
				this.playerEvents.enableSpine();
			}
		}
	}

	private animEventsManager playerEvents;
}
