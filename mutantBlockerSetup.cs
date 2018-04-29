using System;
using TheForest.Utils;
using UnityEngine;


public class mutantBlockerSetup : MonoBehaviour
{
	
	private void Start()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			this._col = base.GetComponent<Collider>();
			this.setBlockerCollision();
		}
		else
		{
			base.enabled = false;
		}
	}

	
	private void OnEnable()
	{
		if (!base.IsInvoking("setBlockerCollision"))
		{
			base.InvokeRepeating("setBlockerCollision", 1f, 10f);
		}
	}

	
	private void OnDisable()
	{
		base.CancelInvoke("setBlockerCollision");
	}

	
	private void setBlockerCollision()
	{
		if (!LocalPlayer.Transform)
		{
			return;
		}
		if (LocalPlayer.AnimControl.playerCollider && LocalPlayer.AnimControl.playerCollider.gameObject.activeSelf && LocalPlayer.AnimControl.playerCollider.enabled && this._col.enabled)
		{
			Physics.IgnoreCollision(this._col, LocalPlayer.AnimControl.playerCollider);
		}
		if (LocalPlayer.AnimControl.playerHeadCollider && LocalPlayer.AnimControl.playerHeadCollider.gameObject.activeSelf && LocalPlayer.AnimControl.playerHeadCollider.enabled && this._col.enabled)
		{
			Physics.IgnoreCollision(this._col, LocalPlayer.AnimControl.playerHeadCollider);
		}
	}

	
	private Collider _col;
}
