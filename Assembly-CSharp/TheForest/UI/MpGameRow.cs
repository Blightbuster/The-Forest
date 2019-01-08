using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.UI
{
	public class MpGameRow : MonoBehaviour
	{
		public void RefreshName(List<Action<MpGameRow>> sortOptions)
		{
			if (this._previousPlayed)
			{
				this._continueButtonLabel.text = UiTranslationDatabase.TranslateKey("CONTINUE", "CONTINUE", false);
				if (!this._continueButtonLabel.transform.parent.gameObject.activeSelf)
				{
					this._continueButtonLabel.transform.parent.gameObject.SetActive(true);
				}
			}
			else if (this._continueButtonLabel.transform.parent.gameObject.activeSelf)
			{
				this._continueButtonLabel.transform.parent.gameObject.SetActive(false);
			}
			base.name = string.Empty;
			for (int i = 0; i < sortOptions.Count; i++)
			{
				sortOptions[i](this);
			}
			base.name += this._gameName.text;
		}

		public void SetLobby(CoopLobbyInfo lobby)
		{
			this._lobby = lobby;
		}

		public UILabel _gameName;

		public UILabel _ping;

		public UILabel _playerLimit;

		public UILabel _continueButtonLabel;

		public UILabel _newButtonLabel;

		public GameObject _prefabDbVersionMissmatch;

		public CoopLobbyInfo _lobby;

		public bool _previousPlayed;

		public bool _versionMismatch;
	}
}
