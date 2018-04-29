using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Level)]
	[Tooltip("Loads a Level by Index number. Before you can load a level, you have to add it to the list of levels defined in File->Build Settings...")]
	public class LoadLevelNum : FsmStateAction
	{
		
		public override void Reset()
		{
			this.levelIndex = null;
			this.additive = false;
			this.loadedEvent = null;
			this.dontDestroyOnLoad = false;
		}

		
		public override void OnEnter()
		{
			if (this.dontDestroyOnLoad.Value)
			{
				Transform root = base.Owner.transform.root;
				UnityEngine.Object.DontDestroyOnLoad(root.gameObject);
			}
			if (this.additive)
			{
				Application.LoadLevelAdditive(this.levelIndex.Value);
			}
			else
			{
				Application.LoadLevel(this.levelIndex.Value);
			}
			base.Fsm.Event(this.loadedEvent);
			base.Finish();
		}

		
		[RequiredField]
		[Tooltip("The level index in File->Build Settings")]
		public FsmInt levelIndex;

		
		[Tooltip("Load the level additively, keeping the current scene.")]
		public bool additive;

		
		[Tooltip("Event to send after the level is loaded.")]
		public FsmEvent loadedEvent;

		
		[Tooltip("Keep this GameObject in the new level. NOTE: The GameObject and components is disabled then enabled on load; uncheck Reset On Disable to keep the active state.")]
		public FsmBool dontDestroyOnLoad;
	}
}
