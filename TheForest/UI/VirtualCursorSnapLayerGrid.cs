using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class VirtualCursorSnapLayerGrid : VirtualCursorSnapLayer
	{
		
		private void Awake()
		{
			int num = Screen.height / this._hCells;
			int num2 = Screen.width / num;
			int hCells = this._hCells;
			this._grid = new List<VirtualCursorSnapNode>[num2, hCells];
			this._pool = new Queue<List<VirtualCursorSnapNode>>();
		}

		
		public override void RegisterNode(VirtualCursorSnapNode node)
		{
			VirtualCursorSnapNodeGrid virtualCursorSnapNodeGrid = node as VirtualCursorSnapNodeGrid;
			if (virtualCursorSnapNodeGrid)
			{
				int num = Mathf.RoundToInt(virtualCursorSnapNodeGrid._vpMin.x * (float)this._grid.GetLength(0));
				int num2 = Mathf.RoundToInt(virtualCursorSnapNodeGrid._vpMax.x * (float)this._grid.GetLength(0));
				int num3 = Mathf.RoundToInt(virtualCursorSnapNodeGrid._vpMin.y * (float)this._grid.GetLength(1));
				int num4 = Mathf.RoundToInt(virtualCursorSnapNodeGrid._vpMax.y * (float)this._grid.GetLength(1));
				this._nodes.Add(node);
			}
		}

		
		public override void UnregisterNode(VirtualCursorSnapNode node)
		{
			if (node == this._currentNode)
			{
				base.CheckCurrentNode();
			}
			if (node.MasterGroupNode)
			{
				node.MasterGroupNode.GroupedNodes.Remove(node);
				node.MasterGroupNode = null;
				return;
			}
			if (node.GroupedNodes != null && node.GroupedNodes.Count > 0)
			{
				VirtualCursorSnapNode virtualCursorSnapNode = node.GroupedNodes[0];
				virtualCursorSnapNode.MasterGroupNode = null;
				node.GroupedNodes.RemoveAt(0);
				for (int i = 0; i < node.GroupedNodes.Count; i++)
				{
					virtualCursorSnapNode.GroupedNodes.Add(node);
					node.MasterGroupNode = virtualCursorSnapNode;
				}
				node.GroupedNodes.Clear();
				this._nodes.Add(virtualCursorSnapNode);
			}
			this._nodes.Remove(node);
		}

		
		public override void Navigate()
		{
			if (this._nextSnap < Time.realtimeSinceStartup)
			{
				float num = -TheForest.Utils.Input.GetAxis("Mouse X");
				float axis = TheForest.Utils.Input.GetAxis("Mouse Y");
				if (!Mathf.Approximately(num, 0f) || !Mathf.Approximately(axis, 0f))
				{
					VirtualCursorSnapNode nodeAtDir = this.GetNodeAtDir(new Vector3(0f, axis, num));
					base.SetCurrentNode(nodeAtDir);
					if (nodeAtDir)
					{
						if (this._speedUp)
						{
							this._nextSnap = Time.realtimeSinceStartup + this._snapDelay / 2f;
						}
						else
						{
							this._nextSnap = Time.realtimeSinceStartup + this._snapDelay;
							if (this._canSpeedUp)
							{
								this._speedUp = true;
							}
							else
							{
								this._canSpeedUp = true;
							}
						}
					}
				}
				else
				{
					this._canSpeedUp = false;
					this._speedUp = false;
				}
			}
		}

		
		public override VirtualCursorSnapNode GetNodeAtDir(Vector3 myDir)
		{
			Transform transform = base.transform;
			myDir.Normalize();
			bool flag = Mathf.Abs(Mathf.Abs(myDir.y) - Mathf.Abs(myDir.z)) < 0.25f;
			bool flag2 = Mathf.Abs(myDir.y) < Mathf.Abs(myDir.z);
			float num = (!flag && !flag2) ? 2f : 1f;
			float num2 = (!flag && flag2) ? 2f : 1f;
			myDir = transform.TransformDirection(myDir);
			Vector3 b = (!(this._currentNode != null)) ? base.transform.position : base.FlattenPosition(this._currentNode.Center);
			float num3 = float.MaxValue;
			VirtualCursorSnapNode result = null;
			for (int i = 0; i < this._nodes.Count; i++)
			{
				VirtualCursorSnapNode virtualCursorSnapNode;
				if (this._nodes[i].GroupedNodes != null && this._nodes[i].GroupedNodes.Count > 0)
				{
					virtualCursorSnapNode = this._nodes[i].GroupedNodes[this._nodes[i].GroupedNodes.Count - 1];
				}
				else
				{
					virtualCursorSnapNode = this._nodes[i];
				}
				if (virtualCursorSnapNode.CanBeSnapedTo)
				{
					Vector3 direction = base.FlattenPosition(virtualCursorSnapNode.Center) - b;
					float num4 = Vector3.Dot(myDir, direction.normalized);
					if (num4 >= 0.5f)
					{
						direction = transform.InverseTransformDirection(direction);
						direction.x *= num + (1f - num4) * 2.5f;
						direction.y *= num2 + (1f - num4) * 2.5f;
						float sqrMagnitude = direction.sqrMagnitude;
						if (sqrMagnitude <= num3)
						{
							result = virtualCursorSnapNode;
							num3 = sqrMagnitude;
						}
					}
				}
			}
			return result;
		}

		
		private List<VirtualCursorSnapNode> NewCell()
		{
			if (this._pool.Count > 0)
			{
				return this._pool.Dequeue();
			}
			return new List<VirtualCursorSnapNode>();
		}

		
		private void ClearCell(List<VirtualCursorSnapNode> cell)
		{
			if (cell.Count > 0)
			{
				cell.Clear();
			}
			this._pool.Enqueue(cell);
		}

		
		public int _hCells = 25;

		
		public float _gridDepth = 3f;

		
		public Camera _camera;

		
		private List<VirtualCursorSnapNode>[,] _grid;

		
		private Queue<List<VirtualCursorSnapNode>> _pool;
	}
}
