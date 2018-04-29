using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	
	[HelpURL("http:
	public class TileHandlerHelper : MonoBehaviour
	{
		
		public void UseSpecifiedHandler(TileHandler handler)
		{
			this.handler = handler;
		}

		
		private void OnEnable()
		{
			NavmeshCut.OnDestroyCallback += this.HandleOnDestroyCallback;
		}

		
		private void OnDisable()
		{
			NavmeshCut.OnDestroyCallback -= this.HandleOnDestroyCallback;
		}

		
		public void DiscardPending()
		{
			List<NavmeshCut> all = NavmeshCut.GetAll();
			for (int i = 0; i < all.Count; i++)
			{
				if (all[i].RequiresUpdate())
				{
					all[i].NotifyUpdated();
				}
			}
		}

		
		private void Start()
		{
			if (this.handler == null)
			{
				if (AstarPath.active == null || AstarPath.active.astarData.recastGraph == null)
				{
					Debug.LogWarning("No AstarPath object in the scene or no RecastGraph on that AstarPath object");
				}
				this.handler = new TileHandler(AstarPath.active.astarData.recastGraph);
				this.handler.CreateTileTypesFromGraph();
			}
		}

		
		private void HandleOnDestroyCallback(NavmeshCut obj)
		{
			this.forcedReloadBounds.Add(obj.LastBounds);
			this.lastUpdateTime = -999f;
		}

		
		private void Update()
		{
			if (this.handler == null || ((this.updateInterval == -1f || Time.realtimeSinceStartup - this.lastUpdateTime < this.updateInterval) && this.handler.isValid) || AstarPath.active.isScanning)
			{
				return;
			}
			this.ForceUpdate();
		}

		
		public void ForceUpdate()
		{
			if (this.handler == null)
			{
				throw new Exception("Cannot update graphs. No TileHandler. Do not call this method in Awake.");
			}
			this.lastUpdateTime = Time.realtimeSinceStartup;
			List<NavmeshCut> all = NavmeshCut.GetAll();
			if (!this.handler.isValid)
			{
				Debug.Log("TileHandler no longer matched the underlaying RecastGraph (possibly because of a graph scan). Recreating TileHandler...");
				this.handler = new TileHandler(this.handler.graph);
				this.handler.CreateTileTypesFromGraph();
				this.forcedReloadBounds.Add(new Bounds(Vector3.zero, new Vector3(1E+07f, 1E+07f, 1E+07f)));
			}
			if (this.forcedReloadBounds.Count == 0)
			{
				int num = 0;
				for (int i = 0; i < all.Count; i++)
				{
					if (all[i].RequiresUpdate())
					{
						num++;
						break;
					}
				}
				if (num == 0)
				{
					return;
				}
			}
			bool flag = this.handler.StartBatchLoad();
			for (int j = 0; j < this.forcedReloadBounds.Count; j++)
			{
				this.handler.ReloadInBounds(this.forcedReloadBounds[j]);
			}
			this.forcedReloadBounds.Clear();
			for (int k = 0; k < all.Count; k++)
			{
				if (all[k].enabled)
				{
					if (all[k].RequiresUpdate())
					{
						this.handler.ReloadInBounds(all[k].LastBounds);
						this.handler.ReloadInBounds(all[k].GetBounds());
					}
				}
				else if (all[k].RequiresUpdate())
				{
					this.handler.ReloadInBounds(all[k].LastBounds);
				}
			}
			for (int l = 0; l < all.Count; l++)
			{
				if (all[l].RequiresUpdate())
				{
					all[l].NotifyUpdated();
				}
			}
			if (flag)
			{
				this.handler.EndBatchLoad();
			}
		}

		
		private TileHandler handler;

		
		public float updateInterval;

		
		private float lastUpdateTime = -999f;

		
		private readonly List<Bounds> forcedReloadBounds = new List<Bounds>();
	}
}
