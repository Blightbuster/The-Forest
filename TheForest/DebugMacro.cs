using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheForest
{
	
	public class DebugMacro : MonoBehaviour
	{
		
		private static bool IsSceneLoaded(string sceneName)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				if (SceneManager.GetSceneAt(i).name == sceneName)
				{
					return true;
				}
			}
			return false;
		}

		
		private void Update()
		{
			if (this._forceRun)
			{
				this._forceRun = false;
				base.StartCoroutine(this.Worker());
			}
			foreach (DebugMacro.MacroItem macroItem in this._macroItems)
			{
				if (macroItem._forceRun)
				{
					base.StartCoroutine(macroItem.Execute());
				}
			}
		}

		
		private IEnumerator Worker()
		{
			if (this._macroItems.SafeCount<DebugMacro.MacroItem>() == 0)
			{
				yield break;
			}
			foreach (DebugMacro.MacroItem eachItem in this._macroItems)
			{
				if (eachItem != null && !eachItem._skip)
				{
					if (eachItem._delay > 0f)
					{
						yield return new WaitForSeconds(eachItem._delay);
					}
					yield return base.StartCoroutine(eachItem.Execute());
				}
			}
			yield break;
		}

		
		public void Trigger()
		{
			base.StartCoroutine(this.Worker());
		}

		
		public string _macroName;

		
		public bool _forceRun;

		
		public DebugMacro.MacroItem[] _macroItems;

		
		[Serializable]
		public enum DebugGotoMacroType
		{
			
			Command,
			
			LoadSceneAddtive,
			
			PlayerMessage,
			
			SpecialActionMessage
		}

		
		[Serializable]
		public class MacroItem
		{
			
			public IEnumerator Execute()
			{
				DebugConsole debugConsole = DebugConsole.GetInstance();
				switch (this._type)
				{
				case DebugMacro.DebugGotoMacroType.Command:
					debugConsole.HandleConsoleInput(this._command + " " + this._args);
					break;
				case DebugMacro.DebugGotoMacroType.LoadSceneAddtive:
					if (!DebugMacro.IsSceneLoaded(this._args) || this._allowDuplicatedScenes)
					{
						yield return SceneManager.LoadSceneAsync(this._args, LoadSceneMode.Additive);
					}
					break;
				case DebugMacro.DebugGotoMacroType.PlayerMessage:
					LocalPlayer.GameObject.SendMessage(this._command, this._args);
					break;
				case DebugMacro.DebugGotoMacroType.SpecialActionMessage:
					LocalPlayer.SpecialActions.SendMessage(this._command, this._args);
					break;
				}
				yield break;
			}

			
			public DebugMacro.DebugGotoMacroType _type;

			
			public string _command;

			
			public string _args;

			
			public float _delay;

			
			public bool _skip;

			
			public bool _forceRun;

			
			public bool _allowDuplicatedScenes;
		}
	}
}
