using System;
using System.Collections.Generic;
using FMOD.Studio;
using PathologicalGames;
using UnityEngine;


public class LOD_Trees : LOD_Base
{
	
	
	public override SpawnPool Pool
	{
		get
		{
			if (!LOD_Trees._spawnPool)
			{
				LOD_Trees._spawnPool = PoolManager.Pools["Trees"];
			}
			return LOD_Trees._spawnPool;
		}
	}

	
	
	public override LOD_Settings LodSettings
	{
		get
		{
			return LOD_Manager.Instance.Trees;
		}
	}

	
	
	
	public TreeHealth CurrentView { get; set; }

	
	public void AddTreeCutDownTarget(GameObject target)
	{
		if (this.OnTreeCutDownTargets == null)
		{
			this.OnTreeCutDownTargets = new List<GameObject>();
		}
		if (!this.OnTreeCutDownTargets.Contains(target))
		{
			this.OnTreeCutDownTargets.Add(target);
		}
	}

	
	public void RemoveTreeCutDownTarget(GameObject target)
	{
		if (this.OnTreeCutDownTargets != null)
		{
			int num = this.OnTreeCutDownTargets.IndexOf(target);
			if (num >= 0)
			{
				this.OnTreeCutDownTargets.RemoveAt(num);
				if (this.OnTreeCutDownTargets.Count == 0)
				{
					this.OnTreeCutDownTargets = null;
				}
			}
		}
	}

	
	public void SendMessageToTargets(string message, object parameter)
	{
		if (this.OnTreeCutDownTargets != null)
		{
			for (int i = 0; i < this.OnTreeCutDownTargets.Count; i++)
			{
				if (this.OnTreeCutDownTargets[i])
				{
					this.OnTreeCutDownTargets[i].SendMessage(message, parameter, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public override void SetLOD(int lod)
	{
		bool flag = this.currentLOD == 2 || lod == 2;
		EventInstance windEvent;
		if (flag)
		{
			windEvent = TreeWindSfx.BeginTransfer(this.CurrentLodTransform);
		}
		else
		{
			windEvent = null;
		}
		base.SetLOD(lod);
		if (flag)
		{
			TreeWindSfx.CompleteTransfer(this.CurrentLodTransform, windEvent);
		}
	}

	
	public bool SpawnStumpLod()
	{
		if (base.transform.localScale.x >= 1f && this.StumpPrefab)
		{
			foreach (object obj in base.transform)
			{
				Transform transform = (Transform)obj;
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(this.StumpPrefab, base.transform.position, base.transform.rotation) as GameObject;
			gameObject.transform.localScale = this.High.transform.localScale;
			gameObject.transform.parent = base.transform;
			return true;
		}
		return false;
	}

	
	public GameObject StumpPrefab;

	
	protected CustomBillboard hcb;

	
	protected int hBillboardId = -1;

	
	protected List<GameObject> OnTreeCutDownTargets;

	
	private static SpawnPool _spawnPool;
}
