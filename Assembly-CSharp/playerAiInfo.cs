using System;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;

public class playerAiInfo : MonoBehaviour
{
	private void Start()
	{
		this.MyClock = Scene.Clock;
		this.mutantControl = Scene.MutantControler;
		this.sceneInfo = Scene.SceneTracker;
		this.netAnimator = base.transform.Find("player_BASE").GetComponent<Animator>();
		base.InvokeRepeating("checkGroundTag", 1f, 2f);
		if (this.debugAddToPlayerList)
		{
			Scene.SceneTracker.allPlayers.Add(base.gameObject);
		}
	}

	private void checkGroundTag()
	{
		try
		{
			RecastGraph recastGraph = AstarPath.active.astarData.recastGraph;
			this.node = recastGraph.GetNearest(base.transform.position).node;
			this.ugroundTag = this.node.Tag;
			this.playerGroundTag = (int)this.ugroundTag;
		}
		catch
		{
		}
		if (this.remotePlayer)
		{
			this.isSwimming = this.netAnimator.GetBool("swimmingBool");
		}
		else
		{
			this.isSwimming = LocalPlayer.FpCharacter.swimming;
		}
	}

	public void InACave()
	{
		if (this.remotePlayer)
		{
			if (!this.sceneInfo.allPlayersInCave.Contains(base.gameObject))
			{
				this.sceneInfo.allPlayersInCave.Add(base.gameObject);
				this.netInCave = true;
			}
			this.mutantControl.SendMessage("enableMpCaveMutants");
		}
		TerrainCollider terrainCollider = null;
		if (Terrain.activeTerrain)
		{
			terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();
		}
		CapsuleCollider component = base.transform.GetComponent<CapsuleCollider>();
		if (!terrainCollider || !component)
		{
			return;
		}
		if (component.enabled)
		{
			Physics.IgnoreCollision(terrainCollider, component, true);
		}
	}

	public void NotInACave()
	{
		if (this.remotePlayer)
		{
			if (this.sceneInfo.allPlayersInCave.Contains(base.gameObject))
			{
				this.sceneInfo.allPlayersInCave.Remove(base.gameObject);
			}
			if (this.sceneInfo.allPlayersInCave.Count == 0)
			{
				this.mutantControl.SendMessage("disableMpCaveMutants");
				this.netInCave = false;
			}
		}
		TerrainCollider component = Terrain.activeTerrain.GetComponent<TerrainCollider>();
		CapsuleCollider component2 = base.transform.GetComponent<CapsuleCollider>();
		if (!component || !component2)
		{
			return;
		}
		if (component2.enabled)
		{
			Physics.IgnoreCollision(component, component2, false);
		}
	}

	public Animator netAnimator;

	public bool remotePlayer;

	private uint ugroundTag;

	public int playerGroundTag;

	public bool isSwimming;

	private GraphNode node;

	private Clock MyClock;

	private mutantController mutantControl;

	private sceneTracker sceneInfo;

	public bool _net;

	public bool debugAddToPlayerList;

	public bool netInCave;
}
