using System;
using System.Collections.Generic;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;


public class navMeshChecker : MonoBehaviour
{
	
	private void Start()
	{
		this.init = true;
	}

	
	public void OnEnable()
	{
		if (!this.init)
		{
			return;
		}
		if (!AstarPath.active)
		{
			return;
		}
		if (this.rg == null)
		{
			this.rg = AstarPath.active.astarData.recastGraph;
		}
		if (this.rg == null)
		{
			return;
		}
		GraphNode node = this.rg.GetNearest(base.transform.position).node;
		if (node != null)
		{
			if (!node.Walkable)
			{
				base.gameObject.SetActive(false);
			}
			if (this.checkForValidArea)
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
				if (!flag)
				{
					base.gameObject.SetActive(false);
				}
			}
		}
	}

	
	private bool init;

	
	private RecastGraph rg;

	
	public bool checkForValidArea;
}
