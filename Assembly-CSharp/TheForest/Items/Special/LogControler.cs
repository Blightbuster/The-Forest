using System;
using System.Collections;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Special
{
	[DoNotSerializePublic]
	public class LogControler : EntityBehaviour<IPlayerState>
	{
		private void Start()
		{
			this._player.Logs = this;
			this._itemCache = ItemDatabase.ItemById(this._logItemId);
			base.enabled = false;
		}

		private void Update()
		{
			if (SteamDSConfig.isDedicatedServer)
			{
				return;
			}
			if ((FirstPersonCharacter.GetDropInput() || (ForestVR.Enabled && TheForest.Utils.Input.GetButtonDown("Fire1") && !LocalPlayer.Inventory.DontShowDrop)) && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
			{
				this.PutDown(false, true, true, null);
			}
		}

		private IEnumerator OnDeserialized()
		{
			if (BoltNetwork.isRunning)
			{
				while (!LocalPlayer.Entity)
				{
					yield return null;
				}
			}
			if (this._logs > 0)
			{
				int logs = this._logs;
				this._logs = 0;
				for (int i = 0; i < logs; i++)
				{
					this.Lift();
				}
				this.UpdateLogCount();
			}
			yield break;
		}

		public bool Lift()
		{
			if (this._logs < 2 && !LocalPlayer.AnimControl.swimming && !LocalPlayer.FpCharacter.PushingSled && !LocalPlayer.FpCharacter.SailingRaft && !LocalPlayer.AnimControl.carry && !LocalPlayer.AnimControl.useRootMotion)
			{
				this._logs++;
				this._logsHeld[this._logs - 1].SetActive(true);
				LocalPlayer.Sfx.PlayWhoosh();
				if (this._logs == 1)
				{
					if (!this._player.HasInSlot(Item.EquipmentSlot.LeftHand, this._lighterItemId))
					{
						this._player.MemorizeItem(Item.EquipmentSlot.LeftHand);
						this._player.UnequipItemAtSlot(Item.EquipmentSlot.LeftHand, false, true, false);
					}
					if (!LocalPlayer.FpCharacter.drinking)
					{
						this._player.MemorizeItem(Item.EquipmentSlot.RightHand);
						this._player.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, true, false);
					}
					for (int i = 0; i < this._itemCache._equipedAnimVars.Length; i++)
					{
						LocalPlayer.Animator.SetBoolReflected(this._itemCache._equipedAnimVars[i].ToString(), true);
					}
					base.enabled = true;
				}
				this.UpdateLogCount();
				return true;
			}
			this.UpdateLogCount();
			return false;
		}

		public void RemoveLog(bool equipPrevious)
		{
			if (this._logs > 0)
			{
				this._logs--;
				this._logsHeld[this._logs].SetActive(false);
				if (this._logs == 0)
				{
					for (int i = 0; i < this._itemCache._equipedAnimVars.Length; i++)
					{
						LocalPlayer.Animator.SetBoolReflected(this._itemCache._equipedAnimVars[i].ToString(), false);
					}
					if (equipPrevious)
					{
						if (!this._player.HasInSlot(Item.EquipmentSlot.LeftHand, this._lighterItemId))
						{
							this._player.EquipPreviousUtility(false);
						}
						this._player.EquipPreviousWeaponDelayed();
					}
					base.enabled = false;
				}
			}
		}

		public bool PutDown(bool fake = false, bool drop = false, bool equipPrevious = true, GameObject preSpawned = null)
		{
			if (this._infiniteLogHack)
			{
				return true;
			}
			if (!fake)
			{
				if (this._logs <= 0)
				{
					return false;
				}
				this.RemoveLog(equipPrevious);
			}
			if (drop)
			{
				bool flag = false;
				if (this._logs == 1)
				{
					flag = true;
				}
				Transform transform = this._logsHeld[Mathf.Min(this._logs, 1)].transform;
				Vector3 vector = transform.position + transform.forward * -2f;
				Quaternion quaternion = LocalPlayer.Transform.rotation;
				quaternion *= Quaternion.AngleAxis(90f, Vector3.up);
				if (LocalPlayer.FpCharacter.PushingSled)
				{
					vector += transform.forward * -1.25f + transform.right * -2f;
				}
				Vector3 origin = vector;
				origin.y += 3f;
				RaycastHit raycastHit;
				if (Physics.Raycast(origin, Vector3.down, out raycastHit, 5f, this._layerMask))
				{
					vector.y = raycastHit.point.y + 2.2f;
				}
				if (flag)
				{
					vector.y += 1f;
				}
				if (BoltNetwork.isRunning)
				{
					DropItem dropItem = DropItem.Create(GlobalTargets.OnlyServer);
					dropItem.PrefabId = BoltPrefabs.Log;
					dropItem.Position = vector;
					dropItem.Rotation = quaternion;
					dropItem.PreSpawned = ((!preSpawned) ? null : preSpawned.GetComponent<BoltEntity>());
					dropItem.Send();
				}
				else if (preSpawned)
				{
					preSpawned.transform.position = vector;
					preSpawned.transform.rotation = quaternion;
				}
				else
				{
					UnityEngine.Object.Instantiate<GameObject>(this._logPrefab, vector, quaternion);
				}
				FMODCommon.PlayOneshotNetworked("event:/player/foley/log_drop_exert", transform, FMODCommon.NetworkRole.Any);
			}
			this.UpdateLogCount();
			return true;
		}

		private void UpdateLogCount()
		{
			if (BoltNetwork.isRunning)
			{
				int num = 0;
				for (int i = 0; i < this._logsHeld.Length; i++)
				{
					if (this._logsHeld[i] && this._logsHeld[i].activeInHierarchy)
					{
						num++;
					}
				}
				base.state.logsHeld = num;
			}
		}

		public int Amount
		{
			get
			{
				return this._logs;
			}
		}

		public bool HasLogs
		{
			get
			{
				return this._logs > 0;
			}
		}

		[ItemIdPicker]
		public int _logItemId;

		[ItemIdPicker]
		public int _lighterItemId;

		public PlayerInventory _player;

		public GameObject[] _logsHeld;

		public GameObject _logPrefab;

		public bool _infiniteLogHack;

		public LayerMask _layerMask;

		[SerializeThis]
		private int _logs;

		private Item _itemCache;
	}
}
