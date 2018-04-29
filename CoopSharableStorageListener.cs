using System;
using Bolt;
using TheForest.Items.Core;


public class CoopSharableStorageListener : EntityBehaviour<IPlayerState>
{
	
	private void Awake()
	{
		base.enabled = BoltNetwork.isRunning;
	}

	
	private void Update()
	{
		if (this.entity && this.entity.isAttached)
		{
			this.CheckContentVersion();
		}
	}

	
	public void CheckContentVersion()
	{
		if (this._contentVersion != this._storage.ContentVersion)
		{
			this.RefreshState();
			this._contentVersion = this._storage.ContentVersion;
		}
	}

	
	private void RefreshState()
	{
		int i;
		for (i = 0; i < this._storage.UsedSlots.Count; i++)
		{
			base.state.SharableStorage[i].Id = this._storage.UsedSlots[i]._itemId;
			this._storage.UsedSlots[i]._properties.Fill(base.state.SharableStorage[i]);
		}
		while (i < base.state.SharableStorage.Length)
		{
			base.state.SharableStorage[i].Id = -1;
			i++;
		}
	}

	
	public ItemStorage _storage;

	
	private int _contentVersion = -1;
}
