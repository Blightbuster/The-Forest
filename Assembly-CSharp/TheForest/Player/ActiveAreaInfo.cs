using System;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.World.Areas;
using UnityEngine;

namespace TheForest.Player
{
	public class ActiveAreaInfo : MonoBehaviour
	{
		public void OnSerializing()
		{
			this._activeAreaHash = Area.GetActiveAreaHash();
			this._isInEndgame = LocalPlayer.IsInEndgame;
		}

		private void OnDeserialized()
		{
			if (base.GetComponent<EmptyObjectIdentifier>() && LocalPlayer.Transform)
			{
				if (this._isInCaves && LocalPlayer.Transform.position.y < Terrain.activeTerrain.SampleHeight(LocalPlayer.Transform.position))
				{
					LocalPlayer.GameObject.SendMessage("InACave", SendMessageOptions.DontRequireReceiver);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
			if (this._currentCave == CaveNames.SnowCave)
			{
				EventRegistry.Player.Publish(TfEvent.EnterSnowCave, null);
			}
		}

		public void SetInCaves(bool inCaves)
		{
			this._isInCaves = inCaves;
			this.IsLeavingCaves = false;
			if (!inCaves)
			{
				this._currentCave = CaveNames.NotInCaves;
			}
		}

		public void SetCurrentCave(CaveNames cave)
		{
			this.IsLeavingCaves = (this._currentCave != CaveNames.NotInCaves && cave == CaveNames.NotInCaves);
			this._currentCave = cave;
		}

		public void EnterSnowCaveHelper()
		{
			this.SetCurrentCave(CaveNames.SnowCave);
		}

		public bool IsInCaves
		{
			get
			{
				return this._isInCaves;
			}
		}

		public bool IsLeavingCaves { get; set; }

		public CaveNames CurrentCave
		{
			get
			{
				return this._currentCave;
			}
		}

		public long ActiveAreaHash
		{
			get
			{
				return this._activeAreaHash;
			}
		}

		public bool HasActiveArea
		{
			get
			{
				return this._activeAreaHash > long.MinValue;
			}
		}

		public bool HasActiveEndgameArea
		{
			get
			{
				return this.HasActiveArea && this._isInEndgame;
			}
		}

		[SerializeThis]
		private long _activeAreaHash;

		[SerializeThis]
		private bool _isInEndgame;

		[SerializeThis]
		private bool _isInCaves;

		[SerializeThis]
		private CaveNames _currentCave = CaveNames.NotInCaves;
	}
}
