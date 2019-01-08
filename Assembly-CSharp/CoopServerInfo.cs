using System;
using Bolt;
using TheForest.Commons.Enums;
using TheForest.Interfaces;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;

public class CoopServerInfo : EntityBehaviour<ICoopServerInfo>
{
	public override void Attached()
	{
		CoopServerInfo.Instance = this;
		if (base.entity.isOwner)
		{
			this.RefreshServerInfo();
			EventRegistry.Game.Subscribe(TfEvent.DifficultySet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
			EventRegistry.Game.Subscribe(TfEvent.RegrowModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
			EventRegistry.Game.Subscribe(TfEvent.NoDestrutionModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
			EventRegistry.Game.Subscribe(TfEvent.CheatAllowedSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
		}
		else
		{
			base.state.AddCallback("AllowCheats", new PropertyCallbackSimple(this.UpdateGameModes));
			base.state.AddCallback("NoDestructionMode", new PropertyCallbackSimple(this.UpdateGameModes));
			base.state.AddCallback("RegrowMode", new PropertyCallbackSimple(this.UpdateGameModes));
			base.state.AddCallback("DifficultyMode", new PropertyCallbackSimple(this.UpdateDifficulty));
			this.UpdateGameModes();
			this.UpdateDifficulty();
		}
	}

	public override void Detached()
	{
		CoopServerInfo.Instance = null;
	}

	public void SetGameMode(IGameMode gm)
	{
		this._gameMode = gm;
	}

	private void OnGameSettingsChanged(object o)
	{
		this.RefreshServerInfo();
	}

	private void RefreshServerInfo()
	{
		if (base.entity.isAttached && base.entity.isOwner)
		{
			base.state.AllowCheats = PlayerPreferences.CheatsAllowed;
			base.state.NoDestructionMode = PlayerPreferences.NoDestruction;
			base.state.RegrowMode = PlayerPreferences.TreeRegrowth;
			base.state.DifficultyMode = (int)GameSetup.Difficulty;
			base.entity.Freeze(false);
		}
	}

	private void UpdateGameModes()
	{
		PlayerPreferences.NoDestructionRemote = base.state.NoDestructionMode;
		PlayerPreferences.TreeRegrowthRemote = base.state.RegrowMode;
		Cheats.SetAllowed(base.state.AllowCheats);
		if (!Cheats.Allowed)
		{
			if (this._gameMode != null)
			{
				this._gameMode.RestoreSettings();
			}
			else
			{
				Cheats.NoSurvival = false;
				Cheats.Creative = false;
				Cheats.GodMode = false;
				Cheats.InfiniteEnergy = false;
			}
		}
	}

	private void UpdateDifficulty()
	{
		Debug.Log("Setting game difficulty #" + base.state.DifficultyMode);
		GameSetup.SetDifficulty((DifficultyModes)base.state.DifficultyMode);
	}

	private void OnDestroy()
	{
		CoopServerInfo.Instance = null;
		EventRegistry.Game.Unsubscribe(TfEvent.DifficultySet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
		EventRegistry.Game.Unsubscribe(TfEvent.RegrowModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
		EventRegistry.Game.Unsubscribe(TfEvent.NoDestrutionModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
		EventRegistry.Game.Unsubscribe(TfEvent.CheatAllowedSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
	}

	public static CoopServerInfo Instance;

	private IGameMode _gameMode;
}
