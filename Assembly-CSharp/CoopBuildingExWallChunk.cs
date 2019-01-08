﻿using System;
using Bolt;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Interfaces;
using UniLinq;
using UnityEngine;

public class CoopBuildingExWallChunk : EntityBehaviour<IWallChunkBuildingState>
{
	public override void Attached()
	{
		CoopWallChunkToken coopWallChunkToken = (CoopWallChunkToken)base.entity.attachToken;
		WallChunkArchitect component = base.GetComponent<WallChunkArchitect>();
		component.transform.position = coopWallChunkToken.P1;
		component.transform.LookAt(coopWallChunkToken.P2);
		component.P1 = coopWallChunkToken.P1;
		component.P2 = coopWallChunkToken.P2;
		component.Addition = coopWallChunkToken.Additions;
		component.Height = coopWallChunkToken.Height;
		component.MultipointPositions = ((coopWallChunkToken.PointsPositions == null) ? null : coopWallChunkToken.PointsPositions.ToList<Vector3>());
		component.WasBuilt = true;
		if (coopWallChunkToken.Support != null)
		{
			component.CurrentSupport = (coopWallChunkToken.Support.GetComponentInChildren(typeof(IStructureSupport)) as IStructureSupport);
		}
		if (!base.entity.isOwner)
		{
			base.entity.SendMessage("OnDeserialized");
		}
	}

	public override void Detached()
	{
		if (BoltNetwork.isClient)
		{
			foreach (BoltEntity boltEntity in base.GetComponentsInChildren<BoltEntity>())
			{
				if (!object.ReferenceEquals(boltEntity, base.entity))
				{
					boltEntity.transform.parent = null;
				}
			}
		}
	}
}
