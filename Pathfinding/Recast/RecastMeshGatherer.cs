using System;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Voxels;
using UnityEngine;

namespace Pathfinding.Recast
{
	
	internal class RecastMeshGatherer
	{
		
		public RecastMeshGatherer(Bounds bounds, int terrainSampleSize, LayerMask mask, List<string> tagMask, float colliderRasterizeDetail)
		{
			terrainSampleSize = Math.Max(terrainSampleSize, 1);
			this.bounds = bounds;
			this.terrainSampleSize = terrainSampleSize;
			this.mask = mask;
			this.tagMask = (tagMask ?? new List<string>());
			this.colliderRasterizeDetail = colliderRasterizeDetail;
		}

		
		private static List<MeshFilter> FilterMeshes(MeshFilter[] meshFilters, List<string> tagMask, LayerMask layerMask)
		{
			List<MeshFilter> list = new List<MeshFilter>(meshFilters.Length / 3);
			foreach (MeshFilter meshFilter in meshFilters)
			{
				Renderer component = meshFilter.GetComponent<Renderer>();
				if (component != null && meshFilter.sharedMesh != null && component.enabled && ((1 << meshFilter.gameObject.layer & layerMask) != 0 || tagMask.Contains(meshFilter.tag)) && meshFilter.GetComponent<RecastMeshObj>() == null)
				{
					list.Add(meshFilter);
				}
			}
			return list;
		}

		
		public void CollectSceneMeshes(List<RasterizationMesh> meshes)
		{
			if (this.tagMask.Count > 0 || this.mask != 0)
			{
				MeshFilter[] meshFilters = UnityEngine.Object.FindObjectsOfType<MeshFilter>();
				List<MeshFilter> list = RecastMeshGatherer.FilterMeshes(meshFilters, this.tagMask, this.mask);
				Dictionary<Mesh, Vector3[]> dictionary = new Dictionary<Mesh, Vector3[]>();
				Dictionary<Mesh, int[]> dictionary2 = new Dictionary<Mesh, int[]>();
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					MeshFilter meshFilter = list[i];
					Renderer component = meshFilter.GetComponent<Renderer>();
					if (component.isPartOfStaticBatch)
					{
						flag = true;
					}
					else if (component.bounds.Intersects(this.bounds))
					{
						Mesh sharedMesh = meshFilter.sharedMesh;
						RasterizationMesh rasterizationMesh = new RasterizationMesh();
						rasterizationMesh.matrix = component.localToWorldMatrix;
						rasterizationMesh.original = meshFilter;
						if (dictionary.ContainsKey(sharedMesh))
						{
							rasterizationMesh.vertices = dictionary[sharedMesh];
							rasterizationMesh.triangles = dictionary2[sharedMesh];
						}
						else
						{
							rasterizationMesh.vertices = sharedMesh.vertices;
							rasterizationMesh.triangles = sharedMesh.triangles;
							dictionary[sharedMesh] = rasterizationMesh.vertices;
							dictionary2[sharedMesh] = rasterizationMesh.triangles;
						}
						rasterizationMesh.bounds = component.bounds;
						meshes.Add(rasterizationMesh);
					}
					if (flag)
					{
						Debug.LogWarning("Some meshes were statically batched. These meshes can not be used for navmesh calculation due to technical constraints.\nDuring runtime scripts cannot access the data of meshes which have been statically batched.\nOne way to solve this problem is to use cached startup (Save & Load tab in the inspector) to only calculate the graph when the game is not playing.");
					}
				}
			}
		}

		
		public void CollectRecastMeshObjs(List<RasterizationMesh> buffer)
		{
			List<RecastMeshObj> list = ListPool<RecastMeshObj>.Claim();
			RecastMeshObj.GetAllInBounds(list, this.bounds);
			Dictionary<Mesh, Vector3[]> dictionary = new Dictionary<Mesh, Vector3[]>();
			Dictionary<Mesh, int[]> dictionary2 = new Dictionary<Mesh, int[]>();
			for (int i = 0; i < list.Count; i++)
			{
				MeshFilter meshFilter = list[i].GetMeshFilter();
				Renderer renderer = (!(meshFilter != null)) ? null : meshFilter.GetComponent<Renderer>();
				if (meshFilter != null && renderer != null)
				{
					Mesh sharedMesh = meshFilter.sharedMesh;
					RasterizationMesh rasterizationMesh = new RasterizationMesh();
					rasterizationMesh.matrix = renderer.localToWorldMatrix;
					rasterizationMesh.original = meshFilter;
					rasterizationMesh.area = list[i].area;
					if (dictionary.ContainsKey(sharedMesh))
					{
						rasterizationMesh.vertices = dictionary[sharedMesh];
						rasterizationMesh.triangles = dictionary2[sharedMesh];
					}
					else
					{
						rasterizationMesh.vertices = sharedMesh.vertices;
						rasterizationMesh.triangles = sharedMesh.triangles;
						dictionary[sharedMesh] = rasterizationMesh.vertices;
						dictionary2[sharedMesh] = rasterizationMesh.triangles;
					}
					rasterizationMesh.bounds = renderer.bounds;
					buffer.Add(rasterizationMesh);
				}
				else
				{
					Collider collider = list[i].GetCollider();
					if (collider == null)
					{
						Debug.LogError("RecastMeshObject (" + list[i].gameObject.name + ") didn't have a collider or MeshFilter+Renderer attached", list[i].gameObject);
					}
					else
					{
						RasterizationMesh rasterizationMesh2 = this.RasterizeCollider(collider);
						if (rasterizationMesh2 != null)
						{
							rasterizationMesh2.area = list[i].area;
							buffer.Add(rasterizationMesh2);
						}
					}
				}
			}
			this.capsuleCache.Clear();
			ListPool<RecastMeshObj>.Release(list);
		}

		
		public void CollectTerrainMeshes(bool rasterizeTrees, float desiredChunkSize, List<RasterizationMesh> result)
		{
			Terrain[] array = UnityEngine.Object.FindObjectsOfType(typeof(Terrain)) as Terrain[];
			if (array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (!(array[i].terrainData == null))
					{
						this.GenerateTerrainChunks(array[i], this.bounds, desiredChunkSize, result);
						if (rasterizeTrees)
						{
							this.CollectTreeMeshes(array[i], result);
						}
					}
				}
			}
		}

		
		private void GenerateTerrainChunks(Terrain terrain, Bounds bounds, float desiredChunkSize, List<RasterizationMesh> result)
		{
			TerrainData terrainData = terrain.terrainData;
			if (terrainData == null)
			{
				throw new ArgumentException("Terrain contains no terrain data");
			}
			Vector3 position = terrain.GetPosition();
			Vector3 center = position + terrainData.size * 0.5f;
			Bounds bounds2 = new Bounds(center, terrainData.size);
			if (!bounds2.Intersects(bounds))
			{
				return;
			}
			int heightmapWidth = terrainData.heightmapWidth;
			int heightmapHeight = terrainData.heightmapHeight;
			float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
			Vector3 heightmapScale = terrainData.heightmapScale;
			heightmapScale.y = terrainData.size.y;
			int num = Mathf.CeilToInt(Mathf.Max(desiredChunkSize / (heightmapScale.x * (float)this.terrainSampleSize), 12f)) * this.terrainSampleSize;
			int num2 = Mathf.CeilToInt(Mathf.Max(desiredChunkSize / (heightmapScale.z * (float)this.terrainSampleSize), 12f)) * this.terrainSampleSize;
			for (int i = 0; i < heightmapHeight; i += num2)
			{
				for (int j = 0; j < heightmapWidth; j += num)
				{
					int width = Mathf.Min(num, heightmapWidth - j);
					int depth = Mathf.Min(num2, heightmapHeight - i);
					RasterizationMesh item = this.GenerateHeightmapChunk(heights, heightmapScale, position, j, i, width, depth, this.terrainSampleSize);
					result.Add(item);
				}
			}
		}

		
		private static int CeilDivision(int lhs, int rhs)
		{
			return (lhs + rhs - 1) / rhs;
		}

		
		private RasterizationMesh GenerateHeightmapChunk(float[,] heights, Vector3 sampleSize, Vector3 offset, int x0, int z0, int width, int depth, int stride)
		{
			int num = RecastMeshGatherer.CeilDivision(width, this.terrainSampleSize) + 1;
			int num2 = RecastMeshGatherer.CeilDivision(depth, this.terrainSampleSize) + 1;
			int length = heights.GetLength(0);
			int length2 = heights.GetLength(1);
			Vector3[] array = new Vector3[num * num2];
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					int num3 = Math.Min(x0 + j * stride, length - 1);
					int num4 = Math.Min(z0 + i * stride, length2 - 1);
					array[i * num + j] = new Vector3((float)num4 * sampleSize.x, heights[num3, num4] * sampleSize.y, (float)num3 * sampleSize.z) + offset;
				}
			}
			int[] array2 = new int[(num - 1) * (num2 - 1) * 2 * 3];
			int num5 = 0;
			for (int k = 0; k < num2 - 1; k++)
			{
				for (int l = 0; l < num - 1; l++)
				{
					array2[num5] = k * num + l;
					array2[num5 + 1] = k * num + l + 1;
					array2[num5 + 2] = (k + 1) * num + l + 1;
					num5 += 3;
					array2[num5] = k * num + l;
					array2[num5 + 1] = (k + 1) * num + l + 1;
					array2[num5 + 2] = (k + 1) * num + l;
					num5 += 3;
				}
			}
			RasterizationMesh rasterizationMesh = new RasterizationMesh(array, array2, default(Bounds));
			rasterizationMesh.RecalculateBounds();
			return rasterizationMesh;
		}

		
		private void CollectTreeMeshes(Terrain terrain, List<RasterizationMesh> result)
		{
			TerrainData terrainData = terrain.terrainData;
			for (int i = 0; i < terrainData.treeInstances.Length; i++)
			{
				TreeInstance treeInstance = terrainData.treeInstances[i];
				TreePrototype treePrototype = terrainData.treePrototypes[treeInstance.prototypeIndex];
				if (!(treePrototype.prefab == null))
				{
					Collider component = treePrototype.prefab.GetComponent<Collider>();
					Vector3 pos = terrain.transform.position + Vector3.Scale(treeInstance.position, terrainData.size);
					if (component == null)
					{
						Bounds bounds = new Bounds(terrain.transform.position + Vector3.Scale(treeInstance.position, terrainData.size), new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale));
						Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale) * 0.5f);
						RasterizationMesh item = new RasterizationMesh(RecastMeshGatherer.BoxColliderVerts, RecastMeshGatherer.BoxColliderTris, bounds, matrix);
						result.Add(item);
					}
					else
					{
						Vector3 s = new Vector3(treeInstance.widthScale, treeInstance.heightScale, treeInstance.widthScale);
						RasterizationMesh rasterizationMesh = this.RasterizeCollider(component, Matrix4x4.TRS(pos, Quaternion.identity, s));
						if (rasterizationMesh != null)
						{
							rasterizationMesh.RecalculateBounds();
							result.Add(rasterizationMesh);
						}
					}
				}
			}
		}

		
		public void CollectColliderMeshes(List<RasterizationMesh> result)
		{
			Collider[] array = UnityEngine.Object.FindObjectsOfType<Collider>();
			if (this.tagMask.Count > 0 || this.mask != 0)
			{
				foreach (Collider collider in array)
				{
					if (((this.mask >> collider.gameObject.layer & 1) != 0 || this.tagMask.Contains(collider.tag)) && collider.enabled && !collider.isTrigger && collider.bounds.Intersects(this.bounds) && collider.GetComponent<RecastMeshObj>() == null)
					{
						RasterizationMesh rasterizationMesh = this.RasterizeCollider(collider);
						if (rasterizationMesh != null)
						{
							result.Add(rasterizationMesh);
						}
					}
				}
			}
			this.capsuleCache.Clear();
		}

		
		private RasterizationMesh RasterizeCollider(Collider col)
		{
			return this.RasterizeCollider(col, col.transform.localToWorldMatrix);
		}

		
		private RasterizationMesh RasterizeCollider(Collider col, Matrix4x4 localToWorldMatrix)
		{
			RasterizationMesh result = null;
			if (col is BoxCollider)
			{
				result = this.RasterizeBoxCollider(col as BoxCollider, localToWorldMatrix);
			}
			else if (col is SphereCollider || col is CapsuleCollider)
			{
				SphereCollider sphereCollider = col as SphereCollider;
				CapsuleCollider capsuleCollider = col as CapsuleCollider;
				float num = (!(sphereCollider != null)) ? capsuleCollider.radius : sphereCollider.radius;
				float height = (!(sphereCollider != null)) ? (capsuleCollider.height * 0.5f / num - 1f) : 0f;
				Matrix4x4 matrix4x = Matrix4x4.TRS((!(sphereCollider != null)) ? capsuleCollider.center : sphereCollider.center, Quaternion.identity, Vector3.one * num);
				matrix4x = localToWorldMatrix * matrix4x;
				result = this.RasterizeCapsuleCollider(num, height, col.bounds, matrix4x);
			}
			else if (col is MeshCollider)
			{
				MeshCollider meshCollider = col as MeshCollider;
				if (meshCollider.sharedMesh != null && meshCollider.sharedMesh.isReadable)
				{
					result = new RasterizationMesh(meshCollider.sharedMesh.vertices, meshCollider.sharedMesh.triangles, meshCollider.bounds, localToWorldMatrix);
				}
			}
			return result;
		}

		
		private RasterizationMesh RasterizeBoxCollider(BoxCollider collider, Matrix4x4 localToWorldMatrix)
		{
			Matrix4x4 matrix4x = Matrix4x4.TRS(collider.center, Quaternion.identity, collider.size * 0.5f);
			matrix4x = localToWorldMatrix * matrix4x;
			return new RasterizationMesh(RecastMeshGatherer.BoxColliderVerts, RecastMeshGatherer.BoxColliderTris, collider.bounds, matrix4x);
		}

		
		private RasterizationMesh RasterizeCapsuleCollider(float radius, float height, Bounds bounds, Matrix4x4 localToWorldMatrix)
		{
			int num = Mathf.Max(4, Mathf.RoundToInt(this.colliderRasterizeDetail * Mathf.Sqrt(localToWorldMatrix.MultiplyVector(Vector3.one).magnitude)));
			if (num > 100)
			{
				Debug.LogWarning("Very large detail for some collider meshes. Consider decreasing Collider Rasterize Detail (RecastGraph)");
			}
			int num2 = num;
			RecastMeshGatherer.CapsuleCache capsuleCache = null;
			for (int i = 0; i < this.capsuleCache.Count; i++)
			{
				RecastMeshGatherer.CapsuleCache capsuleCache2 = this.capsuleCache[i];
				if (capsuleCache2.rows == num && Mathf.Approximately(capsuleCache2.height, height))
				{
					capsuleCache = capsuleCache2;
				}
			}
			Vector3[] array;
			if (capsuleCache == null)
			{
				array = new Vector3[num * num2 + 2];
				List<int> list = new List<int>();
				array[array.Length - 1] = Vector3.up;
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num2; k++)
					{
						array[k + j * num2] = new Vector3(Mathf.Cos((float)k * 3.14159274f * 2f / (float)num2) * Mathf.Sin((float)j * 3.14159274f / (float)(num - 1)), Mathf.Cos((float)j * 3.14159274f / (float)(num - 1)) + ((j >= num / 2) ? (-height) : height), Mathf.Sin((float)k * 3.14159274f * 2f / (float)num2) * Mathf.Sin((float)j * 3.14159274f / (float)(num - 1)));
					}
				}
				array[array.Length - 2] = Vector3.down;
				int l = 0;
				int num3 = num2 - 1;
				while (l < num2)
				{
					list.Add(array.Length - 1);
					list.Add(0 * num2 + num3);
					list.Add(0 * num2 + l);
					num3 = l++;
				}
				for (int m = 1; m < num; m++)
				{
					int n = 0;
					int num4 = num2 - 1;
					while (n < num2)
					{
						list.Add(m * num2 + n);
						list.Add(m * num2 + num4);
						list.Add((m - 1) * num2 + n);
						list.Add((m - 1) * num2 + num4);
						list.Add((m - 1) * num2 + n);
						list.Add(m * num2 + num4);
						num4 = n++;
					}
				}
				int num5 = 0;
				int num6 = num2 - 1;
				while (num5 < num2)
				{
					list.Add(array.Length - 2);
					list.Add((num - 1) * num2 + num6);
					list.Add((num - 1) * num2 + num5);
					num6 = num5++;
				}
				capsuleCache = new RecastMeshGatherer.CapsuleCache();
				capsuleCache.rows = num;
				capsuleCache.height = height;
				capsuleCache.verts = array;
				capsuleCache.tris = list.ToArray();
				this.capsuleCache.Add(capsuleCache);
			}
			array = capsuleCache.verts;
			int[] tris = capsuleCache.tris;
			return new RasterizationMesh(array, tris, bounds, localToWorldMatrix);
		}

		
		private readonly int terrainSampleSize;

		
		private readonly LayerMask mask;

		
		private readonly List<string> tagMask;

		
		private readonly float colliderRasterizeDetail;

		
		private readonly Bounds bounds;

		
		private static readonly int[] BoxColliderTris = new int[]
		{
			0,
			1,
			2,
			0,
			2,
			3,
			6,
			5,
			4,
			7,
			6,
			4,
			0,
			5,
			1,
			0,
			4,
			5,
			1,
			6,
			2,
			1,
			5,
			6,
			2,
			7,
			3,
			2,
			6,
			7,
			3,
			4,
			0,
			3,
			7,
			4
		};

		
		private static readonly Vector3[] BoxColliderVerts = new Vector3[]
		{
			new Vector3(-1f, -1f, -1f),
			new Vector3(1f, -1f, -1f),
			new Vector3(1f, -1f, 1f),
			new Vector3(-1f, -1f, 1f),
			new Vector3(-1f, 1f, -1f),
			new Vector3(1f, 1f, -1f),
			new Vector3(1f, 1f, 1f),
			new Vector3(-1f, 1f, 1f)
		};

		
		private List<RecastMeshGatherer.CapsuleCache> capsuleCache = new List<RecastMeshGatherer.CapsuleCache>();

		
		private class CapsuleCache
		{
			
			public int rows;

			
			public float height;

			
			public Vector3[] verts;

			
			public int[] tris;
		}
	}
}
