using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;


public class astarPreRuntimeSetup : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private IEnumerator setupAstarRegions()
	{
		yield return new WaitForSeconds(2f);
		while (!Scene.SceneTracker.waitForLoadSequence)
		{
			yield return null;
		}
		while (Scene.SceneTracker.doingGlobalNavUpdate)
		{
			yield return null;
		}
		Scene.MutantControler.calculateMainNavArea();
		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(1f);
		yield break;
	}

	
	public void removeSmallRegions()
	{
		AstarPath.RegisterSafeUpdate(delegate
		{
			int[] sizes = new int[10000];
			RecastGraph recastGraph = AstarPath.active.astarData.recastGraph;
			recastGraph.GetNodes(delegate(GraphNode node)
			{
				sizes[(int)((UIntPtr)node.Area)]++;
				return true;
			});
			int largest = 0;
			for (int i = 0; i < sizes.Length; i++)
			{
				largest = ((sizes[i] <= sizes[largest]) ? largest : i);
			}
			recastGraph.GetNodes(delegate(GraphNode node)
			{
				bool flag = false;
				using (List<uint>.Enumerator enumerator = Scene.MutantControler.mostCommonArea.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int num = (int)enumerator.Current;
						if ((long)num == (long)((ulong)node.Area))
						{
							flag = true;
						}
					}
				}
				if ((ulong)node.Area != (ulong)((long)largest) && !flag)
				{
					node.Walkable = false;
				}
				return true;
			});
			AstarPath.active.FloodFill();
		});
	}

	
	private void calculateMainNavAreaEditor(GameObject m)
	{
		m.SetActive(true);
		navAreaSetup[] componentsInChildren = m.GetComponent<mutantController>().navRef.GetComponentsInChildren<navAreaSetup>();
		int num = 0;
		if (!m.GetComponent<mutantController>().navRef.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (navAreaSetup navAreaSetup in componentsInChildren)
		{
			if (navAreaSetup.areaNum > num)
			{
				num = navAreaSetup.areaNum;
			}
		}
		this.mostCommonArea.Clear();
		if (AstarPath.active == null)
		{
			AstarPath.active = (UnityEngine.Object.FindObjectOfType(typeof(AstarPath)) as AstarPath);
		}
		RecastGraph recastGraph = AstarPath.active.astarData.recastGraph;
		for (int j = 0; j <= num; j++)
		{
			List<uint> list = new List<uint>();
			foreach (navAreaSetup navAreaSetup2 in componentsInChildren)
			{
				if (navAreaSetup2.areaNum == j)
				{
					GraphNode node = recastGraph.GetNearest(navAreaSetup2.gameObject.transform.position, NNConstraint.Default).node;
					if (node != null)
					{
						list.Add(node.Area);
					}
				}
			}
			Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
			this.mostCommonArea.Add(list[0]);
			dictionary.Add(list[0], 1);
			for (int l = 1; l < list.Count; l++)
			{
				if (dictionary.ContainsKey(list[l]))
				{
					Dictionary<uint, int> dictionary3;
					Dictionary<uint, int> dictionary2 = dictionary3 = dictionary;
					uint key2;
					uint key = key2 = list[l];
					int num2 = dictionary3[key2];
					dictionary2[key] = num2 + 1;
					if (dictionary[list[l]] > dictionary[this.mostCommonArea[j]])
					{
						this.mostCommonArea[j] = list[l];
					}
				}
				else
				{
					dictionary.Add(list[l], 1);
				}
			}
		}
		m.SetActive(false);
	}

	
	public List<uint> mostCommonArea = new List<uint>();
}
