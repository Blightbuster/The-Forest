using System;
using System.Collections.Generic;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class CoopWeaponUpgrades : MonoBehaviour
{
	private void Awake()
	{
		if (BoltNetwork.isRunning && !this._mirror)
		{
			this._receiverVersions = new int[this._receivers.Length];
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (BoltNetwork.isRunning && !this._proxy)
		{
			BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
			if (componentInParent && componentInParent.isAttached && componentInParent.isOwner)
			{
				this._proxy = UnityEngine.Object.Instantiate<CoopWeaponUpgradesProxy>(Prefabs.Instance.WeaponUpgradesProxy);
				BoltNetwork.Attach(this._proxy.gameObject);
				this._proxy.state.ItemId = base.GetComponent<HeldItemIdentifier>()._itemId;
				this._proxy.state.TargetPlayer = componentInParent;
				this.RefreshToken();
			}
		}
	}

	private void Update()
	{
		if (this._proxy)
		{
			for (int i = 0; i < this._receivers.Length; i++)
			{
				if (this._receivers[i].Version != this._receiverVersions[i])
				{
					this.RefreshToken();
					break;
				}
			}
			if (this._cloth && this._proxy.entity.isAttached && this._proxy.state.Cloth != (this._cloth.enabled && this._cloth.gameObject.activeSelf))
			{
				this._proxy.state.Cloth = this._cloth.enabled;
				this._proxy.entity.Freeze(false);
			}
			if (this._painting && this._proxy.entity.isAttached && this._proxy.state.Color != (int)this._painting.Color)
			{
				this._proxy.state.Color = (int)this._painting.Color;
				this._proxy.entity.Freeze(false);
			}
			if (this._altRenderers != null && this._altRenderers.Length > 1)
			{
				for (int j = 0; j < this._altRenderers.Length; j++)
				{
					if (this._altRenderers[j].enabled)
					{
						this._proxy.state.AltRenderer = j;
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (this._proxy)
		{
			if (this._proxy.entity && this._proxy.entity.isAttached)
			{
				this._proxy.state.Token = null;
				BoltNetwork.Destroy(this._proxy.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(this._proxy.gameObject);
			}
			this._proxy = null;
		}
		this._token = null;
	}

	private void SpawnProxy()
	{
	}

	private void RefreshToken()
	{
		this._token = new CoopWeaponUpgradesToken();
		if (this._token.Views == null)
		{
			this._token.Views = new List<UpgradeViewReceiver.UpgradeViewData>();
		}
		else
		{
			this._token.Views.Clear();
		}
		for (int i = 0; i < this._receivers.Length; i++)
		{
			if (this._receivers[i].CurrentUpgrades != null)
			{
				this._token.Views.AddRange(this._receivers[i].CurrentUpgrades);
			}
			this._receiverVersions[i] = this._receivers[i].Version;
		}
		this._proxy.state.Token = this._token;
		this._proxy.entity.Freeze(false);
	}

	public UpgradeViewReceiver[] _receivers;

	public Transform _mirror;

	public Renderer _cloth;

	public EquipmentPainting _painting;

	public Renderer[] _altRenderers;

	private CoopWeaponUpgradesToken _token;

	private CoopWeaponUpgradesProxy _proxy;

	private int[] _receiverVersions;
}
