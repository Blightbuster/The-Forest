using System;
using UnityEngine;

namespace Pathfinding
{
	
	[HelpURL("http:
	[AddComponentMenu("Pathfinding/Navmesh/RecastTileUpdate")]
	public class RecastTileUpdate : MonoBehaviour
	{
		
		
		
		public static event Action<Bounds> OnNeedUpdates;

		
		private void Start()
		{
			this.ScheduleUpdate();
		}

		
		private void OnDestroy()
		{
			this.ScheduleUpdate();
		}

		
		public void ScheduleUpdate()
		{
			Collider component = base.GetComponent<Collider>();
			if (component != null)
			{
				if (RecastTileUpdate.OnNeedUpdates != null)
				{
					RecastTileUpdate.OnNeedUpdates(component.bounds);
				}
			}
			else if (RecastTileUpdate.OnNeedUpdates != null)
			{
				RecastTileUpdate.OnNeedUpdates(new Bounds(base.transform.position, Vector3.zero));
			}
		}
	}
}
