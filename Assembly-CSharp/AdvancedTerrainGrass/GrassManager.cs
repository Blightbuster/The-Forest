using System;
using System.Collections.Generic;
using System.Threading;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace AdvancedTerrainGrass
{
	[RequireComponent(typeof(Terrain))]
	public class GrassManager : MonoBehaviour
	{
		private void OnEnable()
		{
			this.Init();
		}

		private void OnDisable()
		{
			this.cullingGroup.Dispose();
			this.cullingGroup = null;
			for (int i = 0; i < this.CellContent.Length; i++)
			{
				this.CellContent[i].RelaseBuffers();
			}
			for (int j = 0; j < this.matrixBuffers.Length; j++)
			{
				if (this.matrixBuffers[j] != null)
				{
					this.matrixBuffers[j].Release();
				}
			}
			for (int k = 0; k < this.argsBuffers.Length; k++)
			{
				if (this.argsBuffers[k] != null)
				{
					this.argsBuffers[k].Release();
				}
			}
			this.v_mesh = null;
			this.v_mat = null;
			this.materialPropertyBlocks = null;
			this.WorkerThread.Abort();
			this.WorkerThread = null;
			this.Cells = null;
			this.CellContent = null;
			this.mapByte = null;
			this.TerrainHeights = null;
			this.tempMatrixArray = null;
			this.SoftlyMergedLayers = null;
		}

		private void Update()
		{
			this.DrawGrass();
		}

		public static float GetATGRandNext()
		{
			GrassManager.ATGSeed = (uint)((ulong)GrassManager.ATGSeed * 279470273UL % (ulong)-5);
			return GrassManager.ATGSeed * GrassManager.OneOverInt32MaxVal;
		}

		public void RemoveGrass(Vector3 Worldposition, float Radius)
		{
			Vector2 vector = new Vector2(Worldposition.z - this.TerrainPosition.z, Worldposition.x - this.TerrainPosition.x);
			Radius *= 1.5f;
			if (vector.x > this.TerrainSize.x || vector.x < 0f || vector.y > this.TerrainSize.z || vector.y < 0f)
			{
				return;
			}
			float num = 1f / this.CellSize;
			vector.x *= num;
			vector.y *= num;
			float num2 = Radius * num;
			Vector2 vector2 = new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
			this.CurrentlyAffectedCells[0] = (int)(vector2.x + vector2.y * (float)this.NumberOfCells);
			if (vector.x + num2 > vector2.x + 1f)
			{
				this.CurrentlyAffectedCells[1] = (int)(vector2.x + 1f + vector2.y * (float)this.NumberOfCells);
				if (vector.y + num2 > vector2.y + 1f)
				{
					this.CurrentlyAffectedCells[2] = (int)(vector2.x + 1f + (vector2.y + 1f) * (float)this.NumberOfCells);
				}
				else if (vector.y - num2 < vector2.y)
				{
					this.CurrentlyAffectedCells[2] = (int)(vector2.x + 1f + (vector2.y - 1f) * (float)this.NumberOfCells);
				}
				else
				{
					this.CurrentlyAffectedCells[2] = -1;
				}
			}
			else if (vector.x - num2 < vector2.x)
			{
				this.CurrentlyAffectedCells[1] = (int)(vector2.x - 1f + vector2.y * (float)this.NumberOfCells);
				if (vector.y + num2 > vector2.y + 1f)
				{
					this.CurrentlyAffectedCells[2] = (int)(vector2.x - 1f + (vector2.y + 1f) * (float)this.NumberOfCells);
				}
				else if (vector.y - num2 < vector2.y)
				{
					this.CurrentlyAffectedCells[2] = (int)(vector2.x - 1f + (vector2.y - 1f) * (float)this.NumberOfCells);
				}
				else
				{
					this.CurrentlyAffectedCells[2] = -1;
				}
			}
			else
			{
				this.CurrentlyAffectedCells[1] = -1;
				this.CurrentlyAffectedCells[2] = -1;
			}
			if (vector.y + num2 > vector2.y + 1f)
			{
				this.CurrentlyAffectedCells[3] = (int)(vector2.x + (vector2.y + 1f) * (float)this.NumberOfCells);
			}
			else if (vector.y - num2 < vector2.y)
			{
				this.CurrentlyAffectedCells[3] = (int)(vector2.x + (vector2.y - 1f) * (float)this.NumberOfCells);
			}
			else
			{
				this.CurrentlyAffectedCells[3] = -1;
			}
			float num3 = Radius * Radius;
			for (int i = 0; i < 4; i++)
			{
				int num4 = this.CurrentlyAffectedCells[i];
				if (num4 > -1)
				{
					GrassCell grassCell = this.Cells[num4];
					GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[0]];
					if (grassCell.state == 3)
					{
						grassCell.state = 4;
					}
					Vector2 vector3 = new Vector2(grassCell.Center.x - this.CellSize * 0.5f, grassCell.Center.z - this.CellSize * 0.5f);
					Vector2 vector4 = new Vector2(vector3.x + this.BucketSize * 0.5f, vector3.y + this.BucketSize * 0.5f);
					float y = vector4.y;
					int num5 = grassCellContent.PatchOffsetX + grassCellContent.PatchOffsetZ;
					for (int j = 0; j < this.NumberOfBucketsPerCell; j++)
					{
						for (int k = 0; k < this.NumberOfBucketsPerCell; k++)
						{
							Vector2 vector5 = new Vector2(Worldposition.x - vector4.x, Worldposition.z - vector4.y);
							float sqrMagnitude = vector5.sqrMagnitude;
							if (sqrMagnitude < num3)
							{
								float num6 = (num3 - sqrMagnitude) / num3;
								float num7 = num6 * num6 * num6;
								float num8 = 1f - (7f + (num7 - 7f) * num6) * num7;
								int num9 = num5 + k;
								for (int l = 0; l < this.NumberOfLayers; l++)
								{
									int num10 = (int)this.mapByte[l][num9];
									this.mapByte[l][num9] = Convert.ToByte((int)((float)num10 * num8));
								}
							}
							vector4.y += this.BucketSize;
						}
						vector4.y = y;
						vector4.x += this.BucketSize;
						num5 += (int)this.TerrainDetailSize.y;
					}
				}
			}
		}

		internal void RefreshGrassRenderingSettings(float newDetailDensity, float newCullDistance)
		{
			float t_FadeLength = newCullDistance * 0.2f;
			float t_CacheDistance = (float)Mathf.CeilToInt(newCullDistance / this.CellSize) * this.CellSize + Mathf.Sqrt(this.CellSize * this.CellSize * 0.25f);
			float t_DetailFadeLength = newCullDistance * 0.4f;
			float shadowStart = this.ShadowStart;
			float shadowFadeLength = this.ShadowFadeLength;
			float shadowStartFoliage = this.ShadowStartFoliage;
			float shadowFadeLengthFoliage = this.ShadowFadeLengthFoliage;
			this.RefreshGrassRenderingSettings(newDetailDensity, newCullDistance, t_FadeLength, t_CacheDistance, newCullDistance, t_DetailFadeLength, shadowStart, shadowFadeLength, shadowStartFoliage, shadowFadeLengthFoliage);
		}

		public void RefreshGrassRenderingSettings(float t_DetailDensity, float t_CullDistance, float t_FadeLength, float t_CacheDistance, float t_DetailFadeStart, float t_DetailFadeLength, float t_ShadowStart, float t_ShadowFadeLength, float t_ShadowStartFoliage, float t_ShadowFadeLengthFoliage)
		{
			this.CellsOrCellContentsToInit.Clear();
			this.CellToInit = -1;
			this.DataCopied = true;
			this.TargetDetailDensity = t_DetailDensity;
			this.rebuildtempMatrixArray = true;
			float[] boundingDistances = new float[]
			{
				t_CullDistance,
				t_CacheDistance
			};
			this.cullingGroup.SetBoundingDistances(boundingDistances);
			Shader.SetGlobalVector(this.GrassFadePropsPID, new Vector4((t_CullDistance - t_FadeLength) * (t_CullDistance - t_FadeLength), 1f / (t_FadeLength * t_FadeLength), t_DetailFadeStart * t_DetailFadeStart, 1f / (t_DetailFadeLength * t_DetailFadeLength)));
			Shader.SetGlobalVector(this.GrassShadowFadePropsPID, new Vector4(t_ShadowStart * t_ShadowStart, 1f / (t_ShadowFadeLength * t_ShadowFadeLength), t_ShadowStartFoliage * t_ShadowStartFoliage, 1f / (t_ShadowFadeLengthFoliage * t_ShadowFadeLengthFoliage)));
		}

		public void InitCellsFast()
		{
			this.NumberOfLayers = this.terData.detailPrototypes.Length;
			this.mapByte = new byte[this.SavedTerrainData.DensityMaps.Count][];
			for (int i = 0; i < this.SavedTerrainData.DensityMaps.Count; i++)
			{
				this.mapByte[i] = this.SavedTerrainData.DensityMaps[i].mapByte;
			}
			this.Cells = new GrassCell[this.SavedTerrainData.Cells.Length];
			this.CellContent = new GrassCellContent[this.SavedTerrainData.CellContent.Length];
			this.maxBucketDensity = this.SavedTerrainData.maxBucketDensity;
			this.MaxLayersPerCell = this.SavedTerrainData.MaxLayersPerCell;
			int num = 0;
			int num2 = this.SavedTerrainData.Cells.Length;
			for (int j = 0; j < num2; j++)
			{
				this.Cells[j] = new GrassCell();
				this.Cells[j].index = j;
				this.Cells[j].Center = this.SavedTerrainData.Cells[j].Center;
				this.Cells[j].CellContentIndexes = this.SavedTerrainData.Cells[j].CellContentIndexes;
				this.Cells[j].CellContentCount = this.SavedTerrainData.Cells[j].CellContentCount;
				int cellContentCount = this.Cells[j].CellContentCount;
				for (int k = 0; k < cellContentCount; k++)
				{
					this.CellContent[num] = new GrassCellContent();
					GrassCellContent grassCellContent = this.CellContent[num];
					GrassCellContent grassCellContent2 = this.SavedTerrainData.CellContent[num];
					grassCellContent.RelatedGrassManager = this;
					grassCellContent.index = num;
					grassCellContent.Layer = grassCellContent2.Layer;
					if (grassCellContent2.SoftlyMergedLayers.Length > 0)
					{
						grassCellContent.SoftlyMergedLayers = grassCellContent2.SoftlyMergedLayers;
					}
					else
					{
						grassCellContent.SoftlyMergedLayers = null;
					}
					grassCellContent.GrassMatrixBufferPID = this.GrassMatrixBufferPID;
					grassCellContent.Center = grassCellContent2.Center;
					grassCellContent.Pivot = grassCellContent2.Pivot;
					grassCellContent.PatchOffsetX = grassCellContent2.PatchOffsetX;
					grassCellContent.PatchOffsetZ = grassCellContent2.PatchOffsetZ;
					grassCellContent.Instances = grassCellContent2.Instances;
					num++;
				}
			}
		}

		public void InitCells()
		{
			this.NumberOfLayers = this.terData.detailPrototypes.Length;
			this.OrigNumberOfLayers = this.NumberOfLayers;
			int[][] array = new int[this.OrigNumberOfLayers][];
			int[][] array2 = new int[this.OrigNumberOfLayers][];
			for (int i = 0; i < this.OrigNumberOfLayers; i++)
			{
				int num = this.LayerToMergeWith[i];
				int num2 = num - 1;
				if (num != 0 && num != i + 1 && this.LayerToMergeWith[num2] == 0)
				{
					if (array[num2] == null)
					{
						array[num2] = new int[this.OrigNumberOfLayers - 1];
					}
					if (this.DoSoftMerge[i] && array2[num2] == null)
					{
						array2[num2] = new int[this.OrigNumberOfLayers - 1];
					}
					for (int j = 0; j < this.OrigNumberOfLayers - 1; j++)
					{
						if (array[num2][j] == 0)
						{
							array[num2][j] = i + 1;
							break;
						}
					}
					if (this.DoSoftMerge[i])
					{
						for (int k = 0; k < this.OrigNumberOfLayers - 1; k++)
						{
							if (array2[num2][k] == 0)
							{
								array2[num2][k] = i + 1;
								break;
							}
						}
					}
				}
			}
			this.Cells = new GrassCell[this.NumberOfCells * this.NumberOfCells];
			List<GrassCellContent> list = new List<GrassCellContent>();
			list.Capacity = this.NumberOfCells * this.NumberOfCells * this.NumberOfLayers;
			this.mapByte = new byte[this.NumberOfLayers][];
			for (int l = 0; l < this.NumberOfLayers; l++)
			{
				if (this.LayerToMergeWith[l] == 0 || this.DoSoftMerge[l])
				{
					this.mapByte[l] = new byte[(int)(this.TerrainDetailSize.x * this.TerrainDetailSize.y)];
					bool flag = false;
					if (array[l] != null && !this.DoSoftMerge[l])
					{
						flag = true;
					}
					for (int m = 0; m < (int)this.TerrainDetailSize.x; m++)
					{
						for (int n = 0; n < (int)this.TerrainDetailSize.y; n++)
						{
							int[,] detailLayer = this.terData.GetDetailLayer(m, n, 1, 1, l);
							this.mapByte[l][m * (int)this.TerrainDetailSize.y + n] = Convert.ToByte(detailLayer[0, 0]);
							if (flag)
							{
								for (int num3 = 0; num3 < this.OrigNumberOfLayers - 1; num3++)
								{
									if (array[l][num3] == 0 || this.DoSoftMerge[array[l][num3] - 1])
									{
										break;
									}
									detailLayer = this.terData.GetDetailLayer(m, n, 1, 1, array[l][num3] - 1);
									this.mapByte[l][m * (int)this.TerrainDetailSize.y + n] = Convert.ToByte((int)this.mapByte[l][m * (int)this.TerrainDetailSize.y + n] + detailLayer[0, 0]);
								}
							}
						}
					}
				}
			}
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			for (int num7 = 0; num7 < this.NumberOfCells; num7++)
			{
				for (int num8 = 0; num8 < this.NumberOfCells; num8++)
				{
					int num9 = 0;
					int num10 = num7 * this.NumberOfCells + num8;
					Vector2 vector;
					vector.x = ((float)num7 * this.CellSize + 0.5f * this.CellSize) * this.OneOverTerrainSize.x;
					vector.y = ((float)num8 * this.CellSize + 0.5f * this.CellSize) * this.OneOverTerrainSize.z;
					float interpolatedHeight = this.terData.GetInterpolatedHeight(vector.x, vector.y);
					this.Cells[num10] = new GrassCell();
					this.Cells[num10].index = num10;
					Vector3 center;
					center.x = (float)num7 * this.CellSize + 0.5f * this.CellSize + this.TerrainPosition.x;
					center.y = interpolatedHeight + this.TerrainPosition.y;
					center.z = (float)num8 * this.CellSize + 0.5f * this.CellSize + this.TerrainPosition.z;
					this.Cells[num10].Center = center;
					for (int num11 = 0; num11 < this.NumberOfLayers; num11++)
					{
						if (this.LayerToMergeWith[num11] == 0)
						{
							num9++;
							for (int num12 = 0; num12 < this.NumberOfBucketsPerCell; num12++)
							{
								for (int num13 = 0; num13 < this.NumberOfBucketsPerCell; num13++)
								{
									int num14 = (int)this.mapByte[num11][num7 * this.NumberOfBucketsPerCell * (int)this.TerrainDetailSize.y + num12 * (int)this.TerrainDetailSize.y + num8 * this.NumberOfBucketsPerCell + num13];
									if (num14 > this.maxBucketDensity)
									{
										this.maxBucketDensity = num14;
									}
									num4 += num14;
								}
							}
							int num15 = 0;
							if (array2[num11] != null)
							{
								for (int num16 = 0; num16 < this.OrigNumberOfLayers - 1; num16++)
								{
									if (array2[num11][num16] == 0)
									{
										break;
									}
									int num17 = array2[num11][num16] - 1;
									int num18 = 0;
									for (int num19 = 0; num19 < this.NumberOfBucketsPerCell; num19++)
									{
										for (int num20 = 0; num20 < this.NumberOfBucketsPerCell; num20++)
										{
											int num14 = (int)this.mapByte[num17][num7 * this.NumberOfBucketsPerCell * (int)this.TerrainDetailSize.y + num19 * (int)this.TerrainDetailSize.y + num8 * this.NumberOfBucketsPerCell + num20];
											num18 += num14;
										}
									}
									if (num18 > 0)
									{
										num15++;
										num4 += num18;
									}
								}
								if (num15 * 16 > this.maxBucketDensity)
								{
									this.maxBucketDensity = num15 * 16 + 32;
								}
							}
							if (num4 > this.realmaxInstancesCount)
							{
								this.realmaxInstancesCount = num4;
							}
							if (num4 > 0)
							{
								this.Cells[num10].CellContentIndexes.Add(num5);
								this.Cells[num10].CellContentCount++;
								GrassCellContent grassCellContent = new GrassCellContent();
								grassCellContent.index = num5;
								grassCellContent.Layer = num11;
								grassCellContent.GrassMatrixBufferPID = this.GrassMatrixBufferPID;
								grassCellContent.Center = center;
								grassCellContent.Pivot = new Vector3((float)num7 * this.CellSize, interpolatedHeight, (float)num8 * this.CellSize);
								grassCellContent.PatchOffsetX = num7 * this.NumberOfBucketsPerCell * (int)this.TerrainDetailSize.y;
								grassCellContent.PatchOffsetZ = num8 * this.NumberOfBucketsPerCell;
								grassCellContent.Instances = num4;
								grassCellContent.RelatedGrassManager = this;
								if (num15 > 0)
								{
									List<int> list2 = new List<int>();
									for (int num21 = 0; num21 < this.OrigNumberOfLayers - 1; num21++)
									{
										if (array2[num11][num21] != 0)
										{
											list2.Add(array2[num11][num21] - 1);
										}
									}
									grassCellContent.SoftlyMergedLayers = list2.ToArray();
								}
								list.Add(grassCellContent);
								num5++;
							}
							num4 = 0;
						}
					}
					if (num9 > this.MaxLayersPerCell)
					{
						this.MaxLayersPerCell = num9;
					}
				}
				num6 += (int)this.TerrainDetailSize.x;
			}
			this.CellContent = list.ToArray();
			list.Clear();
		}

		public void Init()
		{
			this.DetailDensity = this.GetInitializationDensityValue();
			Debug.Log("GrassManager - DetailDensity = " + this.DetailDensity);
			Shader.SetGlobalFloat("_AtgGust", 0f);
			Shader.SetGlobalVector("_AtgWindDirSize", new Vector4(1f, 0f, 0f, 0f));
			Shader.SetGlobalVector("_AtgWindStrengthMultipliers", new Vector4(0f, 0f, 0f, 0f));
			this.ter = base.GetComponent<Terrain>();
			this.terData = this.ter.terrainData;
			this.ter.detailObjectDistance = 0f;
			this.CurrentDetailDensity = this.DetailDensity;
			this.TerrainPosition = this.ter.GetPosition();
			this.TerrainSize = this.terData.size;
			this.OneOverTerrainSize.x = 1f / this.TerrainSize.x;
			this.OneOverTerrainSize.y = 1f / this.TerrainSize.y;
			this.OneOverTerrainSize.z = 1f / this.TerrainSize.z;
			this.TerrainDetailSize.x = (float)this.terData.detailWidth;
			this.TerrainDetailSize.y = (float)this.terData.detailHeight;
			this.BucketSize = this.TerrainSize.x / this.TerrainDetailSize.x;
			this.NumberOfBucketsPerCell = (int)this.NumberOfBucketsPerCellEnum;
			this.CellSize = (float)this.NumberOfBucketsPerCell * this.BucketSize;
			this.NumberOfCells = (int)(this.TerrainSize.x / this.CellSize);
			this.TotalCellCount = this.NumberOfCells * this.NumberOfCells;
			this.sh = Shader.Find("AdvancedTerrainGrass/Grass Base Shader");
			this.GrassMatrixBufferPID = Shader.PropertyToID("GrassMatrixBuffer");
			this.GrassNormalBufferPID = Shader.PropertyToID("GrassNormalBuffer");
			this.GrassFadePropsPID = Shader.PropertyToID("_AtgGrassFadeProps");
			this.GrassShadowFadePropsPID = Shader.PropertyToID("_AtgGrassShadowFadeProps");
			this.CullDistance = this.GetInitializationDistanceValue();
			float cullDistance = this.CullDistance;
			this.CacheDistance = (float)Mathf.CeilToInt(this.CullDistance / this.CellSize) * this.CellSize + Mathf.Sqrt(this.CellSize * this.CellSize * 0.25f);
			this.FadeLength = this.CullDistance * 0.2f;
			this.DetailFadeStart = this.CullDistance;
			this.DetailFadeLength = this.DetailFadeStart * 0.4f;
			Shader.SetGlobalVector(this.GrassFadePropsPID, new Vector4((this.CullDistance - this.FadeLength) * (this.CullDistance - this.FadeLength), 1f / (this.FadeLength * this.FadeLength), this.DetailFadeStart * this.DetailFadeStart, 1f / (this.DetailFadeLength * this.DetailFadeLength)));
			Shader.SetGlobalVector(this.GrassShadowFadePropsPID, new Vector4(this.ShadowStart * this.ShadowStart, 1f / (this.ShadowFadeLength * this.ShadowFadeLength), this.ShadowStartFoliage * this.ShadowStartFoliage, 1f / (this.ShadowFadeLengthFoliage * this.ShadowFadeLengthFoliage)));
			if (this.SavedTerrainData != null)
			{
				this.InitCellsFast();
				this.TotalCellCount = this.Cells.Length;
			}
			else
			{
				this.InitCells();
			}
			this.TerrainHeightmapWidth = this.terData.heightmapWidth;
			this.TerrainHeightmapHeight = this.terData.heightmapHeight;
			this.TerrainHeights = new float[this.TerrainHeightmapWidth * this.TerrainHeightmapHeight];
			for (int i = 0; i < this.TerrainHeightmapWidth; i++)
			{
				for (int j = 0; j < this.TerrainHeightmapHeight; j++)
				{
					this.TerrainHeights[i * this.TerrainHeightmapWidth + j] = this.terData.GetHeight(i, j);
				}
			}
			this.OneOverHeightmapWidth = 1f / (float)this.TerrainHeightmapWidth;
			this.OneOverHeightmapHeight = 1f / (float)this.TerrainHeightmapHeight;
			this.TerrainSizeOverHeightmap = this.TerrainSize.x / (float)this.TerrainHeightmapWidth;
			this.OneOverHeightmapWidthRight = this.TerrainSize.x - 2f * (this.TerrainSize.x / (float)(this.TerrainHeightmapWidth - 1)) - 1f;
			this.OneOverHeightmapHeightUp = this.TerrainSize.z - 2f * (this.TerrainSize.z / (float)(this.TerrainHeightmapHeight - 1)) - 1f;
			this.cullingGroup = new CullingGroup();
			this.cullingGroup.targetCamera = Camera.main;
			this.boundingSpheres = new BoundingSphere[this.TotalCellCount];
			this.resultIndices = new int[this.TotalCellCount];
			this.isVisibleBoundingSpheres = new bool[this.TotalCellCount];
			for (int k = 0; k < this.TotalCellCount; k++)
			{
				this.boundingSpheres[k] = new BoundingSphere(this.Cells[k].Center, cullDistance);
				this.isVisibleBoundingSpheres[k] = false;
			}
			this.cullingGroup.SetBoundingSpheres(this.boundingSpheres);
			this.cullingGroup.SetBoundingSphereCount(this.TotalCellCount);
			float[] boundingDistances = new float[]
			{
				this.CullDistance,
				this.CacheDistance
			};
			this.cullingGroup.SetBoundingDistances(boundingDistances);
			this.cullingGroup.onStateChanged = new CullingGroup.StateChanged(this.StateChangedMethod);
			this.SqrTerrainCullingDist = Mathf.Sqrt(this.TerrainSize.x * this.TerrainSize.x + this.TerrainSize.z * this.TerrainSize.z) + this.CullDistance;
			this.SqrTerrainCullingDist *= this.SqrTerrainCullingDist;
			Debug.Log("Max Bucket Density: " + this.maxBucketDensity);
			Debug.Log("MaxLayersPerCell:" + this.MaxLayersPerCell);
			int num = Mathf.CeilToInt((float)(this.NumberOfBucketsPerCell * this.NumberOfBucketsPerCell * (this.maxBucketDensity + 1)) * this.CurrentDetailDensity);
			this.tempMatrixArray = new Matrix4x4[this.NumberOfLayers][];
			for (int l = 0; l < this.NumberOfLayers; l++)
			{
				this.tempMatrixArray[l] = new Matrix4x4[num];
			}
			this.MaxBufferCapacity = num;
			int num2 = Mathf.CeilToInt((this.CacheDistance + this.CellSize) / this.CellSize);
			num2 *= num2;
			int num3 = num2 * this.MaxLayersPerCell;
			num3 = Mathf.CeilToInt((float)num3 * this.PoolScaleFactor);
			this.FreePoolItemsCount = num3;
			this.ListOfFreePoolItems.Clear();
			this.matrixBuffers = new ComputeBuffer[num3];
			this.argsBuffers = new ComputeBuffer[num3];
			this.materialPropertyBlocks = new MaterialPropertyBlock[num3];
			for (int m = 0; m < num3; m++)
			{
				this.ListOfFreePoolItems.Add(m);
				this.matrixBuffers[m] = new ComputeBuffer(num, 64);
				this.argsBuffers[m] = new ComputeBuffer(1, this.args.Length * 4, ComputeBufferType.DrawIndirect);
				this.materialPropertyBlocks[m] = new MaterialPropertyBlock();
			}
			GrassManager.ATGSeed = 264140496u;
			if (this.useBurst)
			{
				this.BurstInit();
			}
			this.WorkerThread = new Thread(new ThreadStart(this.InitCellContentOnThread));
			this.WorkerThread.Start();
		}

		public void SetInitializationDistanceValue(float value)
		{
			this._initDistanceValue = value;
		}

		private float GetInitializationDistanceValue()
		{
			if (this._initDistanceValue <= 0f)
			{
				return TheForestQualitySettings.UserSettings.GrassDistance;
			}
			return this._initDistanceValue;
		}

		public void SetInitializationDensityValue(float value)
		{
			this._initDensityValue = value;
		}

		private float GetInitializationDensityValue()
		{
			if (this._initDensityValue <= 0f)
			{
				return TheForestQualitySettings.UserSettings.GrassDensity;
			}
			return this._initDensityValue;
		}

		public void BurstInit()
		{
			if (this.Cam == null)
			{
				this.Cam = Camera.main;
				if (this.Cam == null)
				{
					return;
				}
			}
			this.CamTransform = this.Cam.transform;
			if ((this.TerrainPosition - this.CamTransform.position).sqrMagnitude > this.SqrTerrainCullingDist)
			{
				return;
			}
			int num = this.Cells.Length;
			for (int i = 0; i < num; i++)
			{
				GrassCell grassCell = this.Cells[i];
				float num2 = Vector3.Distance(this.CamTransform.position, grassCell.Center);
				if (num2 < this.BurstRadius)
				{
					int cellContentCount = grassCell.CellContentCount;
					if (this.FreePoolItemsCount >= cellContentCount)
					{
						for (int j = 0; j < cellContentCount; j++)
						{
							GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[j]];
							int layer = grassCellContent.Layer;
							grassCellContent.v_mat = this.v_mat[layer];
							grassCellContent.v_mesh = this.v_mesh[layer];
							grassCellContent.ShadowCastingMode = this.ShadowCastingMode[layer];
						}
						this.InitCellContent(grassCell.index);
						for (int k = 0; k < cellContentCount; k++)
						{
							GrassCellContent grassCellContent2 = this.CellContent[grassCell.CellContentIndexes[k]];
							if (this.CellContent[grassCell.CellContentIndexes[k]].state != -1)
							{
								int num3 = this.ListOfFreePoolItems[0];
								grassCellContent2.PoolItemIndex = num3;
								this.ListOfFreePoolItems.RemoveAt(0);
								this.FreePoolItemsCount--;
								this.matrixBuffers[num3].SetData(this.tempMatrixArray[k]);
								this.materialPropertyBlocks[num3].SetBuffer(this.GrassMatrixBufferPID, this.matrixBuffers[num3]);
								grassCellContent2.InitCellContent_Delegated();
							}
						}
						grassCell.state = 3;
					}
				}
			}
		}

		private void StateChangedMethod(CullingGroupEvent evt)
		{
			if (evt.isVisible && evt.currentDistance == 0)
			{
				this.isVisibleBoundingSpheres[evt.index] = true;
			}
			else
			{
				this.isVisibleBoundingSpheres[evt.index] = false;
			}
			if (evt.currentDistance == 2)
			{
				GrassCell grassCell = this.Cells[evt.index];
				if (grassCell.state != 2)
				{
					int cellContentCount = grassCell.CellContentCount;
					for (int i = 0; i < cellContentCount; i++)
					{
						GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[i]];
						if (grassCellContent.PoolItemIndex != -1)
						{
							this.ListOfFreePoolItems.Add(this.CellContent[grassCell.CellContentIndexes[i]].PoolItemIndex);
							this.FreePoolItemsCount++;
						}
						this.CellContent[grassCell.CellContentIndexes[i]].ReleaseCellContent();
					}
					if (grassCell.state == 1)
					{
						this.CellsOrCellContentsToInit.Remove(grassCell.index);
					}
					grassCell.state = 0;
				}
			}
		}

		private void DrawGrass()
		{
			if (this.Cam == null)
			{
				this.Cam = Camera.main;
				if (this.Cam == null)
				{
					return;
				}
			}
			if (!this.Cam.isActiveAndEnabled)
			{
				return;
			}
			this.CamTransform = this.Cam.transform;
			this.cullingGroup.targetCamera = this.Cam;
			if ((this.TerrainPosition - this.CamTransform.position).sqrMagnitude > this.SqrTerrainCullingDist)
			{
				return;
			}
			if (this.CameraSelection == GrassCameras.AllCameras)
			{
				this.CameraInWichGrassWillBeDrawn = null;
			}
			else
			{
				this.CameraInWichGrassWillBeDrawn = this.Cam;
			}
			this.cullingGroup.SetDistanceReferencePoint(this.CamTransform.position);
			if (this.IngnoreOcclusion)
			{
				this.numResults = this.cullingGroup.QueryIndices(0, this.resultIndices, 0);
			}
			else
			{
				this.numResults = this.cullingGroup.QueryIndices(true, 0, this.resultIndices, 0);
			}
			if (this.numResults == this.TotalCellCount)
			{
				return;
			}
			if (this.rebuildtempMatrixArray && !this.ThreadIsRunning)
			{
				this.rebuildtempMatrixArray = false;
				this.numResults = 0;
				for (int i = 0; i < this.Cells.Length; i++)
				{
					GrassCell grassCell = this.Cells[i];
					int cellContentCount = grassCell.CellContentCount;
					grassCell.update = false;
					for (int j = 0; j < cellContentCount; j++)
					{
						this.CellContent[grassCell.CellContentIndexes[j]].ReleaseCellContent();
					}
					grassCell.state = 0;
				}
				this.CurrentDetailDensity = this.TargetDetailDensity;
				int num = Mathf.CeilToInt((float)(this.NumberOfBucketsPerCell * this.NumberOfBucketsPerCell * (this.maxBucketDensity + 1)) * this.CurrentDetailDensity);
				this.MaxBufferCapacity = num;
				int num2 = Mathf.CeilToInt(this.CacheDistance / this.CellSize);
				num2 *= num2;
				int num3 = num2 * this.MaxLayersPerCell;
				num3 = Mathf.CeilToInt((float)num3 * this.PoolScaleFactor);
				for (int k = 0; k < this.matrixBuffers.Length; k++)
				{
					if (this.matrixBuffers[k] != null)
					{
						this.matrixBuffers[k].Release();
					}
				}
				for (int l = 0; l < this.argsBuffers.Length; l++)
				{
					if (this.argsBuffers[l] != null)
					{
						this.argsBuffers[l].Release();
					}
				}
				this.FreePoolItemsCount = num3;
				this.ListOfFreePoolItems.Clear();
				this.matrixBuffers = new ComputeBuffer[num3];
				this.argsBuffers = new ComputeBuffer[num3];
				this.materialPropertyBlocks = new MaterialPropertyBlock[num3];
				for (int m = 0; m < num3; m++)
				{
					this.ListOfFreePoolItems.Add(m);
					this.matrixBuffers[m] = new ComputeBuffer(num, 64);
					this.argsBuffers[m] = new ComputeBuffer(1, this.args.Length * 4, ComputeBufferType.DrawIndirect);
					this.materialPropertyBlocks[m] = new MaterialPropertyBlock();
				}
				this.tempMatrixArray = new Matrix4x4[this.NumberOfLayers][];
				for (int n = 0; n < this.NumberOfLayers; n++)
				{
					this.tempMatrixArray[n] = new Matrix4x4[num];
				}
				if (this.useBurst)
				{
					this.BurstInit();
				}
			}
			this.numOfVisibleCells = 0;
			for (int num4 = 0; num4 < this.numResults; num4++)
			{
				if (!this.IngnoreOcclusion || this.isVisibleBoundingSpheres[this.resultIndices[num4]])
				{
					this.numOfVisibleCells++;
					GrassCell grassCell = this.Cells[this.resultIndices[num4]];
					int state = grassCell.state;
					int cellContentCount = grassCell.CellContentCount;
					switch (state)
					{
					case 0:
						if (grassCell.CellContentCount > 0)
						{
							grassCell.state = 1;
							this.CellsOrCellContentsToInit.Add(grassCell.index);
						}
						break;
					case 3:
						for (int num5 = 0; num5 < cellContentCount; num5++)
						{
							if (this.CellContent[grassCell.CellContentIndexes[num5]].state != -1)
							{
								this.CellContent[grassCell.CellContentIndexes[num5]].DrawCellContent_Delegated(this.CameraInWichGrassWillBeDrawn, this.CameraLayer);
							}
						}
						break;
					case 4:
						if (grassCell.CellContentCount > 0)
						{
							for (int num6 = 0; num6 < cellContentCount; num6++)
							{
								if (this.CellContent[grassCell.CellContentIndexes[num6]].state != -1)
								{
									this.CellContent[grassCell.CellContentIndexes[num6]].DrawCellContent_Delegated(this.CameraInWichGrassWillBeDrawn, this.CameraLayer);
								}
							}
							grassCell.update = true;
							grassCell.state = 5;
							this.CellsOrCellContentsToInit.Insert(0, grassCell.index);
						}
						break;
					case 5:
						if (grassCell.CellContentCount > 0)
						{
							for (int num7 = 0; num7 < cellContentCount; num7++)
							{
								if (this.CellContent[grassCell.CellContentIndexes[num7]].state != -1)
								{
									this.CellContent[grassCell.CellContentIndexes[num7]].DrawCellContent_Delegated(this.CameraInWichGrassWillBeDrawn, this.CameraLayer);
								}
							}
						}
						break;
					}
				}
			}
			if (this.CellToInit != -1)
			{
				GrassCell grassCell = this.Cells[this.CellToInit];
				if (grassCell.state == 2)
				{
					int cellContentCount = grassCell.CellContentCount;
					if (!grassCell.update)
					{
						if (this.FreePoolItemsCount >= cellContentCount)
						{
							for (int num8 = 0; num8 < cellContentCount; num8++)
							{
								GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[num8]];
								if (grassCellContent.state != -1)
								{
									int num9 = this.ListOfFreePoolItems[0];
									grassCellContent.PoolItemIndex = num9;
									this.ListOfFreePoolItems.RemoveAt(0);
									this.FreePoolItemsCount--;
									this.matrixBuffers[num9].SetData(this.tempMatrixArray[num8]);
									this.materialPropertyBlocks[num9].SetBuffer(this.GrassMatrixBufferPID, this.matrixBuffers[num9]);
									grassCellContent.InitCellContent_Delegated();
									grassCellContent.DrawCellContent_Delegated(this.CameraInWichGrassWillBeDrawn, this.CameraLayer);
								}
							}
							this.DataCopied = true;
							this.CellToInit = -1;
							grassCell.state = 3;
						}
					}
					else
					{
						for (int num10 = 0; num10 < cellContentCount; num10++)
						{
							GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[num10]];
							if (grassCellContent.state != -1)
							{
								int poolItemIndex = grassCellContent.PoolItemIndex;
								this.matrixBuffers[poolItemIndex].SetData(this.tempMatrixArray[num10]);
								this.materialPropertyBlocks[poolItemIndex].SetBuffer(this.GrassMatrixBufferPID, this.matrixBuffers[poolItemIndex]);
								grassCellContent.UpdateCellContent_Delegated();
								grassCellContent.DrawCellContent_Delegated(this.CameraInWichGrassWillBeDrawn, this.CameraLayer);
							}
						}
						grassCell.update = false;
						this.DataCopied = true;
						this.CellToInit = -1;
						grassCell.state = 3;
					}
				}
			}
			int count = this.CellsOrCellContentsToInit.Count;
			if (count > 0 && !this.ThreadIsRunning)
			{
				if (this.Cells[this.CellsOrCellContentsToInit[0]].state != 1 && this.Cells[this.CellsOrCellContentsToInit[0]].state != 5)
				{
					this.CellsOrCellContentsToInit.RemoveAt(0);
					if (count == 1)
					{
						this.DataCopied = true;
						return;
					}
				}
				if (this.DataCopied)
				{
					GrassCell grassCell = this.Cells[this.CellsOrCellContentsToInit[0]];
					int cellContentCount = grassCell.CellContentCount;
					for (int num11 = 0; num11 < cellContentCount; num11++)
					{
						GrassCellContent grassCellContent = this.CellContent[grassCell.CellContentIndexes[num11]];
						int layer = grassCellContent.Layer;
						grassCellContent.v_mat = this.v_mat[layer];
						grassCellContent.v_mesh = this.v_mesh[layer];
						grassCellContent.ShadowCastingMode = this.ShadowCastingMode[layer];
					}
					this.CellToInit = this.CellsOrCellContentsToInit[0];
					this.DataCopied = false;
					if (this.UseThreading)
					{
						this.wt_cellindex = this.CellsOrCellContentsToInit[0];
						this.WorkerThreadWait.Set();
					}
					else
					{
						this.InitCellContent(this.CellsOrCellContentsToInit[0]);
					}
					this.CellsOrCellContentsToInit.RemoveAt(0);
				}
			}
		}

		public float GetfilteredHeight(float normalizedHeightPos_x, float normalizedHeightPos_y)
		{
			int num = (int)normalizedHeightPos_x;
			int num2 = (int)normalizedHeightPos_y;
			int num3 = (int)(normalizedHeightPos_x + 1f);
			int num4 = (int)(normalizedHeightPos_y + 1f);
			float num5 = normalizedHeightPos_x - (float)num;
			float num6 = (float)num3 - normalizedHeightPos_x;
			float num7 = normalizedHeightPos_y - (float)num2;
			float num8 = (float)num4 - normalizedHeightPos_y;
			num *= this.TerrainHeightmapHeight;
			num3 *= this.TerrainHeightmapHeight;
			float num9 = this.TerrainHeights[num + num2] * num6;
			num9 += this.TerrainHeights[num3 + num2] * num5;
			float num10 = this.TerrainHeights[num + num4] * num6;
			num10 += this.TerrainHeights[num3 + num4] * num5;
			return num9 * num8 + num10 * num7;
		}

		private void InitCellContentOnThread()
		{
			this.WorkerThreadWait.Reset();
			this.WorkerThreadWait.WaitOne();
			for (;;)
			{
				this.WorkerThreadWait.Reset();
				this.ThreadIsRunning = true;
				this.InitCellContent(this.wt_cellindex);
				this.ThreadIsRunning = false;
				WaitHandle.SignalAndWait(this.MainThreadWait, this.WorkerThreadWait);
			}
		}

		public float InitCellContent(int cellIndex)
		{
			if (cellIndex >= this.Cells.SafeCount<GrassCell>())
			{
				return 0f;
			}
			GrassCell grassCell = this.Cells[cellIndex];
			int a = 0;
			int cellContentCount = grassCell.CellContentCount;
			int num = 0;
			for (int i = 0; i < cellContentCount; i++)
			{
				int num2 = grassCell.CellContentIndexes[i];
				GrassCellContent grassCellContent = this.CellContent[num2];
				int num3 = grassCellContent.Layer;
				int num4 = 0;
				this.samplePosition.x = grassCellContent.Pivot.x;
				this.samplePosition.y = grassCellContent.Pivot.z;
				this.tempSamplePosition = this.samplePosition;
				int num5 = (int)this.InstanceRotation[num3];
				bool flag = this.WriteNormalBuffer[num3];
				float num6 = this.Noise[num3];
				float num7 = this.MinSize[num3];
				float num8 = this.MaxSize[num3] - num7;
				int num9 = 1;
				if (grassCellContent.SoftlyMergedLayers != null)
				{
					num9 += grassCellContent.SoftlyMergedLayers.Length;
				}
				for (int j = 0; j < num9; j++)
				{
					this.tempSamplePosition = this.samplePosition;
					if (j > 0)
					{
						num3 = grassCellContent.SoftlyMergedLayers[j - 1];
						num6 = this.Noise[num3];
						num7 = this.MinSize[num3];
						num8 = this.MaxSize[num3] - num7;
					}
					int num10 = cellIndex + num3 + j * 9949;
					for (int k = 0; k < this.NumberOfBucketsPerCell; k++)
					{
						for (int l = 0; l < this.NumberOfBucketsPerCell; l++)
						{
							GrassManager.ATGSeed = (uint)((float)(num10 - (k * this.NumberOfBucketsPerCell + l)) * 0.0001f * 2.14748365E+09f);
							Vector2 vector;
							vector.x = this.tempSamplePosition.x;
							vector.y = this.tempSamplePosition.y;
							int num11 = (int)this.mapByte[num3][grassCellContent.PatchOffsetX + grassCellContent.PatchOffsetZ + k * (int)this.TerrainDetailSize.y + l];
							num11 = (int)((float)num11 * this.CurrentDetailDensity);
							float num12 = (vector.x >= this.TerrainSizeOverHeightmap) ? this.OneOverHeightmapWidth : 0f;
							float num13 = (vector.x < this.OneOverHeightmapWidthRight) ? this.OneOverHeightmapWidth : 0f;
							float num14 = (vector.y >= this.TerrainSizeOverHeightmap) ? this.OneOverHeightmapHeight : 0f;
							float num15 = (vector.y < this.OneOverHeightmapHeightUp) ? this.OneOverHeightmapHeight : 0f;
							for (int m = 0; m < num11; m++)
							{
								float atgrandNext = GrassManager.GetATGRandNext();
								float num16 = atgrandNext * this.BucketSize;
								atgrandNext = GrassManager.GetATGRandNext();
								float num17 = atgrandNext * this.BucketSize;
								Vector2 vector2;
								vector2.x = (vector.x + num16) * this.OneOverTerrainSize.x;
								vector2.y = (vector.y + num17) * this.OneOverTerrainSize.z;
								float num18 = vector2.x * (float)(this.TerrainHeightmapWidth - 1);
								float num19 = vector2.y * (float)(this.TerrainHeightmapHeight - 1);
								int num20 = (int)num18;
								int num21 = (int)num19;
								int num22 = (int)(num18 + 1f);
								int num23 = (int)(num19 + 1f);
								float num24 = num18 - (float)num20;
								float num25 = (float)num22 - num18;
								float num26 = num19 - (float)num21;
								float num27 = (float)num23 - num19;
								num20 *= this.TerrainHeightmapHeight;
								num22 *= this.TerrainHeightmapHeight;
								float num28 = this.TerrainHeights[num20 + num21] * num25;
								num28 += this.TerrainHeights[num22 + num21] * num24;
								float num29 = this.TerrainHeights[num20 + num23] * num25;
								num29 += this.TerrainHeights[num22 + num23] * num24;
								this.tempPosition.y = num28 * num27 + num29 * num26;
								float num30 = this.GetfilteredHeight((vector2.x - num12) * (float)(this.TerrainHeightmapWidth - 1), vector2.y * (float)(this.TerrainHeightmapHeight - 1));
								float num31 = this.GetfilteredHeight((vector2.x + num13) * (float)(this.TerrainHeightmapWidth - 1), vector2.y * (float)(this.TerrainHeightmapHeight - 1));
								float num32 = this.GetfilteredHeight(vector2.x * (float)(this.TerrainHeightmapWidth - 1), (vector2.y + num15) * (float)(this.TerrainHeightmapHeight - 1));
								float num33 = this.GetfilteredHeight(vector2.x * (float)(this.TerrainHeightmapWidth - 1), (vector2.y - num14) * (float)(this.TerrainHeightmapHeight - 1));
								Vector3 vector3;
								vector3.x = -2f * (num31 - num30);
								if (num5 != 2 && num5 != 4)
								{
									vector3.y = 6.283184f * this.TerrainSizeOverHeightmap;
								}
								else
								{
									vector3.y = 4f * this.TerrainSizeOverHeightmap;
								}
								vector3.z = (num32 - num33) * -2f;
								float num34 = vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z;
								float num35 = (float)Math.Sqrt((double)num34);
								float num36 = 1f / num35;
								vector3.x *= num36;
								vector3.y *= num36;
								vector3.z *= num36;
								this.tempPosition.x = this.tempSamplePosition.x + num16 + this.TerrainPosition.x;
								this.tempPosition.z = this.tempSamplePosition.y + num17 + this.TerrainPosition.z;
								float num37 = num7 + Mathf.PerlinNoise(this.tempPosition.x * num6, this.tempPosition.z * num6) * num8;
								this.tempScale.x = num37;
								this.tempScale.y = num37;
								this.tempScale.z = num37;
								Quaternion zeroQuat = this.ZeroQuat;
								if (num5 != 2)
								{
									zeroQuat.x = vector3.z;
									zeroQuat.y = 0f;
									zeroQuat.z = -vector3.x;
									zeroQuat.w = (float)Math.Sqrt((double)(1f + vector3.y));
									float num38 = (float)(1.0 / Math.Sqrt((double)(zeroQuat.w * zeroQuat.w + zeroQuat.x * zeroQuat.x + zeroQuat.y * zeroQuat.y + zeroQuat.z * zeroQuat.z)));
									zeroQuat.w *= num38;
									zeroQuat.x *= num38;
									zeroQuat.y *= num38;
									zeroQuat.z *= num38;
								}
								float num39 = GrassManager.GetATGRandNext() * 180f;
								float num40 = (float)Math.Cos((double)num39);
								float num41 = (float)Math.Sin((double)num39);
								Quaternion quaternion = zeroQuat;
								zeroQuat.x = quaternion.x * num40 - quaternion.z * num41;
								zeroQuat.y = quaternion.w * num41 + quaternion.y * num40;
								zeroQuat.z = quaternion.z * num40 + quaternion.x * num41;
								zeroQuat.w = quaternion.w * num40 - quaternion.y * num41;
								if (num5 == 1)
								{
									num39 = GrassManager.GetATGRandNext() * 180f;
									float num42 = (float)Math.Sin((double)num39);
									float num43 = (float)Math.Cos((double)num39);
									quaternion = zeroQuat;
									zeroQuat.x = quaternion.w * num42 + quaternion.x * num43;
									zeroQuat.y = quaternion.y * num43 + quaternion.z * num42;
									zeroQuat.z = quaternion.z * num43 - quaternion.y * num42;
									zeroQuat.w = quaternion.w * num43 - quaternion.x * num42;
								}
								this.tempMatrix.m03 = this.tempPosition.x;
								this.tempMatrix.m13 = this.tempPosition.y + this.TerrainPosition.y;
								this.tempMatrix.m23 = this.tempPosition.z;
								float num44 = 2f * zeroQuat.x * zeroQuat.x;
								float num45 = 2f * zeroQuat.y * zeroQuat.y;
								float num46 = 2f * zeroQuat.z * zeroQuat.z;
								this.tempMatrix.m00 = 1f - num45 - num46;
								this.tempMatrix.m01 = 2f * zeroQuat.x * zeroQuat.y - 2f * zeroQuat.z * zeroQuat.w;
								this.tempMatrix.m02 = 2f * zeroQuat.x * zeroQuat.z + 2f * zeroQuat.y * zeroQuat.w;
								this.tempMatrix.m10 = 2f * zeroQuat.x * zeroQuat.y + 2f * zeroQuat.z * zeroQuat.w;
								this.tempMatrix.m11 = 1f - num44 - num46;
								this.tempMatrix.m12 = 2f * zeroQuat.y * zeroQuat.z - 2f * zeroQuat.x * zeroQuat.w;
								this.tempMatrix.m20 = 2f * zeroQuat.x * zeroQuat.z - 2f * zeroQuat.y * zeroQuat.w;
								this.tempMatrix.m21 = 2f * zeroQuat.y * zeroQuat.z + 2f * zeroQuat.x * zeroQuat.w;
								this.tempMatrix.m22 = 1f - num44 - num45;
								this.tempMatrix.m00 = this.tempMatrix.m00 * this.tempScale.x;
								this.tempMatrix.m01 = this.tempMatrix.m01 * this.tempScale.y;
								this.tempMatrix.m02 = this.tempMatrix.m02 * this.tempScale.z;
								this.tempMatrix.m10 = this.tempMatrix.m10 * this.tempScale.x;
								this.tempMatrix.m11 = this.tempMatrix.m11 * this.tempScale.y;
								this.tempMatrix.m12 = this.tempMatrix.m12 * this.tempScale.z;
								this.tempMatrix.m20 = this.tempMatrix.m20 * this.tempScale.x;
								this.tempMatrix.m21 = this.tempMatrix.m21 * this.tempScale.y;
								this.tempMatrix.m22 = this.tempMatrix.m22 * this.tempScale.z;
								if (num5 == 2 && flag)
								{
									Vector3 vector4 = vector3;
									float num47 = -num39;
									num40 = (float)Math.Cos((double)num47);
									num41 = (float)Math.Sin((double)num47);
									float num48 = num41 * 2f;
									float num49 = num41 * num48;
									float num50 = num40 * num48;
									vector4.x = (1f - num49) * vector3.x + num50 * vector3.z;
									vector4.z = -num50 * vector3.x + (1f - num49) * vector3.z;
									this.tempMatrix.m30 = vector4.x;
									this.tempMatrix.m31 = vector4.y;
									this.tempMatrix.m32 = vector4.z;
								}
								else
								{
									this.tempMatrix.m30 = 0f;
									this.tempMatrix.m31 = 0f;
									this.tempMatrix.m32 = 0f;
								}
								this.tempMatrix.m33 = (float)j + this.tempScale.x * 0.01f;
								if (num4 < this.tempMatrixArray[i].SafeCount<Matrix4x4>())
								{
									this.tempMatrixArray[i][num4] = this.tempMatrix;
								}
								else
								{
									a = Mathf.Max(a, num4 + 1);
									num++;
								}
								num4++;
							}
							this.tempSamplePosition.y = this.tempSamplePosition.y + this.BucketSize;
						}
						this.tempSamplePosition.y = this.samplePosition.y;
						this.tempSamplePosition.x = this.tempSamplePosition.x + this.BucketSize;
					}
				}
				if (num4 == 0)
				{
					grassCellContent.state = -1;
				}
				else
				{
					grassCellContent.Instances = num4;
				}
			}
			grassCell.state = 2;
			return 1f;
		}

		public Camera Cam;

		public bool IngnoreOcclusion = true;

		private Transform CamTransform;

		public bool showGrid;

		private GameObject go;

		public Material ProjMat;

		public static uint ATGSeed;

		public static float OneOverInt32MaxVal = 2.32830644E-10f;

		public Terrain ter;

		public TerrainData terData;

		public GrassTerrainDefinitions SavedTerrainData;

		public bool useBurst;

		public float BurstRadius = 50f;

		public float DetailDensity = 1f;

		private float CurrentDetailDensity;

		private float TargetDetailDensity;

		private Vector3 TerrainPosition;

		private Vector3 TerrainSize;

		private Vector3 OneOverTerrainSize;

		private Vector2 TerrainDetailSize;

		private float SqrTerrainCullingDist;

		public bool UseThreading = true;

		public bool ThreadIsRunning;

		public bool DataCopied = true;

		public int CellToInit = -1;

		private Thread WorkerThread;

		private EventWaitHandle WorkerThreadWait = new EventWaitHandle(true, EventResetMode.ManualReset);

		private EventWaitHandle MainThreadWait = new EventWaitHandle(true, EventResetMode.ManualReset);

		private int wt_layer = -1;

		private int wt_index;

		private int wt_cellindex;

		private bool wt_b_initCompleteCell = true;

		private int TerrainHeightmapWidth;

		private int TerrainHeightmapHeight;

		private float[] TerrainHeights;

		private float OneOverHeightmapWidth;

		private float OneOverHeightmapHeight;

		private float TerrainSizeOverHeightmap;

		private float OneOverHeightmapWidthRight;

		private float OneOverHeightmapHeightUp;

		public GrassCameras CameraSelection;

		private Camera CameraInWichGrassWillBeDrawn;

		public int CameraLayer;

		private CullingGroup cullingGroup;

		private BoundingSphere[] boundingSpheres;

		private bool[] isVisibleBoundingSpheres;

		private int numResults;

		private int numOfVisibleCells;

		private int[] resultIndices;

		public float CullDistance = 80f;

		public float FadeLength = 20f;

		public float CacheDistance = 120f;

		public float DetailFadeStart = 20f;

		public float DetailFadeLength = 30f;

		public float ShadowStart = 10f;

		public float ShadowFadeLength = 20f;

		public float ShadowStartFoliage = 30f;

		public float ShadowFadeLengthFoliage = 20f;

		[Space(12f)]
		private int NumberOfLayers;

		private int OrigNumberOfLayers;

		public Mesh[] v_mesh;

		public Material[] v_mat;

		public RotationMode[] InstanceRotation;

		public bool[] WriteNormalBuffer;

		public ShadowCastingMode[] ShadowCastingMode;

		public float[] MinSize;

		public float[] MaxSize;

		public float[] Noise;

		public int[] LayerToMergeWith;

		public bool[] DoSoftMerge;

		private int[][] SoftlyMergedLayers;

		private byte[][] mapByte;

		private int GrassMatrixBufferPID;

		private int GrassNormalBufferPID;

		private int TotalCellCount;

		private int NumberOfCells;

		public BucketsPerCell NumberOfBucketsPerCellEnum = BucketsPerCell._16x16;

		private int NumberOfBucketsPerCell;

		private float CellSize;

		private float BucketSize;

		private int maxBucketDensity;

		public Matrix4x4[][] tempMatrixArray;

		public bool rebuildtempMatrixArray;

		private Vector2 samplePosition;

		private Vector2 tempSamplePosition;

		private GrassCell[] Cells;

		private GrassCellContent[] CellContent;

		private List<int> CellsOrCellContentsToInit = new List<int>();

		public float PoolScaleFactor = 2f;

		public ComputeBuffer[] matrixBuffers;

		public ComputeBuffer[] argsBuffers;

		public uint[] args = new uint[5];

		public MaterialPropertyBlock[] materialPropertyBlocks;

		private int MaxLayersPerCell;

		private int MaxBufferCapacity;

		private int realmaxInstancesCount;

		private int FreePoolItemsCount;

		private List<int> ListOfFreePoolItems = new List<int>();

		private int bufferstofree;

		private Shader sh;

		private int GrassFadePropsPID;

		private Vector4 GrassFadeProps;

		private int GrassShadowFadePropsPID;

		private Vector2 GrassShadowFadeProps;

		private int[] CurrentlyAffectedCells = new int[]
		{
			-1,
			-1,
			-1,
			-1
		};

		private Matrix4x4 tempMatrix = Matrix4x4.identity;

		private Vector3 tempPosition;

		private Quaternion tempRotation;

		private Vector3 tempScale;

		private Quaternion ZeroQuat = Quaternion.identity;

		public bool DebugStats;

		public bool DebugCells;

		public bool FirstTimeSynced;

		public int LayerEditMode;

		public int LayerSelection;

		public bool Foldout_Rendersettings = true;

		public bool Foldout_Prototypes = true;

		private float _initDensityValue;

		private float _initDistanceValue;
	}
}
