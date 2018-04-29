using System;
using Bolt;
using TheForest.Commons.Enums;
using TheForest.Tools;
using TheForest.Utils;


public class CoopServerInfo : EntityBehaviour<ICoopServerInfo>
{
	
	public override void Attached()
	{
		CoopServerInfo.Instance = this;
		if (base.entity.isOwner)
		{
			this.RefreshServerInfo();
			EventRegistry.Game.Subscribe(TfEvent.RegrowModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
			EventRegistry.Game.Subscribe(TfEvent.NoDestrutionModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
		}
		else
		{
			base.state.AddCallback("NoDestructionMode", new PropertyCallbackSimple(this.UpdateGameModes));
			base.state.AddCallback("RegrowMode", new PropertyCallbackSimple(this.UpdateGameModes));
			base.state.AddCallback("DifficultyMode", new PropertyCallbackSimple(this.UpdateDifficulty));
			this.UpdateGameModes();
			this.UpdateDifficulty();
		}
	}

	
	private void OnGameSettingsChanged(object o)
	{
		this.RefreshServerInfo();
	}

	
	private void RefreshServerInfo()
	{
		if (base.entity.isAttached && base.entity.isOwner)
		{
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
	}

	
	private void UpdateDifficulty()
	{
		GameSetup.SetDifficulty((DifficultyModes)base.state.DifficultyMode);
	}

	
	public override void Detached()
	{
		CoopServerInfo.Instance = null;
	}

	
	private void OnDestroy()
	{
		CoopServerInfo.Instance = null;
		EventRegistry.Game.Unsubscribe(TfEvent.RegrowModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
		EventRegistry.Game.Unsubscribe(TfEvent.NoDestrutionModeSet, new EventRegistry.SubscriberCallback(this.OnGameSettingsChanged));
	}

	
	public static CoopServerInfo Instance;
}
