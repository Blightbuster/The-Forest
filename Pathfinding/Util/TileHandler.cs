using System;
using System.Collections.Generic;
using Pathfinding.ClipperLib;
using Pathfinding.Poly2Tri;
using Pathfinding.Voxels;
using UnityEngine;

namespace Pathfinding.Util
{
	
	public class TileHandler
	{
		
		public TileHandler(RecastGraph graph)
		{
			if (graph == null)
			{
				throw new ArgumentNullException("graph");
			}
			if (graph.GetTiles() == null)
			{
				Debug.LogWarning("Creating a TileHandler for a graph with no tiles. Please scan the graph before creating a TileHandler");
			}
			this.tileXCount = graph.tileXCount;
			this.tileZCount = graph.tileZCount;
			this.activeTileTypes = new TileHandler.TileType[this.tileXCount * this.tileZCount];
			this.activeTileRotations = new int[this.activeTileTypes.Length];
			this.activeTileOffsets = new int[this.activeTileTypes.Length];
			this.reloadedInBatch = new bool[this.activeTileTypes.Length];
			this._graph = graph;
		}

		
		
		public RecastGraph graph
		{
			get
			{
				return this._graph;
			}
		}

		
		
		public bool isValid
		{
			get
			{
				return this._graph != null && this.tileXCount == this._graph.tileXCount && this.tileZCount == this._graph.tileZCount;
			}
		}

		
		public int GetActiveRotation(Int2 p)
		{
			return this.activeTileRotations[p.x + p.y * this._graph.tileXCount];
		}

		
		public TileHandler.TileType GetTileType(int index)
		{
			return this.tileTypes[index];
		}

		
		public int GetTileTypeCount()
		{
			return this.tileTypes.Count;
		}

		
		public TileHandler.TileType RegisterTileType(Mesh source, Int3 centerOffset, int width = 1, int depth = 1)
		{
			TileHandler.TileType tileType = new TileHandler.TileType(source, new Int3(this.graph.tileSizeX, 1, this.graph.tileSizeZ) * (1000f * this.graph.cellSize), centerOffset, width, depth);
			this.tileTypes.Add(tileType);
			return tileType;
		}

		
		public void CreateTileTypesFromGraph()
		{
			RecastGraph.NavmeshTile[] tiles = this.graph.GetTiles();
			if (tiles == null)
			{
				return;
			}
			if (!this.isValid)
			{
				throw new InvalidOperationException("Graph tiles are invalid (number of tiles is not equal to width*depth of the graph). You need to create a new tile handler if you have changed the graph.");
			}
			for (int i = 0; i < this.graph.tileZCount; i++)
			{
				for (int j = 0; j < this.graph.tileXCount; j++)
				{
					RecastGraph.NavmeshTile navmeshTile = tiles[j + i * this.graph.tileXCount];
					Int3 @int = (Int3)this.graph.GetTileBounds(j, i, 1, 1).min;
					Int3 tileSize = new Int3(this.graph.tileSizeX, 1, this.graph.tileSizeZ) * (1000f * this.graph.cellSize);
					@int += new Int3(tileSize.x * navmeshTile.w / 2, 0, tileSize.z * navmeshTile.d / 2);
					@int = -@int;
					TileHandler.TileType tileType = new TileHandler.TileType(navmeshTile.verts, navmeshTile.tris, tileSize, @int, navmeshTile.w, navmeshTile.d);
					this.tileTypes.Add(tileType);
					int num = j + i * this.graph.tileXCount;
					this.activeTileTypes[num] = tileType;
					this.activeTileRotations[num] = 0;
					this.activeTileOffsets[num] = 0;
				}
			}
		}

		
		public bool StartBatchLoad()
		{
			if (this.isBatching)
			{
				return false;
			}
			this.isBatching = true;
			AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(bool force)
			{
				this.graph.StartBatchTileUpdate();
				return true;
			}));
			return true;
		}

		
		public void EndBatchLoad()
		{
			if (!this.isBatching)
			{
				throw new Exception("Ending batching when batching has not been started");
			}
			for (int i = 0; i < this.reloadedInBatch.Length; i++)
			{
				this.reloadedInBatch[i] = false;
			}
			this.isBatching = false;
			AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(bool force)
			{
				this.graph.EndBatchTileUpdate();
				return true;
			}));
		}

		
		private void CutPoly(Int3[] verts, int[] tris, ref Int3[] outVertsArr, ref int[] outTrisArr, out int outVCount, out int outTCount, Int3[] extraShape, Int3 cuttingOffset, Bounds realBounds, TileHandler.CutMode mode = TileHandler.CutMode.CutAll | TileHandler.CutMode.CutDual, int perturbate = -1)
		{
			if (verts.Length == 0 || tris.Length == 0)
			{
				outVCount = 0;
				outTCount = 0;
				outTrisArr = new int[0];
				outVertsArr = new Int3[0];
				return;
			}
			if (perturbate > 10)
			{
				Debug.LogError("Too many perturbations aborting.\nThis may cause a tile in the navmesh to become empty. Try to see see if any of your NavmeshCut or NavmeshAdd components use invalid custom meshes.");
				outVCount = verts.Length;
				outTCount = tris.Length;
				outTrisArr = tris;
				outVertsArr = verts;
				return;
			}
			List<IntPoint> list = null;
			if (extraShape == null && (mode & TileHandler.CutMode.CutExtra) != (TileHandler.CutMode)0)
			{
				throw new Exception("extraShape is null and the CutMode specifies that it should be used. Cannot use null shape.");
			}
			if ((mode & TileHandler.CutMode.CutExtra) != (TileHandler.CutMode)0)
			{
				list = ListPool<IntPoint>.Claim(extraShape.Length);
				for (int i = 0; i < extraShape.Length; i++)
				{
					list.Add(new IntPoint((long)(extraShape[i].x + cuttingOffset.x), (long)(extraShape[i].z + cuttingOffset.z)));
				}
			}
			IntRect bounds = new IntRect(verts[0].x, verts[0].z, verts[0].x, verts[0].z);
			for (int j = 0; j < verts.Length; j++)
			{
				bounds = bounds.ExpandToContain(verts[j].x, verts[j].z);
			}
			List<NavmeshCut> list2;
			if (mode == TileHandler.CutMode.CutExtra)
			{
				list2 = ListPool<NavmeshCut>.Claim();
			}
			else
			{
				list2 = NavmeshCut.GetAllInRange(realBounds);
			}
			List<NavmeshAdd> allInRange = NavmeshAdd.GetAllInRange(realBounds);
			List<int> list3 = ListPool<int>.Claim();
			List<TileHandler.Cut> list4 = TileHandler.PrepareNavmeshCutsForCutting(list2, cuttingOffset, bounds, perturbate, allInRange.Count > 0);
			List<Int3> list5 = ListPool<Int3>.Claim(verts.Length * 2);
			List<int> list6 = ListPool<int>.Claim(tris.Length);
			Int3[] array = verts;
			int[] array2 = tris;
			if (list2.Count == 0 && allInRange.Count == 0 && (mode & ~(TileHandler.CutMode.CutAll | TileHandler.CutMode.CutDual)) == (TileHandler.CutMode)0 && (mode & TileHandler.CutMode.CutAll) != (TileHandler.CutMode)0)
			{
				this.CopyMesh(array, array2, list5, list6);
			}
			else
			{
				List<IntPoint> list7 = ListPool<IntPoint>.Claim();
				Dictionary<TriangulationPoint, int> dictionary = new Dictionary<TriangulationPoint, int>();
				List<PolygonPoint> list8 = ListPool<PolygonPoint>.Claim();
				PolyTree polyTree = new PolyTree();
				List<List<IntPoint>> intermediateResult = new List<List<IntPoint>>();
				Stack<Polygon> stack = new Stack<Polygon>();
				this.clipper.StrictlySimple = (perturbate > -1);
				this.clipper.ReverseSolution = true;
				Int3[] array3 = null;
				Int3[] clipOut = null;
				Int2 @int = default(Int2);
				if (allInRange.Count > 0)
				{
					array3 = new Int3[7];
					clipOut = new Int3[7];
					@int = new Int2(((Int3)realBounds.extents).x, ((Int3)realBounds.extents).z);
				}
				int num = -1;
				int k = -3;
				for (;;)
				{
					k += 3;
					while (k >= array2.Length)
					{
						num++;
						k = 0;
						if (num >= allInRange.Count)
						{
							array = null;
							break;
						}
						if (array == verts)
						{
							array = null;
						}
						allInRange[num].GetMesh(cuttingOffset, ref array, out array2);
					}
					if (array == null)
					{
						break;
					}
					Int3 int2 = array[array2[k]];
					Int3 int3 = array[array2[k + 1]];
					Int3 int4 = array[array2[k + 2]];
					if (VectorMath.IsColinearXZ(int2, int3, int4))
					{
						Debug.LogWarning("Skipping degenerate triangle.");
					}
					else
					{
						IntRect a = new IntRect(int2.x, int2.z, int2.x, int2.z);
						a = a.ExpandToContain(int3.x, int3.z);
						a = a.ExpandToContain(int4.x, int4.z);
						int num2 = Math.Min(int2.y, Math.Min(int3.y, int4.y));
						int num3 = Math.Max(int2.y, Math.Max(int3.y, int4.y));
						list3.Clear();
						bool flag = false;
						for (int l = 0; l < list4.Count; l++)
						{
							int x = list4[l].boundsY.x;
							int y = list4[l].boundsY.y;
							if (IntRect.Intersects(a, list4[l].bounds) && y >= num2 && x <= num3 && (list4[l].cutsAddedGeom || num == -1))
							{
								Int3 int5 = int2;
								int5.y = x;
								Int3 int6 = int2;
								int6.y = y;
								list3.Add(l);
								flag |= list4[l].isDual;
							}
						}
						if (list3.Count == 0 && (mode & TileHandler.CutMode.CutExtra) == (TileHandler.CutMode)0 && (mode & TileHandler.CutMode.CutAll) != (TileHandler.CutMode)0 && num == -1)
						{
							list6.Add(list5.Count);
							list6.Add(list5.Count + 1);
							list6.Add(list5.Count + 2);
							list5.Add(int2);
							list5.Add(int3);
							list5.Add(int4);
						}
						else
						{
							list7.Clear();
							if (num == -1)
							{
								list7.Add(new IntPoint((long)int2.x, (long)int2.z));
								list7.Add(new IntPoint((long)int3.x, (long)int3.z));
								list7.Add(new IntPoint((long)int4.x, (long)int4.z));
							}
							else
							{
								array3[0] = int2;
								array3[1] = int3;
								array3[2] = int4;
								int num4 = this.ClipAgainstRectangle(array3, clipOut, new Int2(@int.x * 2, @int.y * 2));
								if (num4 == 0)
								{
									continue;
								}
								for (int m = 0; m < num4; m++)
								{
									list7.Add(new IntPoint((long)array3[m].x, (long)array3[m].z));
								}
							}
							dictionary.Clear();
							for (int n = 0; n < 16; n++)
							{
								if ((mode >> (n & 31) & TileHandler.CutMode.CutAll) != (TileHandler.CutMode)0)
								{
									if (1 << n == 1)
									{
										this.CutAll(list7, list3, list4, polyTree);
									}
									else if (1 << n == 2)
									{
										if (!flag)
										{
											goto IL_9AA;
										}
										this.CutDual(list7, list3, list4, flag, intermediateResult, polyTree);
									}
									else if (1 << n == 4)
									{
										this.CutExtra(list7, list, polyTree);
									}
									for (int num5 = 0; num5 < polyTree.ChildCount; num5++)
									{
										PolyNode polyNode = polyTree.Childs[num5];
										List<IntPoint> contour = polyNode.Contour;
										List<PolyNode> childs = polyNode.Childs;
										if (childs.Count == 0 && contour.Count == 3 && num == -1)
										{
											for (int num6 = 0; num6 < 3; num6++)
											{
												Int3 int7 = new Int3((int)contour[num6].X, 0, (int)contour[num6].Y);
												int7.y = TileHandler.SampleYCoordinateInTriangle(int2, int3, int4, int7);
												list6.Add(list5.Count);
												list5.Add(int7);
											}
										}
										else
										{
											Polygon polygon = null;
											int num7 = -1;
											for (List<IntPoint> list9 = contour; list9 != null; list9 = ((num7 >= childs.Count) ? null : childs[num7].Contour))
											{
												list8.Clear();
												for (int num8 = 0; num8 < list9.Count; num8++)
												{
													PolygonPoint polygonPoint = new PolygonPoint((double)list9[num8].X, (double)list9[num8].Y);
													list8.Add(polygonPoint);
													Int3 int8 = new Int3((int)list9[num8].X, 0, (int)list9[num8].Y);
													int8.y = TileHandler.SampleYCoordinateInTriangle(int2, int3, int4, int8);
													dictionary[polygonPoint] = list5.Count;
													list5.Add(int8);
												}
												Polygon polygon2;
												if (stack.Count > 0)
												{
													polygon2 = stack.Pop();
													polygon2.AddPoints(list8);
												}
												else
												{
													polygon2 = new Polygon(list8);
												}
												if (num7 == -1)
												{
													polygon = polygon2;
												}
												else
												{
													polygon.AddHole(polygon2);
												}
												num7++;
											}
											try
											{
												P2T.Triangulate(polygon);
											}
											catch (PointOnEdgeException)
											{
												Debug.LogWarning("PointOnEdgeException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
												this.CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
												return;
											}
											try
											{
												for (int num9 = 0; num9 < polygon.Triangles.Count; num9++)
												{
													DelaunayTriangle delaunayTriangle = polygon.Triangles[num9];
													list6.Add(dictionary[delaunayTriangle.Points._0]);
													list6.Add(dictionary[delaunayTriangle.Points._1]);
													list6.Add(dictionary[delaunayTriangle.Points._2]);
												}
											}
											catch (KeyNotFoundException)
											{
												Debug.LogWarning("KeyNotFoundException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
												this.CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
												return;
											}
											TileHandler.PoolPolygon(polygon, stack);
										}
									}
								}
								IL_9AA:;
							}
						}
					}
				}
				ListPool<IntPoint>.Release(list7);
				ListPool<PolygonPoint>.Release(list8);
			}
			this.CompressMesh(list5, list6, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount);
			for (int num10 = 0; num10 < list2.Count; num10++)
			{
				list2[num10].UsedForCut();
			}
			ListPool<Int3>.Release(list5);
			ListPool<int>.Release(list6);
			ListPool<int>.Release(list3);
			for (int num11 = 0; num11 < list4.Count; num11++)
			{
				ListPool<IntPoint>.Release(list4[num11].contour);
			}
			ListPool<TileHandler.Cut>.Release(list4);
			ListPool<NavmeshCut>.Release(list2);
		}

		
		private static List<TileHandler.Cut> PrepareNavmeshCutsForCutting(List<NavmeshCut> navmeshCuts, Int3 cuttingOffset, IntRect bounds, int perturbate, bool anyNavmeshAdds)
		{
			System.Random random = null;
			if (perturbate > 0)
			{
				random = new System.Random();
			}
			List<List<IntPoint>> list = ListPool<List<IntPoint>>.Claim();
			List<TileHandler.Cut> list2 = ListPool<TileHandler.Cut>.Claim();
			for (int i = 0; i < navmeshCuts.Count; i++)
			{
				Bounds bounds2 = navmeshCuts[i].GetBounds();
				Int3 @int = (Int3)bounds2.min + cuttingOffset;
				Int3 int2 = (Int3)bounds2.max + cuttingOffset;
				IntRect a = new IntRect(@int.x, @int.z, int2.x, int2.z);
				if (IntRect.Intersects(a, bounds) || anyNavmeshAdds)
				{
					Int2 int3 = new Int2(0, 0);
					if (perturbate > 0)
					{
						int3.x = random.Next() % 6 * perturbate - 3 * perturbate;
						if (int3.x >= 0)
						{
							int3.x++;
						}
						int3.y = random.Next() % 6 * perturbate - 3 * perturbate;
						if (int3.y >= 0)
						{
							int3.y++;
						}
					}
					list.Clear();
					navmeshCuts[i].GetContour(list);
					for (int j = 0; j < list.Count; j++)
					{
						List<IntPoint> list3 = list[j];
						if (list3.Count == 0)
						{
							Debug.LogError("Zero Length Contour");
						}
						else
						{
							IntRect bounds3 = new IntRect((int)list3[0].X + cuttingOffset.x, (int)list3[0].Y + cuttingOffset.z, (int)list3[0].X + cuttingOffset.x, (int)list3[0].Y + cuttingOffset.z);
							for (int k = 0; k < list3.Count; k++)
							{
								IntPoint intPoint = list3[k];
								intPoint.X += (long)cuttingOffset.x;
								intPoint.Y += (long)cuttingOffset.z;
								if (perturbate > 0)
								{
									intPoint.X += (long)int3.x;
									intPoint.Y += (long)int3.y;
								}
								bounds3 = bounds3.ExpandToContain((int)intPoint.X, (int)intPoint.Y);
								list3[k] = new IntPoint(intPoint.X, intPoint.Y);
							}
							list2.Add(new TileHandler.Cut
							{
								boundsY = new Int2(@int.y, int2.y),
								bounds = bounds3,
								isDual = navmeshCuts[i].isDual,
								cutsAddedGeom = navmeshCuts[i].cutsAddedGeom,
								contour = list3
							});
						}
					}
				}
			}
			ListPool<List<IntPoint>>.Release(list);
			return list2;
		}

		
		private static void PoolPolygon(Polygon polygon, Stack<Polygon> pool)
		{
			if (polygon.Holes != null)
			{
				for (int i = 0; i < polygon.Holes.Count; i++)
				{
					polygon.Holes[i].Points.Clear();
					polygon.Holes[i].ClearTriangles();
					if (polygon.Holes[i].Holes != null)
					{
						polygon.Holes[i].Holes.Clear();
					}
					pool.Push(polygon.Holes[i]);
				}
			}
			polygon.ClearTriangles();
			if (polygon.Holes != null)
			{
				polygon.Holes.Clear();
			}
			polygon.Points.Clear();
			pool.Push(polygon);
		}

		
		private static int SampleYCoordinateInTriangle(Int3 p1, Int3 p2, Int3 p3, Int3 p)
		{
			double num = (double)(p2.z - p3.z) * (double)(p1.x - p3.x) + (double)(p3.x - p2.x) * (double)(p1.z - p3.z);
			double num2 = ((double)(p2.z - p3.z) * (double)(p.x - p3.x) + (double)(p3.x - p2.x) * (double)(p.z - p3.z)) / num;
			double num3 = ((double)(p3.z - p1.z) * (double)(p.x - p3.x) + (double)(p1.x - p3.x) * (double)(p.z - p3.z)) / num;
			return (int)Math.Round(num2 * (double)p1.y + num3 * (double)p2.y + (1.0 - num2 - num3) * (double)p3.y);
		}

		
		private void CutAll(List<IntPoint> poly, List<int> intersectingCutIndices, List<TileHandler.Cut> cuts, PolyTree result)
		{
			this.clipper.Clear();
			this.clipper.AddPolygon(poly, PolyType.ptSubject);
			for (int i = 0; i < intersectingCutIndices.Count; i++)
			{
				this.clipper.AddPolygon(cuts[intersectingCutIndices[i]].contour, PolyType.ptClip);
			}
			result.Clear();
			this.clipper.Execute(ClipType.ctDifference, result, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
		}

		
		private void CutDual(List<IntPoint> poly, List<int> tmpIntersectingCuts, List<TileHandler.Cut> cuts, bool hasDual, List<List<IntPoint>> intermediateResult, PolyTree result)
		{
			this.clipper.Clear();
			this.clipper.AddPolygon(poly, PolyType.ptSubject);
			for (int i = 0; i < tmpIntersectingCuts.Count; i++)
			{
				if (cuts[tmpIntersectingCuts[i]].isDual)
				{
					this.clipper.AddPolygon(cuts[tmpIntersectingCuts[i]].contour, PolyType.ptClip);
				}
			}
			this.clipper.Execute(ClipType.ctIntersection, intermediateResult, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
			this.clipper.Clear();
			if (intermediateResult != null)
			{
				for (int j = 0; j < intermediateResult.Count; j++)
				{
					this.clipper.AddPolygon(intermediateResult[j], (!Clipper.Orientation(intermediateResult[j])) ? PolyType.ptSubject : PolyType.ptClip);
				}
			}
			for (int k = 0; k < tmpIntersectingCuts.Count; k++)
			{
				if (!cuts[tmpIntersectingCuts[k]].isDual)
				{
					this.clipper.AddPolygon(cuts[tmpIntersectingCuts[k]].contour, PolyType.ptClip);
				}
			}
			result.Clear();
			this.clipper.Execute(ClipType.ctDifference, result, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
		}

		
		private void CutExtra(List<IntPoint> poly, List<IntPoint> extraClipShape, PolyTree result)
		{
			this.clipper.Clear();
			this.clipper.AddPolygon(poly, PolyType.ptSubject);
			this.clipper.AddPolygon(extraClipShape, PolyType.ptClip);
			result.Clear();
			this.clipper.Execute(ClipType.ctIntersection, result, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
		}

		
		private int ClipAgainstRectangle(Int3[] clipIn, Int3[] clipOut, Int2 size)
		{
			int num = this.simpleClipper.ClipPolygon(clipIn, 3, clipOut, 1, 0, 0);
			if (num == 0)
			{
				return num;
			}
			num = this.simpleClipper.ClipPolygon(clipOut, num, clipIn, -1, size.x, 0);
			if (num == 0)
			{
				return num;
			}
			num = this.simpleClipper.ClipPolygon(clipIn, num, clipOut, 1, 0, 2);
			if (num == 0)
			{
				return num;
			}
			return this.simpleClipper.ClipPolygon(clipOut, num, clipIn, -1, size.y, 2);
		}

		
		private void CopyMesh(Int3[] vertices, int[] triangles, List<Int3> outVertices, List<int> outTriangles)
		{
			outTriangles.Capacity = Math.Max(outTriangles.Capacity, triangles.Length);
			outVertices.Capacity = Math.Max(outVertices.Capacity, triangles.Length);
			for (int i = 0; i < vertices.Length; i++)
			{
				outVertices.Add(vertices[i]);
			}
			for (int j = 0; j < triangles.Length; j++)
			{
				outTriangles.Add(triangles[j]);
			}
		}

		
		private void CompressMesh(List<Int3> vertices, List<int> triangles, ref Int3[] outVertices, ref int[] outTriangles, out int outVertexCount, out int outTriangleCount)
		{
			Dictionary<Int3, int> dictionary = this.cached_Int3_int_dict;
			dictionary.Clear();
			if (this.cached_int_array.Length < vertices.Count)
			{
				this.cached_int_array = new int[Math.Max(this.cached_int_array.Length * 2, vertices.Count)];
			}
			int[] array = this.cached_int_array;
			int num = 0;
			for (int i = 0; i < vertices.Count; i++)
			{
				int num2;
				if (!dictionary.TryGetValue(vertices[i], out num2) && !dictionary.TryGetValue(vertices[i] + new Int3(0, 1, 0), out num2) && !dictionary.TryGetValue(vertices[i] + new Int3(0, -1, 0), out num2))
				{
					dictionary.Add(vertices[i], num);
					array[i] = num;
					vertices[num] = vertices[i];
					num++;
				}
				else
				{
					array[i] = num2;
				}
			}
			outTriangleCount = triangles.Count;
			if (outTriangles == null || outTriangles.Length < outTriangleCount)
			{
				outTriangles = new int[outTriangleCount];
			}
			for (int j = 0; j < outTriangleCount; j++)
			{
				outTriangles[j] = array[triangles[j]];
			}
			outVertexCount = num;
			if (outVertices == null || outVertices.Length < outVertexCount)
			{
				outVertices = new Int3[outVertexCount];
			}
			for (int k = 0; k < outVertexCount; k++)
			{
				outVertices[k] = vertices[k];
			}
		}

		
		private void DelaunayRefinement(Int3[] verts, int[] tris, ref int vCount, ref int tCount, bool delaunay, bool colinear, Int3 worldOffset)
		{
			if (tCount % 3 != 0)
			{
				throw new ArgumentException("Triangle array length must be a multiple of 3");
			}
			Dictionary<Int2, int> dictionary = this.cached_Int2_int_dict;
			dictionary.Clear();
			for (int i = 0; i < tCount; i += 3)
			{
				if (!VectorMath.IsClockwiseXZ(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]))
				{
					int num = tris[i];
					tris[i] = tris[i + 2];
					tris[i + 2] = num;
				}
				dictionary[new Int2(tris[i], tris[i + 1])] = i + 2;
				dictionary[new Int2(tris[i + 1], tris[i + 2])] = i;
				dictionary[new Int2(tris[i + 2], tris[i])] = i + 1;
			}
			for (int j = 0; j < tCount; j += 3)
			{
				for (int k = 0; k < 3; k++)
				{
					int num2;
					if (dictionary.TryGetValue(new Int2(tris[j + (k + 1) % 3], tris[j + k % 3]), out num2))
					{
						Int3 @int = verts[tris[j + (k + 2) % 3]];
						Int3 int2 = verts[tris[j + (k + 1) % 3]];
						Int3 int3 = verts[tris[j + (k + 3) % 3]];
						Int3 int4 = verts[tris[num2]];
						@int.y = 0;
						int2.y = 0;
						int3.y = 0;
						int4.y = 0;
						bool flag = false;
						if (!VectorMath.RightOrColinearXZ(@int, int3, int4) || VectorMath.RightXZ(@int, int2, int4))
						{
							if (!colinear)
							{
								goto IL_416;
							}
							flag = true;
						}
						if (colinear && VectorMath.SqrDistancePointSegmentApproximate(@int, int4, int2) < 9f && !dictionary.ContainsKey(new Int2(tris[j + (k + 2) % 3], tris[j + (k + 1) % 3])) && !dictionary.ContainsKey(new Int2(tris[j + (k + 1) % 3], tris[num2])))
						{
							tCount -= 3;
							int num3 = num2 / 3 * 3;
							tris[j + (k + 1) % 3] = tris[num2];
							if (num3 != tCount)
							{
								tris[num3] = tris[tCount];
								tris[num3 + 1] = tris[tCount + 1];
								tris[num3 + 2] = tris[tCount + 2];
								dictionary[new Int2(tris[num3], tris[num3 + 1])] = num3 + 2;
								dictionary[new Int2(tris[num3 + 1], tris[num3 + 2])] = num3;
								dictionary[new Int2(tris[num3 + 2], tris[num3])] = num3 + 1;
								tris[tCount] = 0;
								tris[tCount + 1] = 0;
								tris[tCount + 2] = 0;
							}
							else
							{
								tCount += 3;
							}
							dictionary[new Int2(tris[j], tris[j + 1])] = j + 2;
							dictionary[new Int2(tris[j + 1], tris[j + 2])] = j;
							dictionary[new Int2(tris[j + 2], tris[j])] = j + 1;
						}
						else if (delaunay && !flag)
						{
							float num4 = Int3.Angle(int2 - @int, int3 - @int);
							float num5 = Int3.Angle(int2 - int4, int3 - int4);
							if (num5 > 6.28318548f - 2f * num4)
							{
								tris[j + (k + 1) % 3] = tris[num2];
								int num6 = num2 / 3 * 3;
								int num7 = num2 - num6;
								tris[num6 + (num7 - 1 + 3) % 3] = tris[j + (k + 2) % 3];
								dictionary[new Int2(tris[j], tris[j + 1])] = j + 2;
								dictionary[new Int2(tris[j + 1], tris[j + 2])] = j;
								dictionary[new Int2(tris[j + 2], tris[j])] = j + 1;
								dictionary[new Int2(tris[num6], tris[num6 + 1])] = num6 + 2;
								dictionary[new Int2(tris[num6 + 1], tris[num6 + 2])] = num6;
								dictionary[new Int2(tris[num6 + 2], tris[num6])] = num6 + 1;
							}
						}
					}
					IL_416:;
				}
			}
		}

		
		private Vector3 Point2D2V3(TriangulationPoint p)
		{
			return new Vector3((float)p.X, 0f, (float)p.Y) * 0.001f;
		}

		
		private Int3 IntPoint2Int3(IntPoint p)
		{
			return new Int3((int)p.X, 0, (int)p.Y);
		}

		
		public void ClearTile(int x, int z)
		{
			if (AstarPath.active == null)
			{
				return;
			}
			if (x < 0 || z < 0 || x >= this.graph.tileXCount || z >= this.graph.tileZCount)
			{
				return;
			}
			AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(IWorkItemContext context, bool force)
			{
				this.graph.ReplaceTile(x, z, new Int3[0], new int[0], false);
				this.activeTileTypes[x + z * this.graph.tileXCount] = null;
				GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
				context.QueueFloodFill();
				return true;
			}));
		}

		
		public void ReloadInBounds(Bounds b)
		{
			Int2 tileCoordinates = this.graph.GetTileCoordinates(b.min);
			Int2 tileCoordinates2 = this.graph.GetTileCoordinates(b.max);
			IntRect a = new IntRect(tileCoordinates.x, tileCoordinates.y, tileCoordinates2.x, tileCoordinates2.y);
			a = IntRect.Intersection(a, new IntRect(0, 0, this.graph.tileXCount - 1, this.graph.tileZCount - 1));
			if (!a.IsValid())
			{
				return;
			}
			for (int i = a.ymin; i <= a.ymax; i++)
			{
				for (int j = a.xmin; j <= a.xmax; j++)
				{
					this.ReloadTile(j, i);
				}
			}
		}

		
		public void ReloadTile(int x, int z)
		{
			if (x < 0 || z < 0 || x >= this.graph.tileXCount || z >= this.graph.tileZCount)
			{
				return;
			}
			int num = x + z * this.graph.tileXCount;
			if (this.activeTileTypes[num] != null)
			{
				this.LoadTile(this.activeTileTypes[num], x, z, this.activeTileRotations[num], this.activeTileOffsets[num]);
			}
		}

		
		public void CutShapeWithTile(int x, int z, Int3[] shape, ref Int3[] verts, ref int[] tris, out int vCount, out int tCount)
		{
			if (this.isBatching)
			{
				throw new Exception("Cannot cut with shape when batching. Please stop batching first.");
			}
			int num = x + z * this.graph.tileXCount;
			if (x < 0 || z < 0 || x >= this.graph.tileXCount || z >= this.graph.tileZCount || this.activeTileTypes[num] == null)
			{
				verts = new Int3[0];
				tris = new int[0];
				vCount = 0;
				tCount = 0;
				return;
			}
			Int3[] verts2;
			int[] tris2;
			this.activeTileTypes[num].Load(out verts2, out tris2, this.activeTileRotations[num], this.activeTileOffsets[num]);
			Bounds tileBounds = this.graph.GetTileBounds(x, z, 1, 1);
			Int3 @int = (Int3)tileBounds.min;
			@int = -@int;
			this.CutPoly(verts2, tris2, ref verts, ref tris, out vCount, out tCount, shape, @int, tileBounds, TileHandler.CutMode.CutExtra, -1);
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] -= @int;
			}
		}

		
		protected static T[] ShrinkArray<T>(T[] arr, int newLength)
		{
			newLength = Math.Min(newLength, arr.Length);
			T[] array = new T[newLength];
			if (newLength % 4 == 0)
			{
				for (int i = 0; i < newLength; i += 4)
				{
					array[i] = arr[i];
					array[i + 1] = arr[i + 1];
					array[i + 2] = arr[i + 2];
					array[i + 3] = arr[i + 3];
				}
			}
			else if (newLength % 3 == 0)
			{
				for (int j = 0; j < newLength; j += 3)
				{
					array[j] = arr[j];
					array[j + 1] = arr[j + 1];
					array[j + 2] = arr[j + 2];
				}
			}
			else if (newLength % 2 == 0)
			{
				for (int k = 0; k < newLength; k += 2)
				{
					array[k] = arr[k];
					array[k + 1] = arr[k + 1];
				}
			}
			else
			{
				for (int l = 0; l < newLength; l++)
				{
					array[l] = arr[l];
				}
			}
			return array;
		}

		
		public void LoadTile(TileHandler.TileType tile, int x, int z, int rotation, int yoffset)
		{
			if (tile == null)
			{
				throw new ArgumentNullException("tile");
			}
			if (AstarPath.active == null)
			{
				return;
			}
			int index = x + z * this.graph.tileXCount;
			rotation %= 4;
			if (this.isBatching && this.reloadedInBatch[index] && this.activeTileOffsets[index] == yoffset && this.activeTileRotations[index] == rotation && this.activeTileTypes[index] == tile)
			{
				return;
			}
			this.reloadedInBatch[index] |= this.isBatching;
			this.activeTileOffsets[index] = yoffset;
			this.activeTileRotations[index] = rotation;
			this.activeTileTypes[index] = tile;
			AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(IWorkItemContext context, bool force)
			{
				if (this.activeTileOffsets[index] != yoffset || this.activeTileRotations[index] != rotation || this.activeTileTypes[index] != tile)
				{
					return true;
				}
				GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);
				Int3[] verts;
				int[] tris;
				tile.Load(out verts, out tris, rotation, yoffset);
				Bounds tileBounds = this.graph.GetTileBounds(x, z, tile.Width, tile.Depth);
				Int3 @int = (Int3)tileBounds.min;
				@int = -@int;
				Int3[] array = null;
				int[] array2 = null;
				int num;
				int num2;
				this.CutPoly(verts, tris, ref array, ref array2, out num, out num2, null, @int, tileBounds, TileHandler.CutMode.CutAll | TileHandler.CutMode.CutDual, -1);
				this.DelaunayRefinement(array, array2, ref num, ref num2, true, false, -@int);
				if (num2 != array2.Length)
				{
					array2 = TileHandler.ShrinkArray<int>(array2, num2);
				}
				if (num != array.Length)
				{
					array = TileHandler.ShrinkArray<Int3>(array, num);
				}
				int w = (rotation % 2 != 0) ? tile.Depth : tile.Width;
				int d = (rotation % 2 != 0) ? tile.Width : tile.Depth;
				this.graph.ReplaceTile(x, z, w, d, array, array2, false);
				GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);
				context.QueueFloodFill();
				return true;
			}));
		}

		
		private readonly RecastGraph _graph;

		
		private readonly int tileXCount;

		
		private readonly int tileZCount;

		
		private readonly List<TileHandler.TileType> tileTypes = new List<TileHandler.TileType>();

		
		private readonly Clipper clipper = new Clipper(0);

		
		private int[] cached_int_array = new int[32];

		
		private readonly Dictionary<Int3, int> cached_Int3_int_dict = new Dictionary<Int3, int>();

		
		private readonly Dictionary<Int2, int> cached_Int2_int_dict = new Dictionary<Int2, int>();

		
		private readonly TileHandler.TileType[] activeTileTypes;

		
		private readonly int[] activeTileRotations;

		
		private readonly int[] activeTileOffsets;

		
		private readonly bool[] reloadedInBatch;

		
		private bool isBatching;

		
		private readonly VoxelPolygonClipper simpleClipper;

		
		public class TileType
		{
			
			public TileType(Int3[] sourceVerts, int[] sourceTris, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1)
			{
				if (sourceVerts == null)
				{
					throw new ArgumentNullException("sourceVerts");
				}
				if (sourceTris == null)
				{
					throw new ArgumentNullException("sourceTris");
				}
				this.tris = new int[sourceTris.Length];
				for (int i = 0; i < this.tris.Length; i++)
				{
					this.tris[i] = sourceTris[i];
				}
				this.verts = new Int3[sourceVerts.Length];
				for (int j = 0; j < sourceVerts.Length; j++)
				{
					this.verts[j] = sourceVerts[j] + centerOffset;
				}
				this.offset = tileSize / 2f;
				this.offset.x = this.offset.x * width;
				this.offset.z = this.offset.z * depth;
				this.offset.y = 0;
				for (int k = 0; k < sourceVerts.Length; k++)
				{
					this.verts[k] = this.verts[k] + this.offset;
				}
				this.lastRotation = 0;
				this.lastYOffset = 0;
				this.width = width;
				this.depth = depth;
			}

			
			public TileType(Mesh source, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1)
			{
				if (source == null)
				{
					throw new ArgumentNullException("source");
				}
				Vector3[] vertices = source.vertices;
				this.tris = source.triangles;
				this.verts = new Int3[vertices.Length];
				for (int i = 0; i < vertices.Length; i++)
				{
					this.verts[i] = (Int3)vertices[i] + centerOffset;
				}
				this.offset = tileSize / 2f;
				this.offset.x = this.offset.x * width;
				this.offset.z = this.offset.z * depth;
				this.offset.y = 0;
				for (int j = 0; j < vertices.Length; j++)
				{
					this.verts[j] = this.verts[j] + this.offset;
				}
				this.lastRotation = 0;
				this.lastYOffset = 0;
				this.width = width;
				this.depth = depth;
			}

			
			
			public int Width
			{
				get
				{
					return this.width;
				}
			}

			
			
			public int Depth
			{
				get
				{
					return this.depth;
				}
			}

			
			public void Load(out Int3[] verts, out int[] tris, int rotation, int yoffset)
			{
				rotation = (rotation % 4 + 4) % 4;
				int num = rotation;
				rotation = (rotation - this.lastRotation % 4 + 4) % 4;
				this.lastRotation = num;
				verts = this.verts;
				int num2 = yoffset - this.lastYOffset;
				this.lastYOffset = yoffset;
				if (rotation != 0 || num2 != 0)
				{
					for (int i = 0; i < verts.Length; i++)
					{
						Int3 @int = verts[i] - this.offset;
						Int3 lhs = @int;
						lhs.y += num2;
						lhs.x = @int.x * TileHandler.TileType.Rotations[rotation * 4] + @int.z * TileHandler.TileType.Rotations[rotation * 4 + 1];
						lhs.z = @int.x * TileHandler.TileType.Rotations[rotation * 4 + 2] + @int.z * TileHandler.TileType.Rotations[rotation * 4 + 3];
						verts[i] = lhs + this.offset;
					}
				}
				tris = this.tris;
			}

			
			private Int3[] verts;

			
			private int[] tris;

			
			private Int3 offset;

			
			private int lastYOffset;

			
			private int lastRotation;

			
			private int width;

			
			private int depth;

			
			private static readonly int[] Rotations = new int[]
			{
				1,
				0,
				0,
				1,
				0,
				1,
				-1,
				0,
				-1,
				0,
				0,
				-1,
				0,
				-1,
				1,
				0
			};
		}

		
		[Flags]
		public enum CutMode
		{
			
			CutAll = 1,
			
			CutDual = 2,
			
			CutExtra = 4
		}

		
		private class Cut
		{
			
			public IntRect bounds;

			
			public Int2 boundsY;

			
			public bool isDual;

			
			public bool cutsAddedGeom;

			
			public List<IntPoint> contour;
		}
	}
}
