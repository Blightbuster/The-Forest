using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class InsideCheck : MonoBehaviour
	{
		
		private void Start()
		{
			this._listener = base.GetComponentInChildren<FMOD_Listener>().transform;
			InsideCheck.AffirmGrid();
			this._currentGridCells = new List<InsideCheck.GridCell>();
			this._occlusion = new List<bool>(36);
			for (int i = 0; i < 36; i++)
			{
				this._occlusion.Add(false);
			}
			if (FMOD_StudioSystem.instance)
			{
				this._snapshotInstance = FMOD_StudioSystem.instance.GetEvent("snapshot:/inside_buildings");
			}
			base.StartCoroutine(this.UpdateInsideRoutine());
			FMOD_Listener.DrawInsideCheck = new Action(this.DrawDebug);
			InsideCheck._instance = this;
		}

		
		private void OnDestroy()
		{
			FMODCommon.ReleaseIfValid(this._snapshotInstance, STOP_MODE.IMMEDIATE);
			if (InsideCheck._instance == this)
			{
				InsideCheck._instance = null;
			}
		}

		
		private IEnumerator UpdateInsideRoutine()
		{
			for (;;)
			{
				InsideCheck.GridPosition position = InsideCheck.ToGridPosition(this._listener.position);
				if (this._forceRefresh || position != this._currentGridPosition)
				{
					this._forceRefresh = false;
					this._currentGridPosition = position;
					this._currentGridCells.Clear();
					for (int i = -1; i <= 1; i++)
					{
						for (int j = -1; j <= 1; j++)
						{
							InsideCheck.GridCell gridCell = InsideCheck.GetGridCell(position + new InsideCheck.GridPosition(i, j));
							if (gridCell != null)
							{
								this._currentGridCells.Add(gridCell);
							}
						}
					}
				}
				for (int k = 0; k < this._occlusion.Count; k++)
				{
					this._occlusion[k] = false;
				}
				this._roofDetected = InsideCheck.IsAnyRoofAbove(this._listener.position);
				if (this._roofDetected)
				{
					for (int l = 0; l < this._currentGridCells.Count; l++)
					{
						InsideCheck.RenderWallsToOcclusion(this._currentGridCells[l], this._listener.position, this._occlusion);
					}
				}
				float occlusionPercent = 0f;
				for (int m = 0; m < this._occlusion.Count; m++)
				{
					if (this._occlusion[m])
					{
						occlusionPercent += 1f;
					}
				}
				occlusionPercent *= 100f / (float)this._occlusion.Count;
				this.SetInside(occlusionPercent >= this.OCCLUSION_THRESHOLD);
				yield return YieldPresets.WaitPointOneSeconds;
			}
			yield break;
		}

		
		private void SetInside(bool inside)
		{
			if (inside != this._inside)
			{
				this._inside = inside;
				if (inside)
				{
					UnityUtil.ERRCHECK(this._snapshotInstance.start());
				}
				else
				{
					UnityUtil.ERRCHECK(this._snapshotInstance.stop(STOP_MODE.ALLOWFADEOUT));
				}
			}
		}

		
		private static bool IsAnyRoofAbove(Vector3 position)
		{
			InsideCheck.GridCell gridCell = InsideCheck.GetGridCell(InsideCheck.ToGridPosition(position));
			if (gridCell != null && gridCell.roofs != null)
			{
				for (int i = 0; i < gridCell.roofs.Count; i++)
				{
					if (InsideCheck.IsRoofAbove(gridCell.roofs[i], position))
					{
						return true;
					}
				}
			}
			return false;
		}

		
		private static bool IsRoofAbove(IRoof roof, Vector3 position)
		{
			return roof.GetLevel() > position.y && MathEx.IsPointInPolygon(position, roof.GetPolygon());
		}

		
		private static void RenderWallsToOcclusion(InsideCheck.GridCell cell, Vector3 centre, List<bool> occlusion)
		{
			if (cell.walls != null)
			{
				for (int i = 0; i < cell.walls.Count; i++)
				{
					InsideCheck.RenderChunkToOcclusion(cell.walls[i], centre, occlusion);
				}
			}
		}

		
		private static void RenderChunkToOcclusion(InsideCheck.WallChunk chunk, Vector3 centre, List<bool> occlusion)
		{
			if (centre.y < chunk.start.y || chunk.end.y < centre.y)
			{
				return;
			}
			float num = InsideCheck.CalculateAngle(InsideCheck.ToPlanarPosition(chunk.start, centre));
			float num2 = InsideCheck.CalculateAngle(InsideCheck.ToPlanarPosition(chunk.end, centre));
			if (num2 < num)
			{
				float num3 = num;
				num = num2;
				num2 = num3;
			}
			int num4 = InsideCheck.CalculateIndex(num, occlusion.Count);
			int num5 = InsideCheck.CalculateIndex(num2, occlusion.Count);
			float num6 = num2 - num;
			if (num6 <= 3.14159274f)
			{
				for (int i = num4; i <= num5; i++)
				{
					occlusion[i] = true;
				}
			}
			else
			{
				for (int j = 0; j <= num4; j++)
				{
					occlusion[j] = true;
				}
				for (int k = num5; k < occlusion.Count; k++)
				{
					occlusion[k] = true;
				}
			}
		}

		
		private static float CalculateAngle(Vector2 vector)
		{
			float num = Mathf.Atan2(vector.y, vector.x);
			if (num < 0f)
			{
				num += 6.28318548f;
			}
			return num;
		}

		
		private static int CalculateIndex(float angle, int count)
		{
			return Mathf.Clamp(Mathf.FloorToInt(angle / 6.28318548f * (float)count), 0, count - 1);
		}

		
		private static Vector2 ToPlanarPosition(Vector3 worldPosition, Vector3 centre)
		{
			return new Vector2(worldPosition.x - centre.x, worldPosition.z - centre.z);
		}

		
		private static InsideCheck.GridPosition ToGridPosition(Vector3 worldPosition)
		{
			return new InsideCheck.GridPosition(Mathf.FloorToInt(worldPosition.x / 30f), Mathf.FloorToInt(worldPosition.z / 30f));
		}

		
		private static void AffirmGrid()
		{
			if (InsideCheck._grid == null)
			{
				InsideCheck._grid = new Dictionary<InsideCheck.GridPosition, InsideCheck.GridCell>();
			}
		}

		
		public static void ClearStaticVars()
		{
			if (InsideCheck._grid != null)
			{
				InsideCheck._grid.Clear();
				InsideCheck._grid = null;
			}
		}

		
		private static InsideCheck.GridCell AffirmGridCell(InsideCheck.GridPosition position)
		{
			InsideCheck.AffirmGrid();
			InsideCheck.GridCell gridCell = null;
			if (!InsideCheck._grid.TryGetValue(position, out gridCell))
			{
				gridCell = new InsideCheck.GridCell();
				InsideCheck._grid[position] = gridCell;
			}
			return gridCell;
		}

		
		private static List<InsideCheck.WallChunk> AffirmWallList(InsideCheck.GridPosition position)
		{
			InsideCheck.GridCell gridCell = InsideCheck.AffirmGridCell(position);
			if (gridCell.walls == null)
			{
				gridCell.walls = new List<InsideCheck.WallChunk>();
			}
			return gridCell.walls;
		}

		
		private static List<IRoof> AffirmRoofList(InsideCheck.GridPosition position)
		{
			InsideCheck.GridCell gridCell = InsideCheck.AffirmGridCell(position);
			if (gridCell.roofs == null)
			{
				gridCell.roofs = new List<IRoof>();
			}
			return gridCell.roofs;
		}

		
		private static InsideCheck.GridCell GetGridCell(InsideCheck.GridPosition position)
		{
			InsideCheck.GridCell result = null;
			if (InsideCheck._grid != null)
			{
				InsideCheck._grid.TryGetValue(position, out result);
			}
			return result;
		}

		
		private static void ForceRefresh(InsideCheck.GridPosition position)
		{
			if (InsideCheck._instance != null)
			{
				InsideCheck.GridPosition currentGridPosition = InsideCheck._instance._currentGridPosition;
				if (Mathf.Abs(position.x - InsideCheck._instance._currentGridPosition.x) <= 1 && Mathf.Abs(position.y - InsideCheck._instance._currentGridPosition.y) <= 1)
				{
					InsideCheck._instance._forceRefresh = true;
				}
			}
		}

		
		private static void AddWallChunk(InsideCheck.GridPosition position, InsideCheck.WallChunk chunk)
		{
			List<InsideCheck.WallChunk> list = InsideCheck.AffirmWallList(position);
			list.Add(chunk);
			InsideCheck.ForceRefresh(position);
		}

		
		private static void AddRoof(InsideCheck.GridPosition position, IRoof roof)
		{
			List<IRoof> list = InsideCheck.AffirmRoofList(position);
			list.Add(roof);
			InsideCheck.ForceRefresh(position);
		}

		
		public static int AddWallChunk(Vector3 start, Vector3 end, float height)
		{
			if (end.y < start.y)
			{
				float y = start.y;
				start.y = end.y;
				end.y = y;
			}
			end.y += height;
			InsideCheck.WallChunk wallChunk = new InsideCheck.WallChunk(start, end, InsideCheck._nextGridToken);
			InsideCheck._nextGridToken++;
			InsideCheck.GridPosition gridPosition = InsideCheck.ToGridPosition(start);
			InsideCheck.AddWallChunk(gridPosition, wallChunk);
			InsideCheck.GridPosition gridPosition2 = InsideCheck.ToGridPosition(end);
			if (gridPosition2 != gridPosition)
			{
				InsideCheck.AddWallChunk(gridPosition2, wallChunk);
			}
			return wallChunk.token;
		}

		
		public static void RemoveWallChunk(int token)
		{
			if (InsideCheck._grid != null)
			{
				foreach (KeyValuePair<InsideCheck.GridPosition, InsideCheck.GridCell> keyValuePair in InsideCheck._grid)
				{
					InsideCheck.GridCell value = keyValuePair.Value;
					if (value.walls != null)
					{
						for (int i = value.walls.Count - 1; i >= 0; i--)
						{
							if (value.walls[i].token == token)
							{
								value.walls.RemoveAt(i);
							}
						}
						if (value.walls.Count == 0)
						{
							value.walls = null;
						}
					}
				}
			}
		}

		
		public static void AddRoof(IRoof roof)
		{
			List<Vector3> polygon = roof.GetPolygon();
			if (polygon.Count <= 0)
			{
				return;
			}
			Bounds bounds = new Bounds(polygon[0], Vector3.zero);
			for (int i = 1; i < polygon.Count; i++)
			{
				bounds.Encapsulate(polygon[i]);
			}
			InsideCheck.GridPosition gridPosition = InsideCheck.ToGridPosition(bounds.min);
			InsideCheck.GridPosition gridPosition2 = InsideCheck.ToGridPosition(bounds.max);
			for (int j = gridPosition.x; j <= gridPosition2.x; j++)
			{
				for (int k = gridPosition.y; k <= gridPosition2.y; k++)
				{
					InsideCheck.AddRoof(new InsideCheck.GridPosition(j, k), roof);
				}
			}
		}

		
		public static void RemoveRoof(IRoof roof)
		{
			if (InsideCheck._grid != null)
			{
				foreach (KeyValuePair<InsideCheck.GridPosition, InsideCheck.GridCell> keyValuePair in InsideCheck._grid)
				{
					InsideCheck.GridCell value = keyValuePair.Value;
					if (value.roofs != null)
					{
						for (int i = value.roofs.Count - 1; i >= 0; i--)
						{
							if (value.roofs[i] == roof)
							{
								value.roofs.RemoveAt(i);
							}
						}
						if (value.roofs.Count == 0)
						{
							value.roofs = null;
						}
					}
				}
			}
		}

		
		private void CreateDebugTextures()
		{
			InsideCheck.lineTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			InsideCheck.lineTexture.SetPixel(0, 0, Color.white);
			InsideCheck.lineTexture.Apply();
		}

		
		public void DrawDebug()
		{
			if (InsideCheck.lineTexture == null)
			{
				this.CreateDebugTextures();
			}
			if (Camera.main != null)
			{
				Vector2 vector = new Vector2(110f, (float)(Camera.main.pixelHeight - 110));
				Vector2 vector2 = vector - new Vector2(105f, 105f);
				GUI.Box(new Rect(vector2.x, vector2.y - 35f, 210f, 245f), ((!this._inside) ? "OUTSIDE" : "INSIDE") + "\n" + ((!this._roofDetected) ? "NO ROOF" : "ROOF DETECTED"));
				List<InsideCheck.GridCell> currentGridCells = this._currentGridCells;
				for (int i = 0; i < currentGridCells.Count; i++)
				{
					InsideCheck.GridCell gridCell = currentGridCells[i];
					if (gridCell.walls != null)
					{
						float scale = 1.66666663f;
						for (int j = 0; j < gridCell.walls.Count; j++)
						{
							Vector2 start = InsideCheck.ToUIPosition(LocalPlayer.Transform.InverseTransformPoint(gridCell.walls[j].start), vector, scale);
							Vector2 end = InsideCheck.ToUIPosition(LocalPlayer.Transform.InverseTransformPoint(gridCell.walls[j].end), vector, scale);
							this.DrawLine(start, end, InsideCheck.lineTexture);
						}
					}
				}
				Color color = GUI.color;
				float num = this._listener.eulerAngles.y * 0.0174532924f;
				int num2 = 5;
				for (int k = 0; k < this._occlusion.Count; k++)
				{
					float num3 = (float)((!this._occlusion[k]) ? 0 : 1);
					Vector2 vector3 = vector + 100f * new Vector2(Mathf.Cos(num), -Mathf.Sin(num));
					GUI.color = new Color(num3, num3, num3);
					GUI.DrawTexture(new Rect(vector3.x - (float)(num2 / 2), vector3.y - (float)(num2 / 2), (float)num2, (float)num2), InsideCheck.lineTexture);
					num += 6.28318548f / (float)this._occlusion.Count;
				}
				GUI.color = color;
			}
		}

		
		private static Vector2 ToUIPosition(Vector3 localPosition, Vector2 centre, float scale)
		{
			return centre + new Vector2(localPosition.x, -localPosition.z) * scale;
		}

		
		private void DrawLine(Vector2 start, Vector2 end, Texture2D texture)
		{
			Vector2 vector = end - start;
			this.DrawLine(start, 57.29578f * Mathf.Atan2(vector.y, vector.x), vector.magnitude, texture);
		}

		
		private void DrawLine(Vector2 start, float angle, float length, Texture2D texture)
		{
			Matrix4x4 matrix = GUI.matrix;
			GUIUtility.RotateAroundPivot(angle, start);
			GUI.DrawTexture(new Rect(start.x, start.y, length, 1f), texture);
			GUI.matrix = matrix;
		}

		
		private void DrawTexture(Vector2 position, Texture2D texture)
		{
			float num = (float)texture.width;
			float height = (float)texture.height;
			GUI.DrawTexture(new Rect(position.x - num / 2f, position.y - num / 2f, num, height), texture);
		}

		
		private bool _inside;

		
		private EventInstance _snapshotInstance;

		
		private InsideCheck.GridPosition _currentGridPosition;

		
		private List<InsideCheck.GridCell> _currentGridCells;

		
		private bool _forceRefresh;

		
		private bool _roofDetected;

		
		private List<bool> _occlusion;

		
		private Transform _listener;

		
		private static InsideCheck _instance;

		
		private static Dictionary<InsideCheck.GridPosition, InsideCheck.GridCell> _grid;

		
		private static int _nextGridToken;

		
		private const float GRID_SIZE = 30f;

		
		private const int OCCLUSION_ENTRY_COUNT = 36;

		
		private float OCCLUSION_THRESHOLD = 55f;

		
		private static Texture2D lineTexture;

		
		private struct GridPosition : IEquatable<InsideCheck.GridPosition>
		{
			
			public GridPosition(int x, int y)
			{
				this.x = x;
				this.y = y;
			}

			
			public override int GetHashCode()
			{
				return this.x ^ this.y;
			}

			
			public override bool Equals(object other)
			{
				return other.GetType() == base.GetType() && this.Equals((InsideCheck.GridPosition)other);
			}

			
			public bool Equals(InsideCheck.GridPosition other)
			{
				return other.x == this.x && other.y == this.y;
			}

			
			public static bool operator ==(InsideCheck.GridPosition a, InsideCheck.GridPosition b)
			{
				return a.Equals(b);
			}

			
			public static bool operator !=(InsideCheck.GridPosition a, InsideCheck.GridPosition b)
			{
				return !a.Equals(b);
			}

			
			public static InsideCheck.GridPosition operator +(InsideCheck.GridPosition a, InsideCheck.GridPosition b)
			{
				return new InsideCheck.GridPosition(a.x + b.x, a.y + b.y);
			}

			
			public int x;

			
			public int y;
		}

		
		private class WallChunk
		{
			
			public WallChunk(Vector3 start, Vector3 end, int token)
			{
				this.start = start;
				this.end = end;
				this.token = token;
			}

			
			public Vector3 start;

			
			public Vector3 end;

			
			public int token;
		}

		
		private class GridCell
		{
			
			public List<InsideCheck.WallChunk> walls;

			
			public List<IRoof> roofs;
		}
	}
}
