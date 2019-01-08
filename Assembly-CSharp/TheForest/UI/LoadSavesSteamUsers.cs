using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	public class LoadSavesSteamUsers : MonoBehaviour
	{
		private void Awake()
		{
			LoadSavesSteamUsers.Current = -1;
			if (Directory.Exists(Application.persistentDataPath))
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath);
				directoryInfo.Refresh();
				IEnumerable<CSteamID> enumerable = from d in directoryInfo.GetDirectories()
				where d.Name.All((char c) => c >= '0' && c <= '9')
				select new CSteamID(Convert.ToUInt64(d.Name));
				if (SteamManager.Initialized)
				{
					CSteamID steamID = SteamUser.GetSteamID();
					if (!enumerable.Contains(steamID))
					{
						enumerable = enumerable.Concat(new CSteamID[]
						{
							steamID
						});
					}
					LoadSavesSteamUsers.Current = enumerable.IndexOf(steamID) - 1;
					LoadSavesSteamUsers.SteamUserNames = (from cs in enumerable
					select (cs.m_SteamID != 0UL) ? SteamFriends.GetFriendPersonaName(cs).ToUpperInvariant() : "ANONYMOUS").ToArray<string>();
				}
				else
				{
					LoadSavesSteamUsers.SteamUserNames = (from cs in enumerable
					select (cs.m_SteamID != 0UL) ? cs.ToString() : "ANONYMOUS").ToArray<string>();
				}
				LoadSavesSteamUsers.SteamUserIDs = enumerable.ToArray<CSteamID>();
			}
			else
			{
				try
				{
					LoadSavesSteamUsers.SteamUserIDs = new CSteamID[]
					{
						SteamUser.GetSteamID()
					};
					LoadSavesSteamUsers.SteamUserNames = new string[]
					{
						SteamFriends.GetPersonaName()
					};
				}
				catch (Exception ex)
				{
					LoadSavesSteamUsers.SteamUserIDs = new CSteamID[]
					{
						new CSteamID(0UL)
					};
					LoadSavesSteamUsers.SteamUserNames = new string[]
					{
						"ANONYMOUS"
					};
				}
			}
			if (LoadSavesSteamUsers.SteamUserIDs.Count<CSteamID>() <= 1)
			{
				this._nextUserButton.SetActive(false);
			}
			LoadSavesSteamUsers.Current++;
			SaveSlotUtils.SetUserId(LoadSavesSteamUsers.SteamUserIDs[LoadSavesSteamUsers.Current].ToString());
			this._label.text = LoadSavesSteamUsers.SteamUserNames[LoadSavesSteamUsers.Current];
			base.enabled = false;
		}

		private void Update()
		{
			if (this._nextRoutineStep < Time.realtimeSinceStartup)
			{
				this._routineStep++;
				switch (this._routineStep)
				{
				case 1:
					this._slotRoot.PlayForward();
					this._nextRoutineStep = Time.realtimeSinceStartup + 0.25f;
					break;
				case 2:
					this._slotRoot.gameObject.SetActive(false);
					this._nextRoutineStep = Time.realtimeSinceStartup + Time.deltaTime * 1.5f;
					break;
				case 3:
					this._slotRoot.gameObject.SetActive(true);
					this._nextRoutineStep = Time.realtimeSinceStartup + 0.2f;
					break;
				case 4:
					this._slotRoot.PlayReverse();
					this._nextRoutineStep = Time.realtimeSinceStartup + 0.25f;
					break;
				default:
					base.enabled = false;
					break;
				}
			}
		}

		public void ToggleNextSteamUser()
		{
			if (!base.enabled)
			{
				int num = (int)Mathf.Repeat((float)(LoadSavesSteamUsers.Current + 1), (float)LoadSavesSteamUsers.SteamUserIDs.Length);
				if (LoadSavesSteamUsers.Current != num)
				{
					LoadSavesSteamUsers.Current = num;
					SaveSlotUtils.SetUserId(LoadSavesSteamUsers.SteamUserIDs[LoadSavesSteamUsers.Current].ToString());
					this._label.text = LoadSavesSteamUsers.SteamUserNames[LoadSavesSteamUsers.Current];
					this._routineStep = 0;
					this._nextRoutineStep = 0f;
					base.enabled = true;
				}
			}
		}

		public TweenAlpha _slotRoot;

		public UILabel _label;

		public GameObject _nextUserButton;

		private static int Current = -1;

		private static CSteamID[] SteamUserIDs;

		private static string[] SteamUserNames;

		private Coroutine _refreshRoutine;

		private int _routineStep;

		private float _nextRoutineStep;
	}
}
