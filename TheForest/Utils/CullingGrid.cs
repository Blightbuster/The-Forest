using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class CullingGrid : MonoBehaviour
	{
		
		
		
		public static CullingGrid Instance { get; private set; }

		
		private void Awake()
		{
			if (CullingGrid.Instance != this)
			{
				CullingGrid.Instance = this;
				this._offset = new Vector2((float)(-(float)this._gridSize / 2) * this._gridWorldSize, (float)(-(float)this._gridSize / 2) * this._gridWorldSize);
				this._rendererGrid = new CullingGrid.Cell[this._gridSize, this._gridSize];
				for (int i = 0; i < this._gridSize; i++)
				{
					for (int j = 0; j < this._gridSize; j++)
					{
						CullingGrid.Cell cell = new CullingGrid.Cell();
						cell._cellCenter = this.GridCenterToWorld(i, j);
						this._rendererGrid[i, j] = cell;
					}
				}
				this._activeCells = new List<CullingGrid.Cell>();
				this._workCells = new List<CullingGrid.Cell>();
				this._circlePosition = Vector2.zero;
			}
		}

		
		private void Update()
		{
			if (LocalPlayer.Transform)
			{
				Vector3 position = LocalPlayer.Transform.position;
				this._circlePosition.x = position.x;
				this._circlePosition.y = position.z;
				Vector2 vector = new Vector2(this._gridWorldSize, this._gridWorldSize);
				float magnitude = vector.magnitude;
				float num = this._onRadius + magnitude;
				int num2 = Mathf.Clamp(this.WorldToGridXRounded(this._circlePosition.x - num), 0, this._gridSize);
				int num3 = Mathf.Clamp(this.WorldToGridXRounded(this._circlePosition.x + num) + 1, 0, this._gridSize);
				int num4 = Mathf.Clamp(this.WorldToGridYRounded(this._circlePosition.y - num), 0, this._gridSize);
				int num5 = Mathf.Clamp(this.WorldToGridYRounded(this._circlePosition.y + num) + 1, 0, this._gridSize);
				this._frameCount++;
				for (int i = num2; i < num3; i++)
				{
					for (int j = num4; j < num5; j++)
					{
						CullingGrid.Cell cell = this._rendererGrid[i, j];
						cell._distance = Vector2.Distance(cell._cellCenter, this._circlePosition);
						bool flag = cell._distance <= num;
						if (flag)
						{
							cell._activeAt = this._frameCount;
							if (!cell._enabled)
							{
								this._activeCells.Add(cell);
							}
						}
					}
				}
				foreach (CullingGrid.Cell cell2 in this._activeCells)
				{
					cell2._distance = Vector2.Distance(cell2._cellCenter, this._circlePosition);
				}
				this._activeCells.Sort((CullingGrid.Cell x, CullingGrid.Cell y) => x._distance.CompareTo(y._distance));
				foreach (CullingGrid.Cell cell3 in this._activeCells)
				{
					bool flag2 = cell3._activeAt == this._frameCount;
					if (cell3._enabled != flag2)
					{
						List<Renderer> renderers = cell3._renderers;
						int count = renderers.Count;
						for (int k = 0; k < count; k++)
						{
							renderers[k].enabled = flag2;
						}
						cell3._enabled = flag2;
					}
				}
				this._activeCells.RemoveAll((CullingGrid.Cell c) => !c._enabled);
			}
		}

		
		private void OnEnable()
		{
			for (int i = 0; i < this._gridSize; i++)
			{
				for (int j = 0; j < this._gridSize; j++)
				{
					CullingGrid.Cell cell = this._rendererGrid[i, j];
					if (cell._enabled)
					{
						List<Renderer> renderers = cell._renderers;
						int count = renderers.Count;
						for (int k = 0; k < count; k++)
						{
							renderers[k].enabled = false;
						}
						cell._enabled = false;
					}
				}
			}
		}

		
		private void OnDisable()
		{
			for (int i = 0; i < this._gridSize; i++)
			{
				for (int j = 0; j < this._gridSize; j++)
				{
					CullingGrid.Cell cell = this._rendererGrid[i, j];
					if (!cell._enabled)
					{
						List<Renderer> renderers = cell._renderers;
						int count = renderers.Count;
						for (int k = 0; k < count; k++)
						{
							renderers[k].enabled = true;
						}
						cell._enabled = true;
					}
				}
			}
		}

		
		private void OnDestroy()
		{
			if (CullingGrid.Instance == this)
			{
				CullingGrid.Instance = null;
			}
		}

		
		public static int Register(Renderer r)
		{
			return CullingGrid.Instance.RegisterInternal(r);
		}

		
		public static void Unregister(Renderer r, int token)
		{
			if (CullingGrid.Instance != null)
			{
				r.enabled = true;
				CullingGrid.Instance.UnregisterInternal(r, token);
			}
		}

		
		private int RegisterInternal(Renderer r)
		{
			Vector3 position = r.transform.position;
			int num = Mathf.Clamp(this.WorldToGridX(position.x), 0, this._gridSize - 1);
			int num2 = Mathf.Clamp(this.WorldToGridY(position.z), 0, this._gridSize - 1);
			CullingGrid.Cell cell = this._rendererGrid[num, num2];
			cell._renderers.Add(r);
			r.enabled = cell._enabled;
			return num * this._gridSize + num2;
		}

		
		private void UnregisterInternal(Renderer r, int token)
		{
			int num = token / this._gridSize;
			int num2 = token - num * this._gridSize;
			this._rendererGrid[num, num2]._renderers.Remove(r);
		}

		
		private int WorldToGridX(float xPosition)
		{
			return Mathf.FloorToInt((xPosition - this._offset.x) / this._gridWorldSize);
		}

		
		private int WorldToGridY(float zPosition)
		{
			return Mathf.FloorToInt((zPosition - this._offset.y) / this._gridWorldSize);
		}

		
		private int WorldToGridXRounded(float xPosition)
		{
			return Mathf.RoundToInt((xPosition - this._offset.x) / this._gridWorldSize);
		}

		
		private int WorldToGridYRounded(float zPosition)
		{
			return Mathf.RoundToInt((zPosition - this._offset.y) / this._gridWorldSize);
		}

		
		private float GridToWorldX(float xPosition)
		{
			return xPosition * this._gridWorldSize + this._offset.x;
		}

		
		private float GridToWorldZ(float yPosition)
		{
			return yPosition * this._gridWorldSize + this._offset.y;
		}

		
		private Vector2 GridCenterToWorld(int x, int y)
		{
			return this._offset + this._gridWorldSize * new Vector2((float)x + 0.5f, (float)y + 0.5f);
		}

		
		public int _gridSize = 25;

		
		public float _gridWorldSize = 75f;

		
		public float _onRadius = 300f;

		
		[Header("Gizmos")]
		public bool _showGrid;

		
		public bool _showRendererDensity;

		
		public float _highestCellRendererCount;

		
		public float _highestCellInRangeRendererCount;

		
		public float _currentlyInRadiusRendererCount;

		
		private Vector2 _offset;

		
		private CullingGrid.Cell[,] _rendererGrid;

		
		private Vector2 _circlePosition;

		
		private List<CullingGrid.Cell> _activeCells;

		
		private List<CullingGrid.Cell> _workCells;

		
		private int _frameCount;

		
		public class Cell
		{
			
			public bool _enabled;

			
			public int _activeAt;

			
			public float _distance;

			
			public Vector2 _cellCenter = Vector2.zero;

			
			public List<Renderer> _renderers = new List<Renderer>();
		}
	}
}
