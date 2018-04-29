using System;
using System.Collections;
using TheForest.Commons.Delegates;
using TheForest.Utils;
using UnityEngine;


public class LOD_Manager_StipplingMaterialsSwitcher : MonoBehaviour
{
	
	private void Awake()
	{
		WorkScheduler.RegisterGlobal(new WsTask(this.CheckInCave), false);
	}

	
	private void OnDestroy()
	{
		WorkScheduler.UnregisterGlobal(new WsTask(this.CheckInCave));
	}

	
	private void CheckInCave()
	{
		if (LocalPlayer.IsInCaves)
		{
			this.LoadCave();
		}
		else
		{
			this.LoadWorld();
		}
	}

	
	private void LoadWorld()
	{
		if (!this._worldLODManager && this._request == null)
		{
			base.StartCoroutine(this.LoadWorldRoutine());
		}
	}

	
	private IEnumerator LoadWorldRoutine()
	{
		if (this._cavesLODManager)
		{
			this._cavesLODManager = null;
		}
		this._request = Resources.LoadAsync<LOD_Manager>("LOD Manager world");
		yield return this._request;
		this._worldLODManager = (LOD_Manager)this._request.asset;
		this._request = null;
		this.InitLOD(this._worldLODManager);
		yield break;
	}

	
	private void LoadCave()
	{
		if (!this._cavesLODManager && this._request == null)
		{
			base.StartCoroutine(this.LoadCavesRoutine());
		}
	}

	
	private IEnumerator LoadCavesRoutine()
	{
		if (this._worldLODManager)
		{
			this._worldLODManager = null;
		}
		this._request = Resources.LoadAsync<LOD_Manager>("LOD Manager caves");
		yield return this._request;
		this._cavesLODManager = (LOD_Manager)this._request.asset;
		this._request = null;
		this.InitLOD(this._cavesLODManager);
		yield break;
	}

	
	private void InitLOD(LOD_Manager source)
	{
		this._destination.Bush.StippledMaterials = source.Bush.StippledMaterials;
		this._destination.SmallBush.StippledMaterials = source.SmallBush.StippledMaterials;
		this._destination.Cave.StippledMaterials = source.Cave.StippledMaterials;
		this._destination.CaveEntrance.StippledMaterials = source.CaveEntrance.StippledMaterials;
		this._destination.PickUps.StippledMaterials = source.PickUps.StippledMaterials;
		this._destination.Plant.StippledMaterials = source.Plant.StippledMaterials;
		this._destination.Rocks.StippledMaterials = source.Rocks.StippledMaterials;
		this._destination.SmallRocks.StippledMaterials = source.SmallRocks.StippledMaterials;
		this._destination.Trees.StippledMaterials = source.Trees.StippledMaterials;
	}

	
	public LOD_Manager _destination;

	
	private LOD_Manager _worldLODManager;

	
	private LOD_Manager _cavesLODManager;

	
	private ResourceRequest _request;
}
