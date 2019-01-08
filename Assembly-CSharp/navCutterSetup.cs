using System;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;

public class navCutterSetup : MonoBehaviour
{
	private void Start()
	{
		BoxCollider component = base.transform.GetComponent<BoxCollider>();
		SphereCollider component2 = base.transform.GetComponent<SphereCollider>();
		if (!component && !component2)
		{
			return;
		}
		if (!base.transform.GetComponent<NavmeshCut>())
		{
			base.gameObject.AddComponent<NavmeshCut>();
		}
		NavmeshCut component3 = base.transform.GetComponent<NavmeshCut>();
		if (!Scene.SceneTracker.worldNavCuts.Contains(component3))
		{
			Scene.SceneTracker.worldNavCuts.Add(component3);
		}
		if (component)
		{
			if (base.transform.GetComponent<WaterClean>())
			{
				component3.useRotation = true;
				component3.rectangleSize = new Vector2(component.size.x, component.size.z);
				component3.height = component.size.y;
				component3.center = new Vector3(component3.center.x, -component3.height / 2f, component3.center.z);
			}
			else
			{
				component3.useRotation = true;
				component3.rectangleSize = new Vector2(component.size.x * 1.5f, component.size.z * 1.5f);
				component3.height = component.size.y;
				component3.center = component.center;
				getStructureStrength component4 = base.transform.GetComponent<getStructureStrength>();
				if (component4 && component4._type == getStructureStrength.structureType.wall)
				{
					component3.height += 2f;
					component3.center = new Vector3(component3.center.x, component3.center.y - 2f, component3.center.z);
				}
			}
		}
		else if (component2)
		{
			component3.useRotation = false;
			component3.type = NavmeshCut.MeshType.Circle;
			component3.circleRadius = component2.radius * 1.75f;
			component3.height = 3f;
			component3.center = component2.center;
		}
	}
}
