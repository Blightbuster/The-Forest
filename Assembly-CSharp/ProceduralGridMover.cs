using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

[HelpURL("http://arongranberg.com/astar/docs/class_procedural_grid_mover.php")]
public class ProceduralGridMover : MonoBehaviour
{
	public bool updatingGraph { get; private set; }

	public void Start()
	{
		if (AstarPath.active == null)
		{
			throw new Exception("There is no AstarPath object in the scene");
		}
		this.graph = AstarPath.active.astarData.gridGraph;
		if (this.graph == null)
		{
			throw new Exception("The AstarPath object has no GridGraph");
		}
		this.UpdateGraph();
	}

	private void Update()
	{
		Vector3 a = this.PointToGraphSpace(this.graph.center);
		Vector3 b = this.PointToGraphSpace(this.target.position);
		if (VectorMath.SqrDistanceXZ(a, b) > this.updateDistance * this.updateDistance)
		{
			this.UpdateGraph();
		}
	}

	private Vector3 PointToGraphSpace(Vector3 p)
	{
		return this.graph.inverseMatrix.MultiplyPoint(p);
	}

	public void UpdateGraph()
	{
		if (this.updatingGraph)
		{
			return;
		}
		this.updatingGraph = true;
		IEnumerator ie = this.UpdateGraphCoroutine();
		AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(IWorkItemContext context, bool force)
		{
			if (this.floodFill)
			{
				context.QueueFloodFill();
			}
			if (force)
			{
				while (ie.MoveNext())
				{
				}
			}
			bool flag = !ie.MoveNext();
			if (flag)
			{
				this.updatingGraph = false;
			}
			return flag;
		}));
	}

	private IEnumerator UpdateGraphCoroutine()
	{
		Vector3 dir = this.PointToGraphSpace(this.target.position) - this.PointToGraphSpace(this.graph.center);
		dir.x = Mathf.Round(dir.x);
		dir.z = Mathf.Round(dir.z);
		dir.y = 0f;
		if (dir == Vector3.zero)
		{
			yield break;
		}
		Int2 offset = new Int2(-Mathf.RoundToInt(dir.x), -Mathf.RoundToInt(dir.z));
		this.graph.center += this.graph.matrix.MultiplyVector(dir);
		this.graph.GenerateMatrix();
		if (this.tmp == null || this.tmp.Length != this.graph.nodes.Length)
		{
			this.tmp = new GridNode[this.graph.nodes.Length];
		}
		int width = this.graph.width;
		int depth = this.graph.depth;
		GridNode[] nodes = this.graph.nodes;
		if (Mathf.Abs(offset.x) <= width && Mathf.Abs(offset.y) <= depth)
		{
			for (int i = 0; i < depth; i++)
			{
				int num = i * width;
				int num2 = (i + offset.y + depth) % depth * width;
				for (int j = 0; j < width; j++)
				{
					this.tmp[num2 + (j + offset.x + width) % width] = nodes[num + j];
				}
			}
			yield return null;
			for (int k = 0; k < depth; k++)
			{
				int num3 = k * width;
				for (int l = 0; l < width; l++)
				{
					GridNode gridNode = this.tmp[num3 + l];
					gridNode.NodeInGridIndex = num3 + l;
					nodes[num3 + l] = gridNode;
				}
			}
			IntRect r = new IntRect(0, 0, offset.x, offset.y);
			int minz = r.ymax;
			int maxz = depth;
			if (r.xmin > r.xmax)
			{
				int xmax = r.xmax;
				r.xmax = width + r.xmin;
				r.xmin = width + xmax;
			}
			if (r.ymin > r.ymax)
			{
				int ymax = r.ymax;
				r.ymax = depth + r.ymin;
				r.ymin = depth + ymax;
				minz = 0;
				maxz = r.ymin;
			}
			r = r.Expand(this.graph.erodeIterations + 1);
			r = IntRect.Intersection(r, new IntRect(0, 0, width, depth));
			yield return null;
			for (int m = r.ymin; m < r.ymax; m++)
			{
				for (int n = 0; n < width; n++)
				{
					this.graph.UpdateNodePositionCollision(nodes[m * width + n], n, m, false);
				}
			}
			yield return null;
			for (int num4 = minz; num4 < maxz; num4++)
			{
				for (int num5 = r.xmin; num5 < r.xmax; num5++)
				{
					this.graph.UpdateNodePositionCollision(nodes[num4 * width + num5], num5, num4, false);
				}
			}
			yield return null;
			for (int num6 = r.ymin; num6 < r.ymax; num6++)
			{
				for (int num7 = 0; num7 < width; num7++)
				{
					this.graph.CalculateConnections(num7, num6, nodes[num6 * width + num7]);
				}
			}
			yield return null;
			for (int num8 = minz; num8 < maxz; num8++)
			{
				for (int num9 = r.xmin; num9 < r.xmax; num9++)
				{
					this.graph.CalculateConnections(num9, num8, nodes[num8 * width + num9]);
				}
			}
			yield return null;
			for (int num10 = 0; num10 < depth; num10++)
			{
				for (int num11 = 0; num11 < width; num11++)
				{
					if (num11 == 0 || num10 == 0 || num11 >= width - 1 || num10 >= depth - 1)
					{
						this.graph.CalculateConnections(num11, num10, nodes[num10 * width + num11]);
					}
				}
			}
		}
		else
		{
			for (int num12 = 0; num12 < depth; num12++)
			{
				for (int num13 = 0; num13 < width; num13++)
				{
					this.graph.UpdateNodePositionCollision(nodes[num12 * width + num13], num13, num12, false);
				}
			}
			for (int num14 = 0; num14 < depth; num14++)
			{
				for (int num15 = 0; num15 < width; num15++)
				{
					this.graph.CalculateConnections(num15, num14, nodes[num14 * width + num15]);
				}
			}
		}
		yield return null;
		yield break;
	}

	public float updateDistance = 10f;

	public Transform target;

	public bool floodFill;

	private GridGraph graph;

	private GridNode[] tmp;
}
