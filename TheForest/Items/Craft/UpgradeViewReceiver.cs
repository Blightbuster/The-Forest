﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Items.Craft
{
	
	[AddComponentMenu("Items/Craft/Upgrade View Receiver")]
	[DoNotSerializePublic]
	public class UpgradeViewReceiver : MonoBehaviour
	{
		
		private void Start()
		{
			if (!LevelSerializer.IsDeserializing)
			{
				this._currentUpgrades = new List<UpgradeViewReceiver.UpgradeViewData>();
			}
			if (!this._centerAndRotationAxisUpDirection && this._mainCollider)
			{
				this._centerAndRotationAxisUpDirection = this._mainCollider.transform;
			}
		}

		
		public void OnDeserialized()
		{
			if (base.GetComponent<EmptyObjectIdentifier>())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (base.gameObject.layer != 0)
			{
				base.gameObject.layer = 0;
			}
			if (this._currentUpgrades == null)
			{
				this._currentUpgrades = new List<UpgradeViewReceiver.UpgradeViewData>();
			}
			else if (this._currentUpgrades.Count >= this._upgradeCount)
			{
				this._currentUpgrades.RemoveRange(this._upgradeCount, this._currentUpgrades.Count - this._upgradeCount);
				foreach (UpgradeViewReceiver.UpgradeViewData upgradeViewData in this._currentUpgrades)
				{
					Transform transform = UnityEngine.Object.Instantiate<Transform>(this._upgradeCog.SupportedItemsCache[upgradeViewData.ItemId]._prefab);
					transform.parent = base.transform;
					transform.localPosition = upgradeViewData.Position;
					transform.localRotation = upgradeViewData.Rotation;
					this.SpawnMirrored(upgradeViewData);
				}
			}
		}

		
		public void AddView(int itemId, Transform view)
		{
			UpgradeViewReceiver.UpgradeViewData upgradeViewData = new UpgradeViewReceiver.UpgradeViewData
			{
				ItemId = itemId,
				Position = view.localPosition,
				Rotation = view.localRotation
			};
			this._currentUpgrades.Add(upgradeViewData);
			this._upgradeCount = this._currentUpgrades.Count;
			this.SpawnMirrored(upgradeViewData);
		}

		
		private void SpawnMirrored(UpgradeViewReceiver.UpgradeViewData upgradeViewData)
		{
			if (this._mirrorHeld != null)
			{
				Transform transform = UnityEngine.Object.Instantiate<Transform>(this._upgradeCog.SupportedItemsCache[upgradeViewData.ItemId]._prefab);
				transform.parent = this._mirrorHeld;
				transform.localPosition = upgradeViewData.Position;
				transform.localRotation = upgradeViewData.Rotation;
			}
			if (this._mirrorInventory != null)
			{
				Transform transform = UnityEngine.Object.Instantiate<Transform>(this._upgradeCog.SupportedItemsCache[upgradeViewData.ItemId]._prefab);
				transform.parent = this._mirrorInventory;
				transform.localPosition = upgradeViewData.Position;
				transform.localRotation = upgradeViewData.Rotation;
			}
			this.Version++;
		}

		
		
		
		public List<UpgradeViewReceiver.UpgradeViewData> CurrentUpgrades
		{
			get
			{
				return this._currentUpgrades;
			}
			set
			{
				this._currentUpgrades = value;
			}
		}

		
		
		
		public int Count
		{
			get
			{
				return this._upgradeCount;
			}
			set
			{
				this._upgradeCount = value;
			}
		}

		
		
		
		public int Version { get; set; }

		
		[ItemIdPicker]
		public List<int> _acceptedItemIds;

		
		public MeshCollider _mainCollider;

		
		public Collider _filterCollider;

		
		public Transform _centerAndRotationAxisUpDirection;

		
		public Transform _mirrorInventory;

		
		public Transform _mirrorHeld;

		
		public UpgradeCog _upgradeCog;

		
		[SerializeThis]
		private List<UpgradeViewReceiver.UpgradeViewData> _currentUpgrades;

		
		[SerializeThis]
		private int _upgradeCount;

		
		[SerializeAll]
		[Serializable]
		public class UpgradeViewData
		{
			
			public int ItemId;

			
			public UpgradeViewReceiver.UpgradeViewData.SVector3 Position;

			
			public UpgradeViewReceiver.UpgradeViewData.SVector4 Rotation;

			
			[SerializeAll]
			[Serializable]
			public class SVector3
			{
				
				public static implicit operator Vector3(UpgradeViewReceiver.UpgradeViewData.SVector3 v)
				{
					return new Vector3(v.x, v.y, v.z);
				}

				
				public static implicit operator UpgradeViewReceiver.UpgradeViewData.SVector3(Vector3 v)
				{
					return new UpgradeViewReceiver.UpgradeViewData.SVector3
					{
						x = v.x,
						y = v.y,
						z = v.z
					};
				}

				
				public float x;

				
				public float y;

				
				public float z;
			}

			
			[SerializeAll]
			[Serializable]
			public class SVector4
			{
				
				public static implicit operator Quaternion(UpgradeViewReceiver.UpgradeViewData.SVector4 v)
				{
					return new Quaternion(v.x, v.y, v.z, v.w);
				}

				
				public static implicit operator UpgradeViewReceiver.UpgradeViewData.SVector4(Quaternion q)
				{
					return new UpgradeViewReceiver.UpgradeViewData.SVector4
					{
						x = q.x,
						y = q.y,
						z = q.z,
						w = q.w
					};
				}

				
				public float x;

				
				public float y;

				
				public float z;

				
				public float w;
			}
		}
	}
}
