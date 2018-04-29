using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	public class Constructions : MonoBehaviour
	{
		
		private void Awake()
		{
			foreach (BuildingBlueprint buildingBlueprint in this._blueprints)
			{
				if (buildingBlueprint._ghostPrefab)
				{
					PrefabIdentifier component = buildingBlueprint._ghostPrefab.GetComponent<PrefabIdentifier>();
					if (component && !this._blueprintsByGuid.ContainsKey(component.ClassId))
					{
						this._blueprintsByGuid.Add(component.ClassId, buildingBlueprint);
					}
				}
				if (buildingBlueprint._ghostPrefabMP)
				{
					PrefabIdentifier component2 = buildingBlueprint._ghostPrefabMP.GetComponent<PrefabIdentifier>();
					if (component2 && !this._blueprintsByGuid.ContainsKey(component2.ClassId))
					{
						this._blueprintsByGuid.Add(component2.ClassId, buildingBlueprint);
					}
				}
				if (buildingBlueprint._builtPrefab)
				{
					PrefabIdentifier component3 = buildingBlueprint._builtPrefab.GetComponent<PrefabIdentifier>();
					if (component3 && !this._blueprintsByGuid.ContainsKey(component3.ClassId))
					{
						this._blueprintsByGuid.Add(component3.ClassId, buildingBlueprint);
					}
				}
				if (buildingBlueprint._exclusionGroup != ExclusionGroups.None)
				{
					this._exclusionGroupByTypes.Add(buildingBlueprint._type, buildingBlueprint._exclusionGroup);
				}
			}
		}

		
		public BuildingBlueprint GetBlueprintByPrefabId(string id)
		{
			BuildingBlueprint result;
			this._blueprintsByGuid.TryGetValue(id, out result);
			return result;
		}

		
		public ExclusionGroups GetBlueprintExclusionGroup(BuildingTypes type)
		{
			if (this._exclusionGroupByTypes.ContainsKey(type))
			{
				return this._exclusionGroupByTypes[type];
			}
			return ExclusionGroups.None;
		}

		
		[NameFromProperty("_type", 200, order = 1)]
		public List<BuildingBlueprint> _blueprints;

		
		private Dictionary<string, BuildingBlueprint> _blueprintsByGuid = new Dictionary<string, BuildingBlueprint>();

		
		private Dictionary<BuildingTypes, ExclusionGroups> _exclusionGroupByTypes = new Dictionary<BuildingTypes, ExclusionGroups>();
	}
}
