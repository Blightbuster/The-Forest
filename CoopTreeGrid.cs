using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;


public static class CoopTreeGrid
{
	
	public static void DrawGrid()
	{
		if (CoopTreeGrid.Nodes != null)
		{
			int num = -2048;
			int num2 = 2048;
			if (CoopTreeGrid.r_debug != -2147483648 && CoopTreeGrid.c_debug != -2147483648)
			{
				Debug.DrawLine(new Vector3((float)(num + CoopTreeGrid.c_debug * 64), 0f, (float)num), new Vector3((float)(num + CoopTreeGrid.c_debug * 64), 0f, (float)num2), Color.magenta);
				Debug.DrawLine(new Vector3((float)num, 0f, (float)(num + CoopTreeGrid.r_debug * 64)), new Vector3((float)num2, 0f, (float)(num + CoopTreeGrid.r_debug * 64)), Color.magenta);
			}
			int num3 = CoopTreeGrid.r_debug * 64 + CoopTreeGrid.c_debug;
			if (CoopTreeGrid.Nodes[num3].Trees != null)
			{
				Vector3 start = new Vector3((float)(num + CoopTreeGrid.c_debug * 64 + 32), 0f, (float)(num + CoopTreeGrid.r_debug * 64 + 32));
				for (int i = 0; i < CoopTreeGrid.Nodes[num3].Trees.Count; i++)
				{
					Debug.DrawLine(start, CoopTreeGrid.Nodes[num3].Trees[i].transform.position, Color.red);
				}
			}
		}
	}

	
	public static void ShowObjectNode(CoopGridObject obj)
	{
		int num = CoopTreeGrid.CalculateNode(obj.transform.position);
		int num2 = -2048;
		Vector3 vector = new Vector3((float)(num2 + CoopTreeGrid.c_debug * 64 + 32), 0f, (float)(num2 + CoopTreeGrid.r_debug * 64 + 32));
		Debug.DrawLine(vector, obj.transform.position, Color.green);
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(vector, new Vector3(64f, 64f, 64f));
		Gizmos.DrawWireSphere(obj.transform.position, 32f);
	}

	
	public static void Clear()
	{
		CoopTreeGrid.Nodes = null;
		CoopTreeGrid.AttachQueue = null;
		CoopTreeGrid.AttachRoutineDone = false;
		CoopTreeGrid.TodoPlayerSweeps.Clear();
	}

	
	public static void SweepGrid()
	{
		CoopTreeGrid.SweepNodeIndex = 0;
	}

	
	public static void RegisterObject(CoopGridObject obj)
	{
		CoopTreeGrid.RegisterObject(obj, false);
	}

	
	public static void RegisterObject(CoopGridObject obj, bool is_update)
	{
		int num = CoopTreeGrid.CalculateNode(obj.transform.position);
		if (CoopTreeGrid.Nodes[num].Objects == null)
		{
			CoopTreeGrid.Nodes[num].Objects = new List<CoopGridObject>();
		}
		CoopTreeGrid.Nodes[num].Objects.Add(obj);
		obj.CurrentNode = num;
		if (!is_update)
		{
			obj.entity.Freeze(false);
		}
	}

	
	public static void RemoveObject(CoopGridObject obj)
	{
		if (obj.CurrentNode >= 0)
		{
			CoopTreeGrid.Nodes[obj.CurrentNode].Objects.Remove(obj);
		}
	}

	
	public static void UpdateObject(CoopGridObject obj)
	{
		int num = CoopTreeGrid.CalculateNode(obj.transform.position);
		if (num != obj.CurrentNode)
		{
			CoopTreeGrid.Nodes[obj.CurrentNode].Objects.Remove(obj);
			if (CoopTreeGrid.Nodes[num].Objects == null)
			{
				CoopTreeGrid.Nodes[num].Objects = new List<CoopGridObject>();
			}
			CoopTreeGrid.Nodes[num].Objects.Add(obj);
			obj.CurrentNode = num;
		}
	}

	
	private static int CalculateNode(Vector3 position)
	{
		Vector3 vector = position;
		int num = Mathf.Clamp(2048 + (int)vector.x, 0, 4096);
		int num2 = Mathf.Clamp(2048 + (int)vector.z, 0, 4096);
		CoopTreeGrid.c_debug = Mathf.Clamp(num / 64, 0, 63);
		CoopTreeGrid.r_debug = Mathf.Clamp(num2 / 64, 0, 63);
		return CoopTreeGrid.r_debug * 64 + CoopTreeGrid.c_debug;
	}

	
	public static void Update(IEnumerable<BoltEntity> trees)
	{
		CoopTreeGrid.Nodes = new CoopTreeGrid.Node[4096];
		CoopTreeGrid.AttachQueue = new Queue<BoltEntity>();
		foreach (BoltEntity boltEntity in trees)
		{
			int num = CoopTreeGrid.CalculateNode(boltEntity.transform.position);
			if (CoopTreeGrid.Nodes[num].Trees == null)
			{
				CoopTreeGrid.Nodes[num].Trees = new List<BoltEntity>();
			}
			CoopTreeGrid.Nodes[num].Trees.Add(boltEntity);
		}
		foreach (CoopTreeGrid.Node node in CoopTreeGrid.Nodes)
		{
			if (node.Trees != null)
			{
				foreach (BoltEntity item in node.Trees)
				{
					CoopTreeGrid.AttachQueue.Enqueue(item);
				}
			}
		}
	}

	
	public static void AttachTrees()
	{
		if (BoltNetwork.isServer && CoopTreeGrid.AttachQueue != null)
		{
			int count = CoopTreeGrid.AttachQueue.Count;
			for (int i = 0; i < 100; i++)
			{
				if (CoopTreeGrid.AttachQueue.Count <= 0)
				{
					break;
				}
				BoltEntity boltEntity = CoopTreeGrid.AttachQueue.Dequeue();
				if (!boltEntity.IsAttached())
				{
					BoltNetwork.Attach(boltEntity.gameObject);
					boltEntity.Freeze(true);
				}
				else
				{
					i--;
				}
			}
			if (count > 0 && CoopTreeGrid.AttachQueue.Count == 0 && !CoopTreeGrid.AttachRoutineDone)
			{
				CoopTreeGrid.AttachRoutineDone = true;
				Scene.ActiveMB.StartCoroutine(CoopTreeGrid.AttacheRoutine());
			}
		}
	}

	
	private static IEnumerator AttacheRoutine()
	{
		int doneCount = 0;
		foreach (BoltEntity entity in UnityEngine.Object.FindObjectsOfType<BoltEntity>())
		{
			if (!entity.IsAttached())
			{
				BoltNetwork.Attach(entity);
				if (++doneCount > 100)
				{
					doneCount = 0;
					yield return null;
				}
			}
		}
		yield break;
	}

	
	public static void AttachAdjacent(List<GameObject> positions)
	{
		if (CoopTreeGrid.Nodes == null)
		{
			return;
		}
		for (int i = 0; i < CoopTreeGrid.Nodes.Length; i++)
		{
			CoopTreeGrid.Nodes[i].NewHasPlayer = 0;
		}
		for (int j = 0; j < positions.Count; j++)
		{
			Vector3 position = positions[j].transform.position;
			int num = Mathf.Clamp(2048 + (int)position.x, 0, 4096);
			int num2 = Mathf.Clamp(2048 + (int)position.z, 0, 4096);
			int num3 = Mathf.Clamp(num / 64, 0, 63);
			int num4 = Mathf.Clamp(num2 / 64, 0, 63);
			for (int k = -2; k < 3; k++)
			{
				for (int l = -2; l < 3; l++)
				{
					int num5 = num4 + k;
					int num6 = num3 + l;
					if (num5 >= 0 && num5 < 64 && num6 >= 0 && num6 < 64)
					{
						CoopTreeGrid.Node[] nodes = CoopTreeGrid.Nodes;
						int num7 = num5 * 64 + num6;
						nodes[num7].NewHasPlayer = nodes[num7].NewHasPlayer + 1;
					}
				}
			}
		}
		for (int m = 0; m < CoopTreeGrid.Nodes.Length; m++)
		{
			CoopTreeGrid.Node node = CoopTreeGrid.Nodes[m];
			bool flag = node.NewHasPlayer == 0;
			bool flag2 = CoopTreeGrid.NodeToSweepIndex(m) == CoopTreeGrid.SweepNodeIndex;
			if (node.NewHasPlayer > node.OldHasPlayer || (node.NewHasPlayer == 0 && node.NewHasPlayer != node.OldHasPlayer) || flag2)
			{
				if (flag && flag2)
				{
					flag = false;
					CoopTreeGrid.Nodes[m].NewHasPlayer = -1;
				}
				if (node.Trees != null)
				{
					for (int n = 0; n < node.Trees.Count; n++)
					{
						BoltEntity boltEntity = node.Trees[n];
						if (boltEntity)
						{
							if (boltEntity.isAttached)
							{
								boltEntity.Freeze(flag);
							}
						}
						else
						{
							node.Trees.RemoveAt(n);
							n--;
						}
					}
				}
				if (node.Objects != null)
				{
					for (int num8 = 0; num8 < node.Objects.Count; num8++)
					{
						CoopGridObject coopGridObject = node.Objects[num8];
						if (coopGridObject && coopGridObject.entity)
						{
							if (coopGridObject.entity.isAttached)
							{
								coopGridObject.entity.Freeze(flag);
							}
						}
						else
						{
							node.Objects.RemoveAt(num8);
							num8--;
						}
					}
				}
			}
			CoopTreeGrid.Nodes[m].OldHasPlayer = CoopTreeGrid.Nodes[m].NewHasPlayer;
		}
		int num9 = CoopTreeGrid.SweepToNodeIndex(CoopTreeGrid.SweepNodeIndex);
		if (num9 < CoopTreeGrid.Nodes.Length)
		{
			CoopTreeGrid.SweepNodeIndex++;
		}
		else if (num9 == CoopTreeGrid.Nodes.Length + 4)
		{
			CoopTreeGrid.SweepNodeIndex++;
			Debug.Log("Finished tree grid sweep");
		}
	}

	
	private static int NodeToSweepIndex(int nodeIndex)
	{
		return nodeIndex / 4;
	}

	
	private static int SweepToNodeIndex(int sweepIndex)
	{
		return sweepIndex * 4;
	}

	
	public const int WORLD_EXTENTS = 2048;

	
	public const int WORLD_SIZE = 4096;

	
	public const int NODE_SIZE = 64;

	
	public const int NODE_COUNT = 64;

	
	public const int NODES_PER_FRAME = 512;

	
	private static int r_debug = int.MinValue;

	
	private static int c_debug = int.MinValue;

	
	private static CoopTreeGrid.Node[] Nodes;

	
	private static Queue<BoltEntity> AttachQueue = new Queue<BoltEntity>();

	
	private static int SweepNodeIndex;

	
	public static List<BoltConnection> TodoPlayerSweeps = new List<BoltConnection>();

	
	private static bool AttachRoutineDone;

	
	private const int sweepNodeSize = 4;

	
	private struct Node
	{
		
		public int NewHasPlayer;

		
		public int OldHasPlayer;

		
		public List<BoltEntity> Trees;

		
		public List<CoopGridObject> Objects;
	}
}
