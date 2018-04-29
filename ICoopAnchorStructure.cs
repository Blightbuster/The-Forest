using System;
using System.Collections.Generic;
using TheForest.Buildings.Creation;


public interface ICoopAnchorStructure
{
	
	int GetAnchorIndex(StructureAnchor anchor);

	
	StructureAnchor GetAnchor(int anchor);

	
	
	
	List<StructureAnchor> Anchors { get; set; }
}
