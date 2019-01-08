using System;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_MeshBaker : MB3_MeshBakerCommon
{
	public override MB3_MeshCombiner meshCombiner
	{
		get
		{
			return this._meshCombiner;
		}
	}

	public void BuildSceneMeshObject()
	{
		this._meshCombiner.BuildSceneMeshObject(null, false);
	}

	public virtual bool ShowHide(GameObject[] gos, GameObject[] deleteGOs)
	{
		return this._meshCombiner.ShowHideGameObjects(gos, deleteGOs);
	}

	public virtual void ApplyShowHide()
	{
		this._meshCombiner.ApplyShowHide();
	}

	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		this._meshCombiner.name = base.name + "-mesh";
		return this._meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
	}

	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
	{
		this._meshCombiner.name = base.name + "-mesh";
		return this._meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOinstanceIDs, disableRendererInSource);
	}

	[SerializeField]
	protected MB3_MeshCombinerSingle _meshCombiner = new MB3_MeshCombinerSingle();
}
