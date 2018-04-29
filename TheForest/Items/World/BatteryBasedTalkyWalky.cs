using System;
using Bolt;
using TheForest.Items.Inventory;
using UnityEngine;

namespace TheForest.Items.World
{
	
	[AddComponentMenu("Items/World/Battery Based Talky Walky")]
	public class BatteryBasedTalkyWalky : EntityBehaviour<IPlayerState>
	{
		
		public PlayerInventory _player;

		
		public float _batterieCostPerSecond = 0.2f;

		
		public float _delayBeforeStart = 0.5f;
	}
}
