using System;
using System.Collections.Generic;
using TheForest.Buildings.Creation;
using UnityEngine;


public class CoopAnchoredEx : MonoBehaviour, ICoopAnchorStructure
{
	
	public int GetAnchorIndex(StructureAnchor anchor)
	{
		return this._anchors.IndexOf(anchor);
	}

	
	public StructureAnchor GetAnchor(int anchor)
	{
		return this._anchors[anchor];
	}

	
	
	
	public List<StructureAnchor> Anchors
	{
		get
		{
			return this._anchors;
		}
		set
		{
			this._anchors = value;
		}
	}

	
	[SerializeField]
	private List<StructureAnchor> _anchors = new List<StructureAnchor>();
}
