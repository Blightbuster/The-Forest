using System;
using UnityEngine;

namespace Pathfinding
{
	
	public abstract class GridNodeBase : GraphNode
	{
		
		protected GridNodeBase(AstarPath astar) : base(astar)
		{
		}

		
		
		
		public int NodeInGridIndex
		{
			get
			{
				return this.nodeInGridIndex;
			}
			set
			{
				this.nodeInGridIndex = value;
			}
		}

		
		
		
		public bool WalkableErosion
		{
			get
			{
				return (this.gridFlags & 256) != 0;
			}
			set
			{
				this.gridFlags = (ushort)(((int)this.gridFlags & -257) | ((!value) ? 0 : 256));
			}
		}

		
		
		
		public bool TmpWalkable
		{
			get
			{
				return (this.gridFlags & 512) != 0;
			}
			set
			{
				this.gridFlags = (ushort)(((int)this.gridFlags & -513) | ((!value) ? 0 : 512));
			}
		}

		
		public override float SurfaceArea()
		{
			GridGraph gridGraph = GridNode.GetGridGraph(base.GraphIndex);
			return gridGraph.nodeSize * gridGraph.nodeSize;
		}

		
		public override Vector3 RandomPointOnSurface()
		{
			GridGraph gridGraph = GridNode.GetGridGraph(base.GraphIndex);
			return (Vector3)this.position + new Vector3(UnityEngine.Random.value - 0.5f, 0f, UnityEngine.Random.value - 0.5f) * gridGraph.nodeSize;
		}

		
		public override void AddConnection(GraphNode node, uint cost)
		{
			throw new NotImplementedException("GridNodes do not have support for adding manual connections with your current settings.\nPlease disable ASTAR_GRID_NO_CUSTOM_CONNECTIONS in the Optimizations tab in the A* Inspector");
		}

		
		public override void RemoveConnection(GraphNode node)
		{
			throw new NotImplementedException("GridNodes do not have support for adding manual connections with your current settings.\nPlease disable ASTAR_GRID_NO_CUSTOM_CONNECTIONS in the Optimizations tab in the A* Inspector");
		}

		
		private const int GridFlagsWalkableErosionOffset = 8;

		
		private const int GridFlagsWalkableErosionMask = 256;

		
		private const int GridFlagsWalkableTmpOffset = 9;

		
		private const int GridFlagsWalkableTmpMask = 512;

		
		protected int nodeInGridIndex;

		
		protected ushort gridFlags;
	}
}
