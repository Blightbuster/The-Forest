using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.World.Areas
{
	
	public class Area : MonoBehaviour
	{
		
		private void Awake()
		{
			if (GameSetup.IsSavedGame && LocalPlayer.ActiveAreaInfo)
			{
				long num = base.transform.ToGeoHash();
				if (num == LocalPlayer.ActiveAreaInfo.ActiveAreaHash)
				{
					this.OnEnter(null);
				}
			}
			else if (this._startArea)
			{
				this.OnEnter(null);
			}
		}

		
		private void OnDestroy()
		{
			if (Area.ActiveArea == this)
			{
				Area.ActiveArea = null;
			}
		}

		
		public void OnEnter(Area from)
		{
			if (Area.ActiveArea != this)
			{
				if (Area.ActiveArea)
				{
					Area.ActiveArea.OnLeave(this);
				}
				Area.ActiveArea = this;
				if (!this._startArea || from)
				{
					this.OnEnterNeighbour();
					foreach (Area area in this._neighbours)
					{
						if (area)
						{
							area.OnEnterNeighbour();
						}
					}
				}
			}
		}

		
		public void OnLeave(Area to)
		{
			if (Area.ActiveArea == this)
			{
				Area.ActiveArea = null;
				if (!this._startArea || to)
				{
					this.OnLeaveNeighbour();
					foreach (Area area in this._neighbours)
					{
						if (area)
						{
							area.OnLeaveNeighbour();
						}
					}
				}
			}
		}

		
		public void OnEnterNeighbour()
		{
			if (++this.NeighbourTokens == 1 && !this._loaded)
			{
				this.Load();
			}
		}

		
		public void OnLeaveNeighbour()
		{
			this.NeighbourTokens = Mathf.Max(this.NeighbourTokens - 1, 0);
			if (this.NeighbourTokens == 0 && this._loaded)
			{
				this.Unload();
			}
		}

		
		public void Load()
		{
			this._loaded = true;
			if (base.enabled)
			{
				this._load.Invoke();
			}
		}

		
		public void Unload()
		{
			this._loaded = false;
			if (base.enabled)
			{
				this._unload.Invoke();
			}
		}

		
		public static long GetActiveAreaHash()
		{
			return (!Area.ActiveArea) ? long.MinValue : Area.ActiveArea.transform.ToGeoHash();
		}

		
		public static bool IsActiveAreaTheStartArea()
		{
			return !Area.ActiveArea || Area.ActiveArea._startArea;
		}

		
		
		
		public int NeighbourTokens { get; private set; }

		
		public Area[] _neighbours;

		
		public UnityEvent _load;

		
		public UnityEvent _unload;

		
		public bool _loaded = true;

		
		public bool _startArea;

		
		private static Area ActiveArea;
	}
}
