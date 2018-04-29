using System;
using Bolt;
using TheForest.Items;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Networking
{
	
	public class RespawnDeadTrigger : EntityBehaviour<IPlayerState>
	{
		
		private void Awake()
		{
			base.enabled = false;
		}

		
		private void OnEnable()
		{
			this._sheenIcon.SetActive(true);
			this._pickupIcon.SetActive(false);
		}

		
		private void OnDisable()
		{
			if (LocalPlayer.Tuts)
			{
				LocalPlayer.Tuts.HideReviveMP();
			}
		}

		
		private void OnDestroy()
		{
			if (LocalPlayer.Tuts)
			{
				LocalPlayer.Inventory.Attacked.RemoveListener(new UnityAction(this.LocalPlayerAttacked));
				LocalPlayer.Tuts.HideReviveMP();
			}
		}

		
		private void Update()
		{
			if (base.state.CurrentView == 7)
			{
				if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 2.5f, false))
				{
					LocalPlayer.Tuts.HideReviveMP();
					LocalPlayer.Sfx.PlayTwinkle();
					PlayerHealed playerHealed = PlayerHealed.Create(GlobalTargets.Others);
					playerHealed.HealingItemId = this._healItemId;
					playerHealed.HealTarget = this.entity;
					playerHealed.Send();
					this.OnEnable();
					this.GrabExit();
					base.gameObject.SetActive(false);
					EventRegistry.Achievements.Publish(TfEvent.Achievements.RevivedPlayer, this.entity.source.RemoteEndPoint.SteamId.Id);
				}
				else if (!this._pickupIcon.activeSelf)
				{
					this._sheenIcon.SetActive(false);
					this._pickupIcon.SetActive(true);
				}
			}
			else if (this._sheenIcon.activeSelf || this._pickupIcon.activeSelf)
			{
				this._sheenIcon.SetActive(false);
				this._pickupIcon.SetActive(false);
				LocalPlayer.Tuts.HideReviveMP();
			}
		}

		
		private void GrabEnter()
		{
			LocalPlayer.Inventory.Attacked.AddListener(new UnityAction(this.LocalPlayerAttacked));
			if (!LocalPlayer.Inventory.Owns(this._healItemId, true))
			{
				LocalPlayer.Tuts.ShowReviveMP();
			}
			base.enabled = true;
		}

		
		private void GrabExit()
		{
			LocalPlayer.Inventory.Attacked.RemoveListener(new UnityAction(this.LocalPlayerAttacked));
			LocalPlayer.Tuts.HideReviveMP();
			base.enabled = false;
		}

		
		private void LocalPlayerAttacked()
		{
			if (!this || !base.transform || !LocalPlayer.Transform)
			{
				LocalPlayer.Tuts.HideReviveMP();
				LocalPlayer.Inventory.Attacked.RemoveListener(new UnityAction(this.LocalPlayerAttacked));
				return;
			}
			if (Vector3.Distance(base.transform.position, LocalPlayer.Transform.position) < this._maxKillDistance)
			{
				HitPlayer hitPlayer = HitPlayer.Create(GlobalTargets.Others);
				hitPlayer.Target = this.entity;
				hitPlayer.Send();
				LocalPlayer.Inventory.Attacked.RemoveListener(new UnityAction(this.LocalPlayerAttacked));
				this.OnEnable();
				this.GrabExit();
				base.gameObject.SetActive(false);
				LocalPlayer.Sfx.PlayKillRabbit();
				LocalPlayer.Tuts.HideReviveMP();
			}
		}

		
		[ItemIdPicker]
		public int _healItemId;

		
		public GameObject _sheenIcon;

		
		public GameObject _pickupIcon;

		
		public float _maxKillDistance;
	}
}
