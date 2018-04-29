﻿using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Save
{
	
	public class BrokenSaveChecker : MonoBehaviour
	{
		
		private IEnumerator Start()
		{
			base.enabled = false;
			while (LevelSerializer.IsDeserializing)
			{
				yield return null;
			}
			yield return null;
			if (this.CheckSceneComponents() || this.CheckSerializerId())
			{
				base.enabled = true;
				this._playAnywaysTimer = Time.time + this._playAnywaysDisplayDelay;
				UnityEngine.Object.FindObjectOfType<Clock>().enabled = false;
				UnityEngine.Object.FindObjectOfType<PlayerInventory>().TogglePauseMenu();
			}
			else
			{
				UnityEngine.Object.Destroy(this);
			}
			yield break;
		}

		
		private void OnGUI()
		{
			if (this._backgroundStyle == null)
			{
				this._backgroundStyle = GUI.skin.box;
			}
			GUI.Box(new Rect(-5f, -5f, (float)(Screen.width + 10), (float)(Screen.height + 10)), string.Empty, this._backgroundStyle);
			GUI.Label(new Rect((float)(Screen.width / 2 - 125), (float)(Screen.height / 2 + 40), 250f, 40f), this._brokenSaveText, GUI.skin.box);
			if (GUI.Button(new Rect((float)(Screen.width / 2 - 180), (float)(Screen.height / 2 + 100), 360f, 60f), this._backToTitleSceneButtonText))
			{
				Application.LoadLevel(0);
			}
			if (this._playAnywaysTimer < Time.time && GUI.Button(new Rect((float)(Screen.width / 2 - 180), (float)(Screen.height / 2 + 200), 360f, 60f), this._playAnywaysButtonText))
			{
				UnityEngine.Object.FindObjectOfType<Clock>().enabled = true;
				TheForest.Utils.Input.LockMouse();
				UnityEngine.Object.Destroy(this);
			}
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				LocalPlayer.FpCharacter.LockView(true);
			}
		}

		
		private bool CheckSceneComponents()
		{
			if (!Scene.Atmosphere || !Scene.Yacht || !Scene.Clock || Scene.Clock.Atmos == null || Scene.Atmosphere.Sun == null)
			{
				Debug.LogError("*****************************************");
				Debug.LogError("*****************************************");
				Debug.LogError("******** Corrupted Game Detected ********");
				Debug.LogError("** Check Serializer IDs & Scene object **");
				Debug.LogError("*****************************************");
				Debug.LogError("*****************************************");
				return true;
			}
			return false;
		}

		
		private bool CheckSerializerId()
		{
			string id = UnityEngine.Object.FindObjectOfType<TheForestAtmosphere>().GetComponent<StoreInformation>()._id;
			string id2 = UnityEngine.Object.FindObjectOfType<Clock>().GetComponent<StoreInformation>()._id;
			string id3 = UnityEngine.Object.FindObjectOfType<MassDestructionSaveManager>().GetComponent<StoreInformation>()._id;
			if (!id.Equals(this._atmos) || !id2.Equals(this._clock) || !id3.Equals(this._massDestruction))
			{
				Debug.LogError("************************************");
				Debug.LogError("************************************");
				Debug.LogError("****** Invalid Serializer IDs ******");
				Debug.LogError("************************************");
				Debug.LogError("************************************");
				return true;
			}
			return false;
		}

		
		public string _atmos = "d572d41e-2e1a-496d-8393-1aec35fb9b55";

		
		public string _clock = "ad06b9ad-c5e6-4721-8820-f2271fc43a8f";

		
		public string _massDestruction = "f98c510f-d3a5-4c26-8411-1c81c16cb586";

		
		public string _brokenSaveText = "This save file is corrupted,\nyou must start a fresh game.";

		
		public string _backToTitleSceneButtonText = "Back to main menu";

		
		public string _playAnywaysButtonText = "Understood,\nbut play anyway at the risk of game breaking issues\nand having an invalid save if I choose to save it";

		
		public float _playAnywaysDisplayDelay = 10f;

		
		public GUIStyle _backgroundStyle;

		
		private float _playAnywaysTimer;
	}
}
