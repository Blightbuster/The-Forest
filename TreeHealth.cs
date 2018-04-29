﻿using System;
using Bolt;
using FMOD.Studio;
using ModAPI;
using TheForest.Tools;
using TheForest.Utils;
using UltimateCheatmenu;
using UnityEngine;
using UnityEngine.Events;


public class TreeHealth : MonoBehaviour
{
	
	
	
	public BoltEntity LodEntity { get; set; }

	
	private void OnEnable()
	{
		if (Scene.SceneTracker && !Scene.SceneTracker.closeTrees.Contains(base.gameObject))
		{
			Scene.SceneTracker.closeTrees.Add(base.gameObject);
		}
	}

	
	private void OnDisable()
	{
		if (Scene.SceneTracker && Scene.SceneTracker.closeTrees.Contains(base.gameObject))
		{
			Scene.SceneTracker.closeTrees.Remove(base.gameObject);
		}
		if (this.SpawnedCutTree)
		{
			this.LodEntity = null;
			this.Trunk = null;
			this.TrunkUpperSpawn = null;
			this.PerchTargets = null;
			this.LodTree = null;
		}
	}

	
	public void DamageTree()
	{
		if (BoltNetwork.isRunning)
		{
			return;
		}
		this.Health--;
		if (this.Health <= 0)
		{
			this.CutDown();
		}
	}

	
	private void __Hit__Original()
	{
		if (BoltNetwork.isRunning)
		{
			SpawnCutTree spawnCutTree = SpawnCutTree.Raise(GlobalTargets.OnlyServer);
			spawnCutTree.TreeId = this.LodTree.GetComponent<CoopTreeId>().Id;
			spawnCutTree.Send();
		}
		else
		{
			this.DamageTree();
		}
	}

	
	private void Burnt(GameObject trunk)
	{
		if (!BoltNetwork.isClient)
		{
			TreeHealth.OnTreeCutDown.Invoke(base.transform.position);
			EventRegistry.Player.Publish(TfEvent.CutTree, this);
			if (this.LodTree != null)
			{
				this.LodTree.SendMessageToTargets("OnTreeCutDown", trunk);
				if (BoltNetwork.isRunning)
				{
					CoopTreeId component = this.LodTree.GetComponent<CoopTreeId>();
					if (component)
					{
						component.Goto_Removed();
					}
				}
				UnityEngine.Object.Destroy(this.LodTree);
			}
		}
	}

	
	public void SetLodBase(LOD_Base lb)
	{
		this.LodTree = (LOD_Trees)lb;
		this.LodTree.CurrentView = this;
		if (this.SpawnedCutTree)
		{
			this.LodTree.SendMessage("LodChanged", 0, SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void Explosion(float dist)
	{
		if (!this.Exploded)
		{
			if (BoltNetwork.isRunning)
			{
				if (this.LodTree)
				{
					BoltEntity component = this.LodTree.GetComponent<BoltEntity>();
					if (component.isAttached)
					{
						this.Exploded = true;
						if (component.isOwner)
						{
							if (component.canFreeze)
							{
								component.Freeze(false);
							}
							ITreeCutState state = component.GetState<ITreeCutState>();
							if (state.State == 0)
							{
								state.State = 1;
							}
							state.Damage = 16f;
						}
						else
						{
							DestroyTree destroyTree = DestroyTree.Create(GlobalTargets.OnlyServer);
							destroyTree.Tree = component;
							destroyTree.Send();
						}
					}
				}
			}
			else
			{
				this.Exploded = true;
				if (this.SpawnedCutTree)
				{
					this.DoFallTreeExplosion();
				}
				else
				{
					this.CutDownExplosion();
				}
			}
		}
	}

	
	private void CutDownExplosion()
	{
		if (BoltNetwork.isRunning)
		{
			return;
		}
		this.Trunk = (GameObject)UnityEngine.Object.Instantiate(this.Trunk, base.transform.position, base.transform.rotation);
		if (!this.dontScaleTrunk)
		{
			this.Trunk.transform.localScale = base.transform.localScale;
		}
		TreeHealth component = this.Trunk.GetComponent<TreeHealth>();
		component.destroyCutChunks();
		component.SetLodBase(this.LodTree);
		component.CutDown();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	private void CutDown()
	{
		if (BoltNetwork.isRunning)
		{
			return;
		}
		if (this.SpawnedCutTree)
		{
			this.DoFallTree();
		}
		else
		{
			this.DoSpawnCutTree();
		}
	}

	
	public GameObject DoFallTree()
	{
		if (this.TrunkUpperSpawn)
		{
			this.TrunkUpperSpawn = (GameObject)UnityEngine.Object.Instantiate(this.TrunkUpperSpawn, base.transform.position, base.transform.rotation);
			if (!this.dontScaleTrunk)
			{
				this.TrunkUpperSpawn.transform.localScale = base.transform.localScale;
			}
		}
		GameObject trunkUpperSpawn = this.TrunkUpperSpawn;
		this.LodTree.CurrentView = null;
		this.DestroyTrunk();
		TreeHealth.OnTreeCutDown.Invoke(base.transform.position);
		EventRegistry.Player.Publish(TfEvent.CutTree, this);
		return trunkUpperSpawn;
	}

	
	public GameObject DoFallTreeExplosion()
	{
		if (this.TrunkUpperSpawn)
		{
			this.TrunkUpperSpawn = (GameObject)UnityEngine.Object.Instantiate(this.TrunkUpperSpawn, base.transform.position, base.transform.rotation);
			if (!this.dontScaleTrunk)
			{
				this.TrunkUpperSpawn.transform.localScale = base.transform.localScale;
			}
		}
		this.destroyCutChunks();
		this.LodTree.CurrentView = null;
		this.DestroyTrunk();
		GameObject trunkUpperSpawn = this.TrunkUpperSpawn;
		this.TrunkUpperSpawn = null;
		TreeHealth.OnTreeCutDown.Invoke(base.transform.position);
		EventRegistry.Player.Publish(TfEvent.CutTree, this);
		return trunkUpperSpawn;
	}

	
	public void DestroyTrunk()
	{
		UnityEngine.Object.Destroy(this.Trunk);
		if (this.PerchTargets != null)
		{
			this.PerchTargets.SetActive(true);
		}
		if (this.LodTree != null)
		{
			this.LodTree.SendMessageToTargets("OnTreeCutDown", this.TrunkUpperSpawn);
			foreach (object obj in base.transform)
			{
				Transform transform = (Transform)obj;
				ExplodeTreeStump component = transform.GetComponent<ExplodeTreeStump>();
				if (component)
				{
					transform.parent = this.LodTree.transform;
					component.CoolDown();
					UnityEngine.Object.Destroy(base.gameObject, 0.01f);
					break;
				}
			}
		}
	}

	
	public void DoSpawnCutTree()
	{
		if (BoltNetwork.isRunning)
		{
			return;
		}
		EventInstance windEvent = TreeWindSfx.BeginTransfer(base.transform);
		this.Trunk = (GameObject)UnityEngine.Object.Instantiate(this.Trunk, base.transform.position, base.transform.rotation);
		if (!this.dontScaleTrunk)
		{
			this.Trunk.transform.localScale = base.transform.localScale;
		}
		TreeHealth component = this.Trunk.GetComponent<TreeHealth>();
		TreeLeavesRenderer treeLeavesRenderer = this.Trunk.AddComponent<TreeLeavesRenderer>();
		treeLeavesRenderer._leavesRenderer = component.Trunk.GetComponent<Renderer>();
		component.SetLodBase(this.LodTree);
		UnityEngine.Object.Destroy(base.gameObject);
		PrefabId log = BoltPrefabs.Log;
		TreeWindSfx.CompleteTransfer(this.Trunk.transform, windEvent);
	}

	
	public void disableCutChunks()
	{
		TreeCutChunk[] componentsInChildren = base.transform.GetComponentsInChildren<TreeCutChunk>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].transform.parent.gameObject.SetActive(false);
		}
	}

	
	public void destroyCutChunks()
	{
		TreeCutChunk[] componentsInChildren = base.transform.GetComponentsInChildren<TreeCutChunk>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].transform.parent.gameObject);
		}
	}

	
	private void Hit()
	{
		try
		{
			if (UCheatmenu.InstantTree)
			{
				this.Explosion(100f);
				return;
			}
			this.__Hit__Original();
		}
		catch (Exception ex)
		{
			Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
			this.__Hit__Original();
		}
	}

	
	public int Health = 5;

	
	public bool SpawnedCutTree;

	
	public GameObject Trunk;

	
	public GameObject TrunkUpperSpawn;

	
	public GameObject PerchTargets;

	
	public LOD_Trees LodTree;

	
	public bool dontScaleTrunk;

	
	public static TreeHealth.TreeCutDownEvent OnTreeCutDown = new TreeHealth.TreeCutDownEvent();

	
	private bool Exploded;

	
	public class TreeCutDownEvent : UnityEvent<Vector3>
	{
	}
}
