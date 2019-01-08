using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items;
using TheForest.UI.Anim;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class BuildMission : EntityBehaviour<IBuildMissionState>
	{
		private IEnumerator Start()
		{
			if (BoltNetwork.isClient)
			{
				while (this._itemId == 0)
				{
					yield return null;
				}
				this.Init(this._itemId);
			}
			yield break;
		}

		private void Init(int itemId)
		{
			this._itemId = itemId;
			BuildMission.ActiveMissions[this._itemId] = this;
		}

		public void RefreshAmount()
		{
			this.SetAmountNeeded(this._amountNeeded, false);
		}

		public static void AddNeededToBuildMission(int itemId, int amount, bool isCancelling)
		{
			if (!BoltNetwork.isClient)
			{
				BuildMission buildMission;
				if (!BuildMission.ActiveMissions.TryGetValue(itemId, out buildMission))
				{
					if (BoltNetwork.isRunning)
					{
						if (!BoltNetwork.isServer)
						{
							return;
						}
						buildMission = BoltNetwork.Instantiate(Prefabs.Instance.BuildMissionPrefab.gameObject).GetComponent<BuildMission>();
					}
					else
					{
						buildMission = UnityEngine.Object.Instantiate<BuildMission>(Prefabs.Instance.BuildMissionPrefab);
					}
					if (!buildMission)
					{
						return;
					}
					buildMission.ItemId = itemId;
					buildMission.SetAmountNeeded(amount, isCancelling);
				}
				else
				{
					buildMission.SetAmountNeeded(Mathf.Max(buildMission._amountNeeded + amount, 0), isCancelling);
				}
			}
		}

		private bool CheckWidget(bool ignoreIfNull)
		{
			if (!this._buildMissingWidget && !ignoreIfNull)
			{
				this._buildMissingWidget = Scene.HudGui.GetBuildMissionWidget(this._itemId);
				if (this._buildMissingWidget)
				{
					this._buildMissingWidget._buildLog._needed = this._amountNeeded;
					if (BoltNetwork.isClient)
					{
						this.SetAmountNeeded(this._amountNeeded, true);
					}
				}
			}
			return this._buildMissingWidget;
		}

		private void SetAmountNeeded(int value, bool isCancelling)
		{
			this._amountNeeded = value;
			if (BoltNetwork.isServer)
			{
				base.state.AmountNeeded = value;
			}
			if (this.CheckWidget(value == 0))
			{
				if (this._buildMissingWidget && this._buildMissingWidget._buildLog)
				{
					if (this._amountNeeded > 0)
					{
						this._buildMissingWidget._buildLog._needed = this._amountNeeded;
					}
					else
					{
						this._buildMissingWidget.CrossOff();
						this._buildMissingWidget = null;
					}
				}
				else
				{
					Debug.LogError(string.Concat(new object[]
					{
						"No BuildLog for BuildMission itemId=",
						this._itemId,
						" (",
						ItemDatabase.ItemById(this._itemId),
						")"
					}));
				}
			}
		}

		public override void Attached()
		{
			if (!base.entity.isOwner)
			{
				base.state.AddCallback("ItemId", delegate
				{
					this.ItemId = base.state.ItemId;
				});
				base.state.AddCallback("AmountNeeded", delegate
				{
					this.SetAmountNeeded(base.state.AmountNeeded, true);
				});
			}
		}

		public int ItemId
		{
			get
			{
				return this._itemId;
			}
			set
			{
				if (BoltNetwork.isServer)
				{
					base.state.ItemId = value;
				}
				this._itemId = value;
				this.Init(value);
			}
		}

		private int _itemId;

		private int _amountNeeded;

		private BuildMissionWidget _buildMissingWidget;

		public static Dictionary<int, BuildMission> ActiveMissions = new Dictionary<int, BuildMission>();
	}
}
