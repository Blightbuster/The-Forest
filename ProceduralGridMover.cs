using System;
using System.Collections;
using Pathfinding;
using UnityEngine;


[HelpURL("http:
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
			for (int z = 0; z < depth; z++)
			{
				int pz = z * width;
				int tz = (z + offset.y + depth) % depth * width;
				for (int x = 0; x < width; x++)
				{
					this.tmp[tz + (x + offset.x + width) % width] = nodes[pz + x];
				}
			}
			yield return null;
			for (int z2 = 0; z2 < depth; z2++)
			{
				int pz2 = z2 * width;
				for (int x2 = 0; x2 < width; x2++)
				{
					GridNode node = this.tmp[pz2 + x2];
					node.NodeInGridIndex = pz2 + x2;
					nodes[pz2 + x2] = node;
				}
			}
			IntRect r = new IntRect(0, 0, offset.x, offset.y);
			int minz = r.ymax;
			int maxz = depth;
			if (r.xmin > r.xmax)
			{
				int tmp2 = r.xmax;
				r.xmax = width + r.xmin;
				r.xmin = width + tmp2;
			}
			if (r.ymin > r.ymax)
			{
				int tmp3 = r.ymax;
				r.ymax = depth + r.ymin;
				r.ymin = depth + tmp3;
				minz = 0;
				maxz = r.ymin;
			}
			r = r.Expand(this.graph.erodeIterations + 1);
			r = IntRect.Intersection(r, new IntRect(0, 0, width, depth));
			yield return null;
			for (int z3 = r.ymin; z3 < r.ymax; z3++)
			{
				for (int x3 = 0; x3 < width; x3++)
				{
					this.graph.UpdateNodePositionCollision(nodes[z3 * width + x3], x3, z3, false);
				}
			}
			yield return null;
			for (int z4 = minz; z4 < maxz; z4++)
			{
				for (int x4 = r.xmin; x4 < r.xmax; x4++)
				{
					this.graph.UpdateNodePositionCollision(nodes[z4 * width + x4], x4, z4, false);
				}
			}
			yield return null;
			for (int z5 = r.ymin; z5 < r.ymax; z5++)
			{
				for (int x5 = 0; x5 < width; x5++)
				{
					this.graph.CalculateConnections(x5, z5, nodes[z5 * width + x5]);
				}
			}
			yield return null;
			for (int z6 = minz; z6 < maxz; z6++)
			{
				for (int x6 = r.xmin; x6 < r.xmax; x6++)
				{
					this.graph.CalculateConnections(x6, z6, nodes[z6 * width + x6]);
				}
			}
			yield return null;
			for (int z7 = 0; z7 < depth; z7++)
			{
				for (int x7 = 0; x7 < width; x7++)
				{
					if (x7 == 0 || z7 == 0 || x7 >= width - 1 || z7 >= depth - 1)
					{
						this.graph.CalculateConnections(x7, z7, nodes[z7 * width + x7]);
					}
				}
			}
		}
		else
		{
			for (int z8 = 0; z8 < depth; z8++)
			{
				for (int x8 = 0; x8 < width; x8++)
				{
					this.graph.UpdateNodePositionCollision(nodes[z8 * width + x8], x8, z8, false);
				}
			}
			for (int z9 = 0; z9 < depth; z9++)
			{
				for (int x9 = 0; x9 < width; x9++)
				{
					this.graph.CalculateConnections(x9, z9, nodes[z9 * width + x9]);
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
