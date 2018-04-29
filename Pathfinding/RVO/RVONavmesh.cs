using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.RVO
{
	
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Navmesh")]
	[HelpURL("http:
	public class RVONavmesh : GraphModifier
	{
		
		public override void OnPostCacheLoad()
		{
			this.OnLatePostScan();
		}

		
		public override void OnLatePostScan()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.RemoveObstacles();
			NavGraph[] graphs = AstarPath.active.graphs;
			RVOSimulator rvosimulator = UnityEngine.Object.FindObjectOfType<RVOSimulator>();
			if (rvosimulator == null)
			{
				throw new NullReferenceException("No RVOSimulator could be found in the scene. Please add one to any GameObject");
			}
			Simulator simulator = rvosimulator.GetSimulator();
			for (int i = 0; i < graphs.Length; i++)
			{
				RecastGraph recastGraph = graphs[i] as RecastGraph;
				if (recastGraph != null)
				{
					foreach (RecastGraph.NavmeshTile ng in recastGraph.GetTiles())
					{
						this.AddGraphObstacles(simulator, ng);
					}
				}
				else
				{
					INavmesh navmesh = graphs[i] as INavmesh;
					if (navmesh != null)
					{
						this.AddGraphObstacles(simulator, navmesh);
					}
				}
			}
			simulator.UpdateObstacles();
		}

		
		public void RemoveObstacles()
		{
			if (this.lastSim == null)
			{
				return;
			}
			Simulator simulator = this.lastSim;
			this.lastSim = null;
			for (int i = 0; i < this.obstacles.Count; i++)
			{
				simulator.RemoveObstacle(this.obstacles[i]);
			}
			this.obstacles.Clear();
		}

		
		public void AddGraphObstacles(Simulator sim, INavmesh ng)
		{
			if (this.obstacles.Count > 0 && this.lastSim != null && this.lastSim != sim)
			{
				Debug.LogError("Simulator has changed but some old obstacles are still added for the previous simulator. Deleting previous obstacles.");
				this.RemoveObstacles();
			}
			this.lastSim = sim;
			int[] uses = new int[20];
			Dictionary<int, int> outline = new Dictionary<int, int>();
			Dictionary<int, Int3> vertexPositions = new Dictionary<int, Int3>();
			HashSet<int> hasInEdge = new HashSet<int>();
			ng.GetNodes(delegate(GraphNode _node)
			{
				TriangleMeshNode triangleMeshNode = _node as TriangleMeshNode;
				uses[0] = (uses[1] = (uses[2] = 0));
				if (triangleMeshNode != null)
				{
					for (int j = 0; j < triangleMeshNode.connections.Length; j++)
					{
						TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[j] as TriangleMeshNode;
						if (triangleMeshNode2 != null)
						{
							int num3 = triangleMeshNode.SharedEdge(triangleMeshNode2);
							if (num3 != -1)
							{
								uses[num3] = 1;
							}
						}
					}
					for (int k = 0; k < 3; k++)
					{
						if (uses[k] == 0)
						{
							int i2 = k;
							int i3 = (k + 1) % triangleMeshNode.GetVertexCount();
							outline[triangleMeshNode.GetVertexIndex(i2)] = triangleMeshNode.GetVertexIndex(i3);
							hasInEdge.Add(triangleMeshNode.GetVertexIndex(i3));
							vertexPositions[triangleMeshNode.GetVertexIndex(i2)] = triangleMeshNode.GetVertex(i2);
							vertexPositions[triangleMeshNode.GetVertexIndex(i3)] = triangleMeshNode.GetVertex(i3);
						}
					}
				}
				return true;
			});
			for (int i = 0; i < 2; i++)
			{
				bool flag = i == 1;
				foreach (int num in new List<int>(outline.Keys))
				{
					if (flag || !hasInEdge.Contains(num))
					{
						int key = num;
						List<Vector3> list = new List<Vector3>();
						list.Add((Vector3)vertexPositions[key]);
						while (outline.ContainsKey(key))
						{
							int num2 = outline[key];
							outline.Remove(key);
							Vector3 item = (Vector3)vertexPositions[num2];
							list.Add(item);
							if (num2 == num)
							{
								break;
							}
							key = num2;
						}
						if (list.Count > 1)
						{
							sim.AddObstacle(list.ToArray(), this.wallHeight, flag);
						}
					}
				}
			}
		}

		
		public float wallHeight = 5f;

		
		private readonly List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

		
		private Simulator lastSim;
	}
}
