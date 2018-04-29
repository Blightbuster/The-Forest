using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Pathfinding.Recast;
using Pathfinding.Serialization;
using Pathfinding.Util;
using Pathfinding.Voxels;
using UnityEngine;

namespace Pathfinding
{
	
	[JsonOptIn]
	[Serializable]
	public class RecastGraph : NavGraph, INavmesh, IRaycastableGraph, IUpdatableGraph, INavmeshHolder
	{
		
		
		public Bounds forcedBounds
		{
			get
			{
				return new Bounds(this.forcedBoundsCenter, this.forcedBoundsSize);
			}
		}

		
		public RecastGraph.NavmeshTile GetTile(int x, int z)
		{
			return this.tiles[x + z * this.tileXCount];
		}

		
		public Int3 GetVertex(int index)
		{
			int num = index >> 12 & 524287;
			return this.tiles[num].GetVertex(index);
		}

		
		public int GetTileIndex(int index)
		{
			return index >> 12 & 524287;
		}

		
		public int GetVertexArrayIndex(int index)
		{
			return index & 4095;
		}

		
		public void GetTileCoordinates(int tileIndex, out int x, out int z)
		{
			z = tileIndex / this.tileXCount;
			x = tileIndex - z * this.tileXCount;
		}

		
		public RecastGraph.NavmeshTile[] GetTiles()
		{
			return this.tiles;
		}

		
		public Bounds GetTileBounds(IntRect rect)
		{
			return this.GetTileBounds(rect.xmin, rect.ymin, rect.Width, rect.Height);
		}

		
		public Bounds GetTileBounds(int x, int z, int width = 1, int depth = 1)
		{
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3((float)(x * this.tileSizeX) * this.cellSize, 0f, (float)(z * this.tileSizeZ) * this.cellSize) + this.forcedBounds.min, new Vector3((float)((x + width) * this.tileSizeX) * this.cellSize, this.forcedBounds.size.y, (float)((z + depth) * this.tileSizeZ) * this.cellSize) + this.forcedBounds.min);
			return result;
		}

		
		public Int2 GetTileCoordinates(Vector3 p)
		{
			p -= this.forcedBounds.min;
			p.x /= this.cellSize * (float)this.tileSizeX;
			p.z /= this.cellSize * (float)this.tileSizeZ;
			return new Int2((int)p.x, (int)p.z);
		}

		
		public override void OnDestroy()
		{
			base.OnDestroy();
			TriangleMeshNode.SetNavmeshHolder(this.active.astarData.GetGraphIndex(this), null);
		}

		
		public override void RelocateNodes(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
		{
			if (this.tiles != null)
			{
				Matrix4x4 inverse = oldMatrix.inverse;
				Matrix4x4 matrix4x = newMatrix * inverse;
				if (this.tiles.Length > 1)
				{
					throw new Exception("RelocateNodes cannot be used on tiled recast graphs");
				}
				for (int i = 0; i < this.tiles.Length; i++)
				{
					RecastGraph.NavmeshTile navmeshTile = this.tiles[i];
					if (navmeshTile != null)
					{
						Int3[] verts = navmeshTile.verts;
						for (int j = 0; j < verts.Length; j++)
						{
							verts[j] = (Int3)matrix4x.MultiplyPoint((Vector3)verts[j]);
						}
						for (int k = 0; k < navmeshTile.nodes.Length; k++)
						{
							TriangleMeshNode triangleMeshNode = navmeshTile.nodes[k];
							triangleMeshNode.UpdatePositionFromVertices();
						}
						navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
					}
				}
			}
			base.SetMatrix(newMatrix);
		}

		
		private static RecastGraph.NavmeshTile NewEmptyTile(int x, int z)
		{
			return new RecastGraph.NavmeshTile
			{
				x = x,
				z = z,
				w = 1,
				d = 1,
				verts = new Int3[0],
				tris = new int[0],
				nodes = new TriangleMeshNode[0],
				bbTree = ObjectPool<BBTree>.Claim()
			};
		}

		
		public override void GetNodes(GraphNodeDelegateCancelable del)
		{
			if (this.tiles == null)
			{
				return;
			}
			for (int i = 0; i < this.tiles.Length; i++)
			{
				if (this.tiles[i] != null && this.tiles[i].x + this.tiles[i].z * this.tileXCount == i)
				{
					TriangleMeshNode[] nodes = this.tiles[i].nodes;
					if (nodes != null)
					{
						int num = 0;
						while (num < nodes.Length && del(nodes[num]))
						{
							num++;
						}
					}
				}
			}
		}

		
		[Obsolete("Use node.ClosestPointOnNode instead")]
		public Vector3 ClosestPointOnNode(TriangleMeshNode node, Vector3 pos)
		{
			return Polygon.ClosestPointOnTriangle((Vector3)this.GetVertex(node.v0), (Vector3)this.GetVertex(node.v1), (Vector3)this.GetVertex(node.v2), pos);
		}

		
		[Obsolete("Use node.ContainsPoint instead")]
		public bool ContainsPoint(TriangleMeshNode node, Vector3 pos)
		{
			return VectorMath.IsClockwiseXZ((Vector3)this.GetVertex(node.v0), (Vector3)this.GetVertex(node.v1), pos) && VectorMath.IsClockwiseXZ((Vector3)this.GetVertex(node.v1), (Vector3)this.GetVertex(node.v2), pos) && VectorMath.IsClockwiseXZ((Vector3)this.GetVertex(node.v2), (Vector3)this.GetVertex(node.v0), pos);
		}

		
		public void SnapForceBoundsToScene()
		{
			List<RasterizationMesh> list = this.CollectMeshes(new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)));
			if (list.Count == 0)
			{
				return;
			}
			Bounds bounds = list[0].bounds;
			for (int i = 1; i < list.Count; i++)
			{
				bounds.Encapsulate(list[i].bounds);
			}
			this.forcedBoundsCenter = bounds.center;
			this.forcedBoundsSize = bounds.size;
		}

		
		public IntRect GetTouchingTiles(Bounds b)
		{
			b.center -= this.forcedBounds.min;
			IntRect intRect = new IntRect(Mathf.FloorToInt(b.min.x / ((float)this.tileSizeX * this.cellSize)), Mathf.FloorToInt(b.min.z / ((float)this.tileSizeZ * this.cellSize)), Mathf.FloorToInt(b.max.x / ((float)this.tileSizeX * this.cellSize)), Mathf.FloorToInt(b.max.z / ((float)this.tileSizeZ * this.cellSize)));
			intRect = IntRect.Intersection(intRect, new IntRect(0, 0, this.tileXCount - 1, this.tileZCount - 1));
			return intRect;
		}

		
		public IntRect GetTouchingTilesRound(Bounds b)
		{
			b.center -= this.forcedBounds.min;
			IntRect intRect = new IntRect(Mathf.RoundToInt(b.min.x / ((float)this.tileSizeX * this.cellSize)), Mathf.RoundToInt(b.min.z / ((float)this.tileSizeZ * this.cellSize)), Mathf.RoundToInt(b.max.x / ((float)this.tileSizeX * this.cellSize)) - 1, Mathf.RoundToInt(b.max.z / ((float)this.tileSizeZ * this.cellSize)) - 1);
			intRect = IntRect.Intersection(intRect, new IntRect(0, 0, this.tileXCount - 1, this.tileZCount - 1));
			return intRect;
		}

		
		GraphUpdateThreading IUpdatableGraph.CanUpdateAsync(GraphUpdateObject o)
		{
			return (!o.updatePhysics) ? GraphUpdateThreading.SeparateThread : ((GraphUpdateThreading)7);
		}

		
		void IUpdatableGraph.UpdateAreaInit(GraphUpdateObject o)
		{
			if (!o.updatePhysics)
			{
				return;
			}
			if (!this.dynamic)
			{
				throw new Exception("Recast graph must be marked as dynamic to enable graph updates");
			}
			RelevantGraphSurface.UpdateAllPositions();
			IntRect touchingTiles = this.GetTouchingTiles(o.bounds);
			Bounds tileBounds = this.GetTileBounds(touchingTiles);
			tileBounds.Expand(new Vector3(1f, 0f, 1f) * this.TileBorderSizeInWorldUnits * 2f);
			List<RasterizationMesh> inputMeshes = this.CollectMeshes(tileBounds);
			if (this.globalVox == null)
			{
				this.globalVox = new Voxelize(this.CellHeight, this.cellSize, this.walkableClimb, this.walkableHeight, this.maxSlope);
				this.globalVox.maxEdgeLength = this.maxEdgeLength;
			}
			this.globalVox.inputMeshes = inputMeshes;
		}

		
		void IUpdatableGraph.UpdateArea(GraphUpdateObject guo)
		{
			IntRect touchingTiles = this.GetTouchingTiles(guo.bounds);
			if (!guo.updatePhysics)
			{
				for (int i = touchingTiles.ymin; i <= touchingTiles.ymax; i++)
				{
					for (int j = touchingTiles.xmin; j <= touchingTiles.xmax; j++)
					{
						RecastGraph.NavmeshTile graph = this.tiles[i * this.tileXCount + j];
						NavMeshGraph.UpdateArea(guo, graph);
					}
				}
				return;
			}
			if (!this.dynamic)
			{
				throw new Exception("Recast graph must be marked as dynamic to enable graph updates with updatePhysics = true");
			}
			Voxelize voxelize = this.globalVox;
			if (voxelize == null)
			{
				throw new InvalidOperationException("No Voxelizer object. UpdateAreaInit should have been called before this function.");
			}
			for (int k = touchingTiles.xmin; k <= touchingTiles.xmax; k++)
			{
				for (int l = touchingTiles.ymin; l <= touchingTiles.ymax; l++)
				{
					this.stagingTiles.Add(this.BuildTileMesh(voxelize, k, l, 0));
				}
			}
			uint graphIndex = (uint)AstarPath.active.astarData.GetGraphIndex(this);
			for (int m = 0; m < this.stagingTiles.Count; m++)
			{
				RecastGraph.NavmeshTile navmeshTile = this.stagingTiles[m];
				GraphNode[] nodes = navmeshTile.nodes;
				for (int n = 0; n < nodes.Length; n++)
				{
					nodes[n].GraphIndex = graphIndex;
				}
			}
		}

		
		void IUpdatableGraph.UpdateAreaPost(GraphUpdateObject guo)
		{
			for (int i = 0; i < this.stagingTiles.Count; i++)
			{
				RecastGraph.NavmeshTile navmeshTile = this.stagingTiles[i];
				int num = navmeshTile.x + navmeshTile.z * this.tileXCount;
				RecastGraph.NavmeshTile navmeshTile2 = this.tiles[num];
				for (int j = 0; j < navmeshTile2.nodes.Length; j++)
				{
					navmeshTile2.nodes[j].Destroy();
				}
				this.tiles[num] = navmeshTile;
			}
			for (int k = 0; k < this.stagingTiles.Count; k++)
			{
				RecastGraph.NavmeshTile tile = this.stagingTiles[k];
				this.ConnectTileWithNeighbours(tile, false);
			}
			this.stagingTiles.Clear();
		}

		
		private void ConnectTileWithNeighbours(RecastGraph.NavmeshTile tile, bool onlyUnflagged = false)
		{
			if (tile.w != 1 || tile.d != 1)
			{
				throw new ArgumentException("Tile widths or depths other than 1 are not supported. The fields exist mainly for possible future expansions.");
			}
			for (int i = -1; i <= 1; i++)
			{
				int num = tile.z + i;
				if (num >= 0 && num < this.tileZCount)
				{
					for (int j = -1; j <= 1; j++)
					{
						int num2 = tile.x + j;
						if (num2 >= 0 && num2 < this.tileXCount)
						{
							if (j == 0 != (i == 0))
							{
								RecastGraph.NavmeshTile navmeshTile = this.tiles[num2 + num * this.tileXCount];
								if (!onlyUnflagged || !navmeshTile.flag)
								{
									this.ConnectTiles(navmeshTile, tile);
								}
							}
						}
					}
				}
			}
		}

		
		private void RemoveConnectionsFromTile(RecastGraph.NavmeshTile tile)
		{
			if (tile.x > 0)
			{
				int num = tile.x - 1;
				for (int i = tile.z; i < tile.z + tile.d; i++)
				{
					this.RemoveConnectionsFromTo(this.tiles[num + i * this.tileXCount], tile);
				}
			}
			if (tile.x + tile.w < this.tileXCount)
			{
				int num2 = tile.x + tile.w;
				for (int j = tile.z; j < tile.z + tile.d; j++)
				{
					this.RemoveConnectionsFromTo(this.tiles[num2 + j * this.tileXCount], tile);
				}
			}
			if (tile.z > 0)
			{
				int num3 = tile.z - 1;
				for (int k = tile.x; k < tile.x + tile.w; k++)
				{
					this.RemoveConnectionsFromTo(this.tiles[k + num3 * this.tileXCount], tile);
				}
			}
			if (tile.z + tile.d < this.tileZCount)
			{
				int num4 = tile.z + tile.d;
				for (int l = tile.x; l < tile.x + tile.w; l++)
				{
					this.RemoveConnectionsFromTo(this.tiles[l + num4 * this.tileXCount], tile);
				}
			}
		}

		
		private void RemoveConnectionsFromTo(RecastGraph.NavmeshTile a, RecastGraph.NavmeshTile b)
		{
			if (a == null || b == null)
			{
				return;
			}
			if (a == b)
			{
				return;
			}
			int num = b.x + b.z * this.tileXCount;
			for (int i = 0; i < a.nodes.Length; i++)
			{
				TriangleMeshNode triangleMeshNode = a.nodes[i];
				if (triangleMeshNode.connections != null)
				{
					for (int j = 0; j < triangleMeshNode.connections.Length; j++)
					{
						TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[j] as TriangleMeshNode;
						if (triangleMeshNode2 != null)
						{
							int num2 = triangleMeshNode2.GetVertexIndex(0);
							num2 = (num2 >> 12 & 524287);
							if (num2 == num)
							{
								triangleMeshNode.RemoveConnection(triangleMeshNode.connections[j]);
								j--;
							}
						}
					}
				}
			}
		}

		
		public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
		{
			return this.GetNearestForce(position, null);
		}

		
		public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
		{
			if (this.tiles == null)
			{
				return default(NNInfoInternal);
			}
			Vector3 vector = position - this.forcedBounds.min;
			int num = Mathf.FloorToInt(vector.x / (this.cellSize * (float)this.tileSizeX));
			int num2 = Mathf.FloorToInt(vector.z / (this.cellSize * (float)this.tileSizeZ));
			num = Mathf.Clamp(num, 0, this.tileXCount - 1);
			num2 = Mathf.Clamp(num2, 0, this.tileZCount - 1);
			int num3 = Math.Max(this.tileXCount, this.tileZCount);
			NNInfoInternal nninfoInternal = default(NNInfoInternal);
			float positiveInfinity = float.PositiveInfinity;
			bool flag = this.nearestSearchOnlyXZ || (constraint != null && constraint.distanceXZ);
			for (int i = 0; i < num3; i++)
			{
				if (!flag && positiveInfinity < (float)(i - 1) * this.cellSize * (float)Math.Max(this.tileSizeX, this.tileSizeZ))
				{
					break;
				}
				int num4 = Math.Min(i + num2 + 1, this.tileZCount);
				for (int j = Math.Max(-i + num2, 0); j < num4; j++)
				{
					int num5 = Math.Abs(i - Math.Abs(j - num2));
					if (-num5 + num >= 0)
					{
						int num6 = -num5 + num;
						RecastGraph.NavmeshTile navmeshTile = this.tiles[num6 + j * this.tileXCount];
						if (navmeshTile != null)
						{
							if (flag)
							{
								nninfoInternal = navmeshTile.bbTree.QueryClosestXZ(position, constraint, ref positiveInfinity, nninfoInternal);
								if (positiveInfinity < float.PositiveInfinity)
								{
									break;
								}
							}
							else
							{
								nninfoInternal = navmeshTile.bbTree.QueryClosest(position, constraint, ref positiveInfinity, nninfoInternal);
							}
						}
					}
					if (num5 != 0 && num5 + num < this.tileXCount)
					{
						int num7 = num5 + num;
						RecastGraph.NavmeshTile navmeshTile2 = this.tiles[num7 + j * this.tileXCount];
						if (navmeshTile2 != null)
						{
							if (flag)
							{
								nninfoInternal = navmeshTile2.bbTree.QueryClosestXZ(position, constraint, ref positiveInfinity, nninfoInternal);
								if (positiveInfinity < float.PositiveInfinity)
								{
									break;
								}
							}
							else
							{
								nninfoInternal = navmeshTile2.bbTree.QueryClosest(position, constraint, ref positiveInfinity, nninfoInternal);
							}
						}
					}
				}
			}
			nninfoInternal.node = nninfoInternal.constrainedNode;
			nninfoInternal.constrainedNode = null;
			nninfoInternal.clampedPosition = nninfoInternal.constClampedPosition;
			return nninfoInternal;
		}

		
		public GraphNode PointOnNavmesh(Vector3 position, NNConstraint constraint)
		{
			if (this.tiles == null)
			{
				return null;
			}
			Vector3 vector = position - this.forcedBounds.min;
			int num = Mathf.FloorToInt(vector.x / (this.cellSize * (float)this.tileSizeX));
			int num2 = Mathf.FloorToInt(vector.z / (this.cellSize * (float)this.tileSizeZ));
			if (num < 0 || num2 < 0 || num >= this.tileXCount || num2 >= this.tileZCount)
			{
				return null;
			}
			RecastGraph.NavmeshTile navmeshTile = this.tiles[num + num2 * this.tileXCount];
			if (navmeshTile != null)
			{
				return navmeshTile.bbTree.QueryInside(position, constraint);
			}
			return null;
		}

		
		public override IEnumerable<Progress> ScanInternal()
		{
			TriangleMeshNode.SetNavmeshHolder(AstarPath.active.astarData.GetGraphIndex(this), this);
			foreach (Progress p in this.ScanAllTiles())
			{
				yield return p;
			}
			yield break;
		}

		
		private void InitializeTileInfo()
		{
			int num = Mathf.Max((int)(this.forcedBounds.size.x / this.cellSize + 0.5f), 1);
			int num2 = Mathf.Max((int)(this.forcedBounds.size.z / this.cellSize + 0.5f), 1);
			if (!this.useTiles)
			{
				this.tileSizeX = num;
				this.tileSizeZ = num2;
			}
			else
			{
				this.tileSizeX = this.editorTileSize;
				this.tileSizeZ = this.editorTileSize;
			}
			this.tileXCount = (num + this.tileSizeX - 1) / this.tileSizeX;
			this.tileZCount = (num2 + this.tileSizeZ - 1) / this.tileSizeZ;
			if (this.tileXCount * this.tileZCount > 524288)
			{
				throw new Exception(string.Concat(new object[]
				{
					"Too many tiles (",
					this.tileXCount * this.tileZCount,
					") maximum is ",
					524288,
					"\nTry disabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* inspector."
				}));
			}
			this.tiles = new RecastGraph.NavmeshTile[this.tileXCount * this.tileZCount];
		}

		
		private void BuildTiles(Queue<Int2> tileQueue, List<RasterizationMesh>[] meshBuckets, ManualResetEvent doneEvent, int threadIndex)
		{
			try
			{
				Voxelize voxelize = new Voxelize(this.CellHeight, this.cellSize, this.walkableClimb, this.walkableHeight, this.maxSlope);
				voxelize.maxEdgeLength = this.maxEdgeLength;
				for (;;)
				{
					Int2 @int;
					lock (tileQueue)
					{
						if (tileQueue.Count == 0)
						{
							break;
						}
						@int = tileQueue.Dequeue();
					}
					voxelize.inputMeshes = meshBuckets[@int.x + @int.y * this.tileXCount];
					this.tiles[@int.x + @int.y * this.tileXCount] = this.BuildTileMesh(voxelize, @int.x, @int.y, threadIndex);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				if (doneEvent != null)
				{
					doneEvent.Set();
				}
			}
		}

		
		private void ConnectTiles(Queue<Int2> tileQueue, ManualResetEvent doneEvent)
		{
			try
			{
				for (;;)
				{
					Int2 @int;
					lock (tileQueue)
					{
						if (tileQueue.Count == 0)
						{
							break;
						}
						@int = tileQueue.Dequeue();
					}
					if (@int.x < this.tileXCount - 1)
					{
						this.ConnectTiles(this.tiles[@int.x + @int.y * this.tileXCount], this.tiles[@int.x + 1 + @int.y * this.tileXCount]);
					}
					if (@int.y < this.tileZCount - 1)
					{
						this.ConnectTiles(this.tiles[@int.x + @int.y * this.tileXCount], this.tiles[@int.x + (@int.y + 1) * this.tileXCount]);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				if (doneEvent != null)
				{
					doneEvent.Set();
				}
			}
		}

		
		private List<RasterizationMesh>[] PutMeshesIntoTileBuckets(List<RasterizationMesh> meshes)
		{
			List<RasterizationMesh>[] array = new List<RasterizationMesh>[this.tiles.Length];
			Vector3 amount = new Vector3(1f, 0f, 1f) * this.TileBorderSizeInWorldUnits * 2f;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new List<RasterizationMesh>();
			}
			for (int j = 0; j < meshes.Count; j++)
			{
				RasterizationMesh rasterizationMesh = meshes[j];
				Bounds bounds = rasterizationMesh.bounds;
				bounds.Expand(amount);
				IntRect touchingTiles = this.GetTouchingTiles(bounds);
				for (int k = touchingTiles.ymin; k <= touchingTiles.ymax; k++)
				{
					for (int l = touchingTiles.xmin; l <= touchingTiles.xmax; l++)
					{
						array[l + k * this.tileXCount].Add(rasterizationMesh);
					}
				}
			}
			return array;
		}

		
		protected IEnumerable<Progress> ScanAllTiles()
		{
			this.InitializeTileInfo();
			if (this.scanEmptyGraph)
			{
				this.FillWithEmptyTiles();
				yield break;
			}
			yield return new Progress(0f, "Finding Meshes");
			List<RasterizationMesh> meshes = this.CollectMeshes(this.forcedBounds);
			this.walkableClimb = Mathf.Min(this.walkableClimb, this.walkableHeight);
			List<RasterizationMesh>[] buckets = this.PutMeshesIntoTileBuckets(meshes);
			Queue<Int2> tileQueue = new Queue<Int2>();
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					tileQueue.Enqueue(new Int2(j, i));
				}
			}
			int threadCount = Mathf.Min(tileQueue.Count, Mathf.Max(1, AstarPath.CalculateThreadCount(ThreadCount.AutomaticHighLoad)));
			ManualResetEvent[] waitEvents = new ManualResetEvent[threadCount];
			for (int k = 0; k < waitEvents.Length; k++)
			{
				waitEvents[k] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(delegate(object state)
				{
					this.$this.BuildTiles(tileQueue, buckets, waitEvents[(int)state], (int)state);
				}, k);
			}
			int timeoutMillis = (!Application.isPlaying) ? 200 : 1;
			while (!WaitHandle.WaitAll(waitEvents, timeoutMillis))
			{
				object tileQueue3 = tileQueue;
				int count;
				lock (tileQueue3)
				{
					count = tileQueue.Count;
				}
				yield return new Progress(Mathf.Lerp(0.1f, 0.9f, (float)(this.tiles.Length - count + 1) / (float)this.tiles.Length), string.Concat(new object[]
				{
					"Generating Tile ",
					this.tiles.Length - count + 1,
					"/",
					this.tiles.Length
				}));
			}
			yield return new Progress(0.9f, "Assigning Graph Indices");
			uint graphIndex = (uint)AstarPath.active.astarData.GetGraphIndex(this);
			this.GetNodes(delegate(GraphNode node)
			{
				node.GraphIndex = graphIndex;
				return true;
			});
			for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum++)
			{
				for (int l = 0; l < this.tiles.Length; l++)
				{
					if ((this.tiles[l].x + this.tiles[l].z) % 2 == coordinateSum)
					{
						tileQueue.Enqueue(new Int2(this.tiles[l].x, this.tiles[l].z));
					}
				}
				for (int m = 0; m < waitEvents.Length; m++)
				{
					waitEvents[m].Reset();
					ThreadPool.QueueUserWorkItem(delegate(object state)
					{
						this.$this.ConnectTiles(tileQueue, state as ManualResetEvent);
					}, waitEvents[m]);
				}
				while (!WaitHandle.WaitAll(waitEvents, timeoutMillis))
				{
					object tileQueue2 = tileQueue;
					int count2;
					lock (tileQueue2)
					{
						count2 = tileQueue.Count;
					}
					yield return new Progress(Mathf.Lerp(0.9f, 1f, (float)(this.tiles.Length - count2 + 1) / (float)this.tiles.Length), string.Concat(new object[]
					{
						"Connecting Tile ",
						this.tiles.Length - count2 + 1,
						"/",
						this.tiles.Length,
						" (Phase ",
						coordinateSum + 1,
						")"
					}));
				}
			}
			yield break;
		}

		
		private List<RasterizationMesh> CollectMeshes(Bounds bounds)
		{
			List<RasterizationMesh> list = new List<RasterizationMesh>();
			RecastMeshGatherer recastMeshGatherer = new RecastMeshGatherer(bounds, this.terrainSampleSize, this.mask, this.tagMask, this.colliderRasterizeDetail);
			if (this.rasterizeMeshes)
			{
				recastMeshGatherer.CollectSceneMeshes(list);
			}
			recastMeshGatherer.CollectRecastMeshObjs(list);
			if (this.rasterizeTerrain)
			{
				float desiredChunkSize = this.cellSize * (float)Math.Max(this.tileSizeX, this.tileSizeZ);
				recastMeshGatherer.CollectTerrainMeshes(this.rasterizeTrees, desiredChunkSize, list);
			}
			if (this.rasterizeColliders)
			{
				recastMeshGatherer.CollectColliderMeshes(list);
			}
			if (list.Count == 0)
			{
				Debug.LogWarning("No MeshFilters were found contained in the layers specified by the 'mask' variables");
			}
			return list;
		}

		
		private void FillWithEmptyTiles()
		{
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					this.tiles[i * this.tileXCount + j] = RecastGraph.NewEmptyTile(j, i);
				}
			}
		}

		
		
		private float CellHeight
		{
			get
			{
				return Mathf.Max(this.forcedBounds.size.y / 64000f, 0.001f);
			}
		}

		
		
		private int CharacterRadiusInVoxels
		{
			get
			{
				return Mathf.CeilToInt(this.characterRadius / this.cellSize - 0.1f);
			}
		}

		
		
		private int TileBorderSizeInVoxels
		{
			get
			{
				return this.CharacterRadiusInVoxels + 3;
			}
		}

		
		
		private float TileBorderSizeInWorldUnits
		{
			get
			{
				return (float)this.TileBorderSizeInVoxels * this.cellSize;
			}
		}

		
		private Bounds CalculateTileBoundsWithBorder(int x, int z)
		{
			float num = (float)this.tileSizeX * this.cellSize;
			float num2 = (float)this.tileSizeZ * this.cellSize;
			Vector3 min = this.forcedBounds.min;
			Vector3 max = this.forcedBounds.max;
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3((float)x * num, 0f, (float)z * num2) + min, new Vector3((float)(x + 1) * num + min.x, max.y, (float)(z + 1) * num2 + min.z));
			result.Expand(new Vector3(1f, 0f, 1f) * this.TileBorderSizeInWorldUnits * 2f);
			return result;
		}

		
		protected RecastGraph.NavmeshTile BuildTileMesh(Voxelize vox, int x, int z, int threadIndex = 0)
		{
			vox.borderSize = this.TileBorderSizeInVoxels;
			vox.forcedBounds = this.CalculateTileBoundsWithBorder(x, z);
			vox.width = this.tileSizeX + vox.borderSize * 2;
			vox.depth = this.tileSizeZ + vox.borderSize * 2;
			if (!this.useTiles && this.relevantGraphSurfaceMode == RecastGraph.RelevantGraphSurfaceMode.OnlyForCompletelyInsideTile)
			{
				vox.relevantGraphSurfaceMode = RecastGraph.RelevantGraphSurfaceMode.RequireForAll;
			}
			else
			{
				vox.relevantGraphSurfaceMode = this.relevantGraphSurfaceMode;
			}
			vox.minRegionSize = Mathf.RoundToInt(this.minRegionSize / (this.cellSize * this.cellSize));
			vox.Init();
			vox.VoxelizeInput();
			vox.FilterLedges(vox.voxelWalkableHeight, vox.voxelWalkableClimb, vox.cellSize, vox.cellHeight, vox.forcedBounds.min);
			vox.FilterLowHeightSpans(vox.voxelWalkableHeight, vox.cellSize, vox.cellHeight, vox.forcedBounds.min);
			vox.BuildCompactField();
			vox.BuildVoxelConnections();
			vox.ErodeWalkableArea(this.CharacterRadiusInVoxels);
			vox.BuildDistanceField();
			vox.BuildRegions();
			VoxelContourSet cset = new VoxelContourSet();
			vox.BuildContours(this.contourMaxError, 1, cset, 1);
			VoxelMesh mesh;
			vox.BuildPolyMesh(cset, 3, out mesh);
			for (int i = 0; i < mesh.verts.Length; i++)
			{
				mesh.verts[i] = vox.VoxelToWorldInt3(mesh.verts[i]);
			}
			return this.CreateTile(vox, mesh, x, z, threadIndex);
		}

		
		private RecastGraph.NavmeshTile CreateTile(Voxelize vox, VoxelMesh mesh, int x, int z, int threadIndex = 0)
		{
			if (mesh.tris == null)
			{
				throw new ArgumentNullException("mesh.tris");
			}
			if (mesh.verts == null)
			{
				throw new ArgumentNullException("mesh.verts");
			}
			RecastGraph.NavmeshTile navmeshTile = new RecastGraph.NavmeshTile
			{
				x = x,
				z = z,
				w = 1,
				d = 1,
				tris = mesh.tris,
				verts = mesh.verts,
				bbTree = new BBTree()
			};
			if (navmeshTile.tris.Length % 3 != 0)
			{
				throw new ArgumentException("Indices array's length must be a multiple of 3 (mesh.tris)");
			}
			if (navmeshTile.verts.Length >= 4095)
			{
				if (this.tileXCount * this.tileZCount == 1)
				{
					throw new ArgumentException("Too many vertices per tile (more than " + 4095 + ").\n<b>Try enabling tiling in the recast graph settings.</b>\n");
				}
				throw new ArgumentException("Too many vertices per tile (more than " + 4095 + ").\n<b>Try reducing tile size or enabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector</b>");
			}
			else
			{
				navmeshTile.verts = Utility.RemoveDuplicateVertices(navmeshTile.verts, navmeshTile.tris);
				TriangleMeshNode[] array = new TriangleMeshNode[navmeshTile.tris.Length / 3];
				navmeshTile.nodes = array;
				uint num = (uint)(AstarPath.active.astarData.graphs.Length + threadIndex);
				if (num > 255u)
				{
					throw new Exception("Graph limit reached. Multithreaded recast calculations cannot be done because a few scratch graph indices are required.");
				}
				int num2 = x + z * this.tileXCount;
				num2 <<= 12;
				TriangleMeshNode.SetNavmeshHolder((int)num, navmeshTile);
				object active = AstarPath.active;
				lock (active)
				{
					for (int i = 0; i < array.Length; i++)
					{
						TriangleMeshNode triangleMeshNode = new TriangleMeshNode(this.active);
						array[i] = triangleMeshNode;
						triangleMeshNode.GraphIndex = num;
						triangleMeshNode.v0 = (navmeshTile.tris[i * 3] | num2);
						triangleMeshNode.v1 = (navmeshTile.tris[i * 3 + 1] | num2);
						triangleMeshNode.v2 = (navmeshTile.tris[i * 3 + 2] | num2);
						if (!VectorMath.IsClockwiseXZ(triangleMeshNode.GetVertex(0), triangleMeshNode.GetVertex(1), triangleMeshNode.GetVertex(2)))
						{
							int v = triangleMeshNode.v0;
							triangleMeshNode.v0 = triangleMeshNode.v2;
							triangleMeshNode.v2 = v;
						}
						triangleMeshNode.Walkable = true;
						triangleMeshNode.Penalty = this.initialPenalty;
						triangleMeshNode.UpdatePositionFromVertices();
					}
				}
				navmeshTile.bbTree.RebuildFrom(array);
				this.CreateNodeConnections(navmeshTile.nodes);
				TriangleMeshNode.SetNavmeshHolder((int)num, null);
				return navmeshTile;
			}
		}

		
		private void CreateNodeConnections(TriangleMeshNode[] nodes)
		{
			List<MeshNode> list = ListPool<MeshNode>.Claim();
			List<uint> list2 = ListPool<uint>.Claim();
			Dictionary<Int2, int> dictionary = ObjectPoolSimple<Dictionary<Int2, int>>.Claim();
			dictionary.Clear();
			for (int i = 0; i < nodes.Length; i++)
			{
				TriangleMeshNode triangleMeshNode = nodes[i];
				int vertexCount = triangleMeshNode.GetVertexCount();
				for (int j = 0; j < vertexCount; j++)
				{
					Int2 key = new Int2(triangleMeshNode.GetVertexIndex(j), triangleMeshNode.GetVertexIndex((j + 1) % vertexCount));
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, i);
					}
				}
			}
			foreach (TriangleMeshNode triangleMeshNode2 in nodes)
			{
				list.Clear();
				list2.Clear();
				int vertexCount2 = triangleMeshNode2.GetVertexCount();
				for (int l = 0; l < vertexCount2; l++)
				{
					int vertexIndex = triangleMeshNode2.GetVertexIndex(l);
					int vertexIndex2 = triangleMeshNode2.GetVertexIndex((l + 1) % vertexCount2);
					int num;
					if (dictionary.TryGetValue(new Int2(vertexIndex2, vertexIndex), out num))
					{
						TriangleMeshNode triangleMeshNode3 = nodes[num];
						int vertexCount3 = triangleMeshNode3.GetVertexCount();
						for (int m = 0; m < vertexCount3; m++)
						{
							if (triangleMeshNode3.GetVertexIndex(m) == vertexIndex2 && triangleMeshNode3.GetVertexIndex((m + 1) % vertexCount3) == vertexIndex)
							{
								uint costMagnitude = (uint)(triangleMeshNode2.position - triangleMeshNode3.position).costMagnitude;
								list.Add(triangleMeshNode3);
								list2.Add(costMagnitude);
								break;
							}
						}
					}
				}
				triangleMeshNode2.connections = list.ToArray();
				triangleMeshNode2.connectionCosts = list2.ToArray();
			}
			dictionary.Clear();
			ObjectPoolSimple<Dictionary<Int2, int>>.Release(ref dictionary);
			ListPool<MeshNode>.Release(list);
			ListPool<uint>.Release(list2);
		}

		
		private void ConnectTiles(RecastGraph.NavmeshTile tile1, RecastGraph.NavmeshTile tile2)
		{
			if (tile1 == null || tile2 == null)
			{
				return;
			}
			if (tile1.nodes == null)
			{
				throw new ArgumentException("tile1 does not contain any nodes");
			}
			if (tile2.nodes == null)
			{
				throw new ArgumentException("tile2 does not contain any nodes");
			}
			int num = Mathf.Clamp(tile2.x, tile1.x, tile1.x + tile1.w - 1);
			int num2 = Mathf.Clamp(tile1.x, tile2.x, tile2.x + tile2.w - 1);
			int num3 = Mathf.Clamp(tile2.z, tile1.z, tile1.z + tile1.d - 1);
			int num4 = Mathf.Clamp(tile1.z, tile2.z, tile2.z + tile2.d - 1);
			int num5;
			int i;
			int num6;
			int num7;
			float num8;
			if (num == num2)
			{
				num5 = 2;
				i = 0;
				num6 = num3;
				num7 = num4;
				num8 = (float)this.tileSizeZ * this.cellSize;
			}
			else
			{
				if (num3 != num4)
				{
					throw new ArgumentException("Tiles are not adjacent (neither x or z coordinates match)");
				}
				num5 = 0;
				i = 2;
				num6 = num;
				num7 = num2;
				num8 = (float)this.tileSizeX * this.cellSize;
			}
			if (Math.Abs(num6 - num7) != 1)
			{
				Debug.Log(string.Concat(new object[]
				{
					tile1.x,
					" ",
					tile1.z,
					" ",
					tile1.w,
					" ",
					tile1.d,
					"\n",
					tile2.x,
					" ",
					tile2.z,
					" ",
					tile2.w,
					" ",
					tile2.d,
					"\n",
					num,
					" ",
					num3,
					" ",
					num2,
					" ",
					num4
				}));
				throw new ArgumentException(string.Concat(new object[]
				{
					"Tiles are not adjacent (tile coordinates must differ by exactly 1. Got '",
					num6,
					"' and '",
					num7,
					"')"
				}));
			}
			int num9 = (int)Math.Round((double)(((float)Math.Max(num6, num7) * num8 + this.forcedBounds.min[num5]) * 1000f));
			TriangleMeshNode[] nodes = tile1.nodes;
			TriangleMeshNode[] nodes2 = tile2.nodes;
			foreach (TriangleMeshNode triangleMeshNode in nodes)
			{
				int vertexCount = triangleMeshNode.GetVertexCount();
				for (int k = 0; k < vertexCount; k++)
				{
					Int3 vertex = triangleMeshNode.GetVertex(k);
					Int3 vertex2 = triangleMeshNode.GetVertex((k + 1) % vertexCount);
					if (Math.Abs(vertex[num5] - num9) < 2 && Math.Abs(vertex2[num5] - num9) < 2)
					{
						int num10 = Math.Min(vertex[i], vertex2[i]);
						int num11 = Math.Max(vertex[i], vertex2[i]);
						if (num10 != num11)
						{
							foreach (TriangleMeshNode triangleMeshNode2 in nodes2)
							{
								int vertexCount2 = triangleMeshNode2.GetVertexCount();
								for (int m = 0; m < vertexCount2; m++)
								{
									Int3 vertex3 = triangleMeshNode2.GetVertex(m);
									Int3 vertex4 = triangleMeshNode2.GetVertex((m + 1) % vertexCount);
									if (Math.Abs(vertex3[num5] - num9) < 2 && Math.Abs(vertex4[num5] - num9) < 2)
									{
										int num12 = Math.Min(vertex3[i], vertex4[i]);
										int num13 = Math.Max(vertex3[i], vertex4[i]);
										if (num12 != num13)
										{
											if (num11 > num12 && num10 < num13 && ((vertex == vertex3 && vertex2 == vertex4) || (vertex == vertex4 && vertex2 == vertex3) || VectorMath.SqrDistanceSegmentSegment((Vector3)vertex, (Vector3)vertex2, (Vector3)vertex3, (Vector3)vertex4) < this.walkableClimb * this.walkableClimb))
											{
												uint costMagnitude = (uint)(triangleMeshNode.position - triangleMeshNode2.position).costMagnitude;
												triangleMeshNode.AddConnection(triangleMeshNode2, costMagnitude);
												triangleMeshNode2.AddConnection(triangleMeshNode, costMagnitude);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		
		public void StartBatchTileUpdate()
		{
			if (this.batchTileUpdate)
			{
				throw new InvalidOperationException("Calling StartBatchLoad when batching is already enabled");
			}
			this.batchTileUpdate = true;
		}

		
		public void EndBatchTileUpdate()
		{
			if (!this.batchTileUpdate)
			{
				throw new InvalidOperationException("Calling EndBatchLoad when batching not enabled");
			}
			this.batchTileUpdate = false;
			int num = this.tileXCount;
			int num2 = this.tileZCount;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					this.tiles[j + i * this.tileXCount].flag = false;
				}
			}
			for (int k = 0; k < this.batchUpdatedTiles.Count; k++)
			{
				this.tiles[this.batchUpdatedTiles[k]].flag = true;
			}
			for (int l = 0; l < num2; l++)
			{
				for (int m = 0; m < num; m++)
				{
					if (m < num - 1 && (this.tiles[m + l * this.tileXCount].flag || this.tiles[m + 1 + l * this.tileXCount].flag) && this.tiles[m + l * this.tileXCount] != this.tiles[m + 1 + l * this.tileXCount])
					{
						this.ConnectTiles(this.tiles[m + l * this.tileXCount], this.tiles[m + 1 + l * this.tileXCount]);
					}
					if (l < num2 - 1 && (this.tiles[m + l * this.tileXCount].flag || this.tiles[m + (l + 1) * this.tileXCount].flag) && this.tiles[m + l * this.tileXCount] != this.tiles[m + (l + 1) * this.tileXCount])
					{
						this.ConnectTiles(this.tiles[m + l * this.tileXCount], this.tiles[m + (l + 1) * this.tileXCount]);
					}
				}
			}
			this.batchUpdatedTiles.Clear();
		}

		
		private void ClearTiles(int x, int z, int w, int d)
		{
			for (int i = z; i < z + d; i++)
			{
				for (int j = x; j < x + w; j++)
				{
					RecastGraph.NavmeshTile navmeshTile = this.tiles[j + i * this.tileXCount];
					if (navmeshTile != null)
					{
						this.RemoveConnectionsFromTile(navmeshTile);
						for (int k = 0; k < navmeshTile.nodes.Length; k++)
						{
							navmeshTile.nodes[k].Destroy();
						}
						for (int l = navmeshTile.z; l < navmeshTile.z + navmeshTile.d; l++)
						{
							for (int m = navmeshTile.x; m < navmeshTile.x + navmeshTile.w; m++)
							{
								RecastGraph.NavmeshTile navmeshTile2 = this.tiles[m + l * this.tileXCount];
								if (navmeshTile2 == null || navmeshTile2 != navmeshTile)
								{
									throw new Exception("This should not happen");
								}
								if (l < z || l >= z + d || m < x || m >= x + w)
								{
									this.tiles[m + l * this.tileXCount] = RecastGraph.NewEmptyTile(m, l);
									if (this.batchTileUpdate)
									{
										this.batchUpdatedTiles.Add(m + l * this.tileXCount);
									}
								}
								else
								{
									this.tiles[m + l * this.tileXCount] = null;
								}
							}
						}
						ObjectPool<BBTree>.Release(ref navmeshTile.bbTree);
					}
				}
			}
		}

		
		public void ReplaceTile(int x, int z, Int3[] verts, int[] tris, bool worldSpace)
		{
			this.ReplaceTile(x, z, 1, 1, verts, tris, worldSpace);
		}

		
		public void ReplaceTile(int x, int z, int w, int d, Int3[] verts, int[] tris, bool worldSpace)
		{
			if (x + w > this.tileXCount || z + d > this.tileZCount || x < 0 || z < 0)
			{
				throw new ArgumentException(string.Concat(new object[]
				{
					"Tile is placed at an out of bounds position or extends out of the graph bounds (",
					x,
					", ",
					z,
					" [",
					w,
					", ",
					d,
					"] ",
					this.tileXCount,
					" ",
					this.tileZCount,
					")"
				}));
			}
			if (w < 1 || d < 1)
			{
				throw new ArgumentException(string.Concat(new object[]
				{
					"width and depth must be greater or equal to 1. Was ",
					w,
					", ",
					d
				}));
			}
			this.ClearTiles(x, z, w, d);
			RecastGraph.NavmeshTile navmeshTile = new RecastGraph.NavmeshTile
			{
				x = x,
				z = z,
				w = w,
				d = d,
				tris = tris,
				verts = verts,
				bbTree = ObjectPool<BBTree>.Claim()
			};
			if (navmeshTile.tris.Length % 3 != 0)
			{
				throw new ArgumentException("Triangle array's length must be a multiple of 3 (tris)");
			}
			if (navmeshTile.verts.Length > 65535)
			{
				throw new ArgumentException("Too many vertices per tile (more than 65535)");
			}
			if (!worldSpace)
			{
				if (!Mathf.Approximately((float)(x * this.tileSizeX) * this.cellSize * 1000f, (float)Math.Round((double)((float)(x * this.tileSizeX) * this.cellSize * 1000f))))
				{
					Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
				}
				if (!Mathf.Approximately((float)(z * this.tileSizeZ) * this.cellSize * 1000f, (float)Math.Round((double)((float)(z * this.tileSizeZ) * this.cellSize * 1000f))))
				{
					Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
				}
				Int3 rhs = (Int3)(new Vector3((float)(x * this.tileSizeX) * this.cellSize, 0f, (float)(z * this.tileSizeZ) * this.cellSize) + this.forcedBounds.min);
				for (int i = 0; i < verts.Length; i++)
				{
					verts[i] += rhs;
				}
			}
			TriangleMeshNode[] array = new TriangleMeshNode[navmeshTile.tris.Length / 3];
			navmeshTile.nodes = array;
			int graphIndex = AstarPath.active.astarData.graphs.Length;
			TriangleMeshNode.SetNavmeshHolder(graphIndex, navmeshTile);
			int num = x + z * this.tileXCount;
			num <<= 12;
			if (navmeshTile.verts.Length > 4095)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Too many vertices in the tile (",
					navmeshTile.verts.Length,
					" > ",
					4095,
					")\nYou can enable ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector to raise this limit."
				}));
				this.tiles[num] = RecastGraph.NewEmptyTile(x, z);
				return;
			}
			for (int j = 0; j < array.Length; j++)
			{
				TriangleMeshNode triangleMeshNode = new TriangleMeshNode(this.active);
				array[j] = triangleMeshNode;
				triangleMeshNode.GraphIndex = (uint)graphIndex;
				triangleMeshNode.v0 = (navmeshTile.tris[j * 3] | num);
				triangleMeshNode.v1 = (navmeshTile.tris[j * 3 + 1] | num);
				triangleMeshNode.v2 = (navmeshTile.tris[j * 3 + 2] | num);
				if (!VectorMath.IsClockwiseXZ(triangleMeshNode.GetVertex(0), triangleMeshNode.GetVertex(1), triangleMeshNode.GetVertex(2)))
				{
					int v = triangleMeshNode.v0;
					triangleMeshNode.v0 = triangleMeshNode.v2;
					triangleMeshNode.v2 = v;
				}
				triangleMeshNode.Walkable = true;
				triangleMeshNode.Penalty = this.initialPenalty;
				triangleMeshNode.UpdatePositionFromVertices();
			}
			navmeshTile.bbTree.RebuildFrom(array);
			this.CreateNodeConnections(navmeshTile.nodes);
			for (int k = z; k < z + d; k++)
			{
				for (int l = x; l < x + w; l++)
				{
					this.tiles[l + k * this.tileXCount] = navmeshTile;
				}
			}
			if (this.batchTileUpdate)
			{
				this.batchUpdatedTiles.Add(x + z * this.tileXCount);
			}
			else
			{
				this.ConnectTileWithNeighbours(navmeshTile, false);
			}
			TriangleMeshNode.SetNavmeshHolder(graphIndex, null);
			graphIndex = AstarPath.active.astarData.GetGraphIndex(this);
			for (int m = 0; m < array.Length; m++)
			{
				array[m].GraphIndex = (uint)graphIndex;
			}
		}

		
		public bool Linecast(Vector3 origin, Vector3 end)
		{
			return this.Linecast(origin, end, base.GetNearest(origin, NNConstraint.None).node);
		}

		
		public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit)
		{
			return NavMeshGraph.Linecast(this, origin, end, hint, out hit, null);
		}

		
		public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint)
		{
			GraphHitInfo graphHitInfo;
			return NavMeshGraph.Linecast(this, origin, end, hint, out graphHitInfo, null);
		}

		
		public bool Linecast(Vector3 tmp_origin, Vector3 tmp_end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
		{
			return NavMeshGraph.Linecast(this, tmp_origin, tmp_end, hint, out hit, trace);
		}

		
		public override void OnDrawGizmos(bool drawNodes)
		{
			if (!drawNodes)
			{
				return;
			}
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(this.forcedBounds.center, this.forcedBounds.size);
			PathHandler debugData = AstarPath.active.debugPathData;
			GraphNodeDelegateCancelable del = delegate(GraphNode _node)
			{
				TriangleMeshNode triangleMeshNode = _node as TriangleMeshNode;
				if (AstarPath.active.showSearchTree && debugData != null)
				{
					bool flag = NavGraph.InSearchTree(triangleMeshNode, AstarPath.active.debugPath);
					if (flag && this.showNodeConnections)
					{
						PathNode pathNode = debugData.GetPathNode(triangleMeshNode);
						if (pathNode.parent != null)
						{
							Gizmos.color = this.NodeColor(triangleMeshNode, debugData);
							Gizmos.DrawLine((Vector3)triangleMeshNode.position, (Vector3)debugData.GetPathNode(triangleMeshNode).parent.node.position);
						}
					}
					if (this.showMeshOutline)
					{
						Gizmos.color = ((!triangleMeshNode.Walkable) ? AstarColor.UnwalkableNode : this.NodeColor(triangleMeshNode, debugData));
						if (!flag)
						{
							Gizmos.color *= new Color(1f, 1f, 1f, 0.1f);
						}
						Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(0), (Vector3)triangleMeshNode.GetVertex(1));
						Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(1), (Vector3)triangleMeshNode.GetVertex(2));
						Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(2), (Vector3)triangleMeshNode.GetVertex(0));
					}
				}
				else
				{
					if (this.showNodeConnections)
					{
						Gizmos.color = this.NodeColor(triangleMeshNode, null);
						for (int i = 0; i < triangleMeshNode.connections.Length; i++)
						{
							Gizmos.DrawLine((Vector3)triangleMeshNode.position, Vector3.Lerp((Vector3)triangleMeshNode.connections[i].position, (Vector3)triangleMeshNode.position, 0.4f));
						}
					}
					if (this.showMeshOutline)
					{
						Gizmos.color = ((!triangleMeshNode.Walkable) ? AstarColor.UnwalkableNode : this.NodeColor(triangleMeshNode, debugData));
						Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(0), (Vector3)triangleMeshNode.GetVertex(1));
						Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(1), (Vector3)triangleMeshNode.GetVertex(2));
						Gizmos.DrawLine((Vector3)triangleMeshNode.GetVertex(2), (Vector3)triangleMeshNode.GetVertex(0));
					}
				}
				return true;
			};
			this.GetNodes(del);
		}

		
		public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
		{
			base.DeserializeSettingsCompatibility(ctx);
			this.characterRadius = ctx.reader.ReadSingle();
			this.contourMaxError = ctx.reader.ReadSingle();
			this.cellSize = ctx.reader.ReadSingle();
			ctx.reader.ReadSingle();
			this.walkableHeight = ctx.reader.ReadSingle();
			this.maxSlope = ctx.reader.ReadSingle();
			this.maxEdgeLength = ctx.reader.ReadSingle();
			this.editorTileSize = ctx.reader.ReadInt32();
			this.tileSizeX = ctx.reader.ReadInt32();
			this.nearestSearchOnlyXZ = ctx.reader.ReadBoolean();
			this.useTiles = ctx.reader.ReadBoolean();
			this.relevantGraphSurfaceMode = (RecastGraph.RelevantGraphSurfaceMode)ctx.reader.ReadInt32();
			this.rasterizeColliders = ctx.reader.ReadBoolean();
			this.rasterizeMeshes = ctx.reader.ReadBoolean();
			this.rasterizeTerrain = ctx.reader.ReadBoolean();
			this.rasterizeTrees = ctx.reader.ReadBoolean();
			this.colliderRasterizeDetail = ctx.reader.ReadSingle();
			this.forcedBoundsCenter = ctx.DeserializeVector3();
			this.forcedBoundsSize = ctx.DeserializeVector3();
			this.mask = ctx.reader.ReadInt32();
			int num = ctx.reader.ReadInt32();
			this.tagMask = new List<string>(num);
			for (int i = 0; i < num; i++)
			{
				this.tagMask.Add(ctx.reader.ReadString());
			}
			this.showMeshOutline = ctx.reader.ReadBoolean();
			this.showNodeConnections = ctx.reader.ReadBoolean();
			this.terrainSampleSize = ctx.reader.ReadInt32();
			this.walkableClimb = ctx.DeserializeFloat(this.walkableClimb);
			this.minRegionSize = ctx.DeserializeFloat(this.minRegionSize);
			this.tileSizeZ = ctx.DeserializeInt(this.tileSizeX);
			this.showMeshSurface = ctx.reader.ReadBoolean();
		}

		
		public override void SerializeExtraInfo(GraphSerializationContext ctx)
		{
			BinaryWriter writer = ctx.writer;
			if (this.tiles == null)
			{
				writer.Write(-1);
				return;
			}
			writer.Write(this.tileXCount);
			writer.Write(this.tileZCount);
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					RecastGraph.NavmeshTile navmeshTile = this.tiles[j + i * this.tileXCount];
					if (navmeshTile == null)
					{
						throw new Exception("NULL Tile");
					}
					writer.Write(navmeshTile.x);
					writer.Write(navmeshTile.z);
					if (navmeshTile.x == j && navmeshTile.z == i)
					{
						writer.Write(navmeshTile.w);
						writer.Write(navmeshTile.d);
						writer.Write(navmeshTile.tris.Length);
						for (int k = 0; k < navmeshTile.tris.Length; k++)
						{
							writer.Write(navmeshTile.tris[k]);
						}
						writer.Write(navmeshTile.verts.Length);
						for (int l = 0; l < navmeshTile.verts.Length; l++)
						{
							ctx.SerializeInt3(navmeshTile.verts[l]);
						}
						writer.Write(navmeshTile.nodes.Length);
						for (int m = 0; m < navmeshTile.nodes.Length; m++)
						{
							navmeshTile.nodes[m].SerializeNode(ctx);
						}
					}
				}
			}
		}

		
		public override void DeserializeExtraInfo(GraphSerializationContext ctx)
		{
			BinaryReader reader = ctx.reader;
			this.tileXCount = reader.ReadInt32();
			if (this.tileXCount < 0)
			{
				return;
			}
			this.tileZCount = reader.ReadInt32();
			this.tiles = new RecastGraph.NavmeshTile[this.tileXCount * this.tileZCount];
			TriangleMeshNode.SetNavmeshHolder((int)ctx.graphIndex, this);
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					int num = j + i * this.tileXCount;
					int num2 = reader.ReadInt32();
					if (num2 < 0)
					{
						throw new Exception("Invalid tile coordinates (x < 0)");
					}
					int num3 = reader.ReadInt32();
					if (num3 < 0)
					{
						throw new Exception("Invalid tile coordinates (z < 0)");
					}
					if (num2 != j || num3 != i)
					{
						this.tiles[num] = this.tiles[num3 * this.tileXCount + num2];
					}
					else
					{
						RecastGraph.NavmeshTile navmeshTile = new RecastGraph.NavmeshTile();
						navmeshTile.x = num2;
						navmeshTile.z = num3;
						navmeshTile.w = reader.ReadInt32();
						navmeshTile.d = reader.ReadInt32();
						navmeshTile.bbTree = ObjectPool<BBTree>.Claim();
						this.tiles[num] = navmeshTile;
						int num4 = reader.ReadInt32();
						if (num4 % 3 != 0)
						{
							throw new Exception("Corrupt data. Triangle indices count must be divisable by 3. Got " + num4);
						}
						navmeshTile.tris = new int[num4];
						for (int k = 0; k < navmeshTile.tris.Length; k++)
						{
							navmeshTile.tris[k] = reader.ReadInt32();
						}
						navmeshTile.verts = new Int3[reader.ReadInt32()];
						for (int l = 0; l < navmeshTile.verts.Length; l++)
						{
							navmeshTile.verts[l] = ctx.DeserializeInt3();
						}
						int num5 = reader.ReadInt32();
						navmeshTile.nodes = new TriangleMeshNode[num5];
						num <<= 12;
						for (int m = 0; m < navmeshTile.nodes.Length; m++)
						{
							TriangleMeshNode triangleMeshNode = new TriangleMeshNode(this.active);
							navmeshTile.nodes[m] = triangleMeshNode;
							triangleMeshNode.DeserializeNode(ctx);
							triangleMeshNode.v0 = (navmeshTile.tris[m * 3] | num);
							triangleMeshNode.v1 = (navmeshTile.tris[m * 3 + 1] | num);
							triangleMeshNode.v2 = (navmeshTile.tris[m * 3 + 2] | num);
							triangleMeshNode.UpdatePositionFromVertices();
						}
						navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
					}
				}
			}
		}

		
		public bool dynamic = true;

		
		[JsonMember]
		public float characterRadius = 1.5f;

		
		[JsonMember]
		public float contourMaxError = 2f;

		
		[JsonMember]
		public float cellSize = 0.5f;

		
		[JsonMember]
		public float walkableHeight = 2f;

		
		[JsonMember]
		public float walkableClimb = 0.5f;

		
		[JsonMember]
		public float maxSlope = 30f;

		
		[JsonMember]
		public float maxEdgeLength = 20f;

		
		[JsonMember]
		public float minRegionSize = 3f;

		
		[JsonMember]
		public int editorTileSize = 128;

		
		[JsonMember]
		public int tileSizeX = 128;

		
		[JsonMember]
		public int tileSizeZ = 128;

		
		[JsonMember]
		public bool nearestSearchOnlyXZ;

		
		[JsonMember]
		public bool useTiles;

		
		public bool scanEmptyGraph;

		
		[JsonMember]
		public RecastGraph.RelevantGraphSurfaceMode relevantGraphSurfaceMode;

		
		[JsonMember]
		public bool rasterizeColliders;

		
		[JsonMember]
		public bool rasterizeMeshes = true;

		
		[JsonMember]
		public bool rasterizeTerrain = true;

		
		[JsonMember]
		public bool rasterizeTrees = true;

		
		[JsonMember]
		public float colliderRasterizeDetail = 10f;

		
		[JsonMember]
		public Vector3 forcedBoundsCenter;

		
		[JsonMember]
		public Vector3 forcedBoundsSize = new Vector3(100f, 40f, 100f);

		
		[JsonMember]
		public LayerMask mask = -1;

		
		[JsonMember]
		public List<string> tagMask = new List<string>();

		
		[JsonMember]
		public bool showMeshOutline = true;

		
		[JsonMember]
		public bool showNodeConnections;

		
		[JsonMember]
		public bool showMeshSurface;

		
		[JsonMember]
		public int terrainSampleSize = 3;

		
		private Voxelize globalVox;

		
		public int tileXCount;

		
		public int tileZCount;

		
		private RecastGraph.NavmeshTile[] tiles;

		
		private bool batchTileUpdate;

		
		private List<int> batchUpdatedTiles = new List<int>();

		
		private List<RecastGraph.NavmeshTile> stagingTiles = new List<RecastGraph.NavmeshTile>();

		
		public const int VertexIndexMask = 4095;

		
		public const int TileIndexMask = 524287;

		
		public const int TileIndexOffset = 12;

		
		public const int BorderVertexMask = 1;

		
		public const int BorderVertexOffset = 31;

		
		public enum RelevantGraphSurfaceMode
		{
			
			DoNotRequire,
			
			OnlyForCompletelyInsideTile,
			
			RequireForAll
		}

		
		public class NavmeshTile : INavmeshHolder, INavmesh
		{
			
			public void GetTileCoordinates(int tileIndex, out int x, out int z)
			{
				x = this.x;
				z = this.z;
			}

			
			public int GetVertexArrayIndex(int index)
			{
				return index & 4095;
			}

			
			public Int3 GetVertex(int index)
			{
				int num = index & 4095;
				return this.verts[num];
			}

			
			public void GetNodes(GraphNodeDelegateCancelable del)
			{
				if (this.nodes == null)
				{
					return;
				}
				int num = 0;
				while (num < this.nodes.Length && del(this.nodes[num]))
				{
					num++;
				}
			}

			
			public int[] tris;

			
			public Int3[] verts;

			
			public int x;

			
			public int z;

			
			public int w;

			
			public int d;

			
			public TriangleMeshNode[] nodes;

			
			public BBTree bbTree;

			
			public bool flag;
		}
	}
}
