using System;
using HutongGames.PlayMaker.Actions;
using UnityEngine;


public class iTweenFSMEvents : MonoBehaviour
{
	
	private void iTweenOnStart(int aniTweenID)
	{
		if (this.itweenID == aniTweenID)
		{
			this.itweenFSMAction.Fsm.Event(this.itweenFSMAction.startEvent);
		}
	}

	
	private void iTweenOnComplete(int aniTweenID)
	{
		if (this.itweenID == aniTweenID)
		{
			if (this.islooping)
			{
				if (!this.donotfinish)
				{
					this.itweenFSMAction.Fsm.Event(this.itweenFSMAction.finishEvent);
					this.itweenFSMAction.Finish();
				}
			}
			else
			{
				this.itweenFSMAction.Fsm.Event(this.itweenFSMAction.finishEvent);
				this.itweenFSMAction.Finish();
			}
		}
	}

	
	public static int itweenIDCount;

	
	public int itweenID;

	
	public iTweenFsmAction itweenFSMAction;

	
	public bool donotfinish;

	
	public bool islooping;
}
