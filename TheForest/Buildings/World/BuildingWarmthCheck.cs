using System;
using System.Collections;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Interfaces;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class BuildingWarmthCheck : MonoBehaviour
	{
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("structure") && !this._hasActiveWarmth)
			{
				FloorArchitect componentInParent = other.GetComponentInParent<FloorArchitect>();
				if (componentInParent)
				{
					IStructureSupport structureSupport = this.SearchValidCeiling(componentInParent);
					if (structureSupport != null && this.FloorHasLitFire(componentInParent.transform))
					{
						base.StartCoroutine(this.BuildingWarmthCheckRoutine(componentInParent, structureSupport));
						return;
					}
				}
			}
		}

		
		private IStructureSupport SearchValidCeiling(FloorArchitect floor)
		{
			IStructureSupport structureSupport = this.SearchValidCeiling<FloorArchitect>(floor);
			if (structureSupport == null)
			{
				structureSupport = this.SearchValidCeiling<RoofArchitect>(floor);
			}
			return structureSupport;
		}

		
		private IStructureSupport SearchValidCeiling<T>(FloorArchitect floor) where T : IStructureSupport
		{
			foreach (T t in floor.GetComponentsInChildren<T>())
			{
				if (t != null && t.GetLevel() > floor.GetLevel() + 4f && this.IsPlayerPositionValid(LocalPlayer.Transform.position, floor, t))
				{
					return t;
				}
			}
			return null;
		}

		
		private IEnumerator BuildingWarmthCheckRoutine(FloorArchitect floor, IStructureSupport ceiling)
		{
			this._hasActiveWarmth = true;
			LocalPlayer.GameObject.SendMessage("HomeWarmth");
			yield return YieldPresets.WaitPointFiveSeconds;
			for (int i = 0; i < 2; i++)
			{
				bool hasLitFire = true;
				while (hasLitFire && floor != null && floor.gameObject && ceiling != null && (MonoBehaviour)ceiling && ((MonoBehaviour)ceiling).gameObject && this.IsPlayerPositionValid(LocalPlayer.Transform.position, floor, ceiling))
				{
					i = 0;
					yield return YieldPresets.WaitOneSecond;
					hasLitFire = this.FloorHasLitFire(floor.transform);
				}
				if (!floor || !hasLitFire)
				{
					break;
				}
				ceiling = this.SearchValidCeiling(floor);
			}
			LocalPlayer.GameObject.SendMessage("LeaveHomeWarmth");
			this._hasActiveWarmth = false;
			yield break;
		}

		
		private bool IsPlayerPositionValid(Vector3 testPos, IStructureSupport floor, IStructureSupport ceiling)
		{
			return testPos.y > floor.GetLevel() && testPos.y < ceiling.GetLevel() && MathEx.IsPointInPolygon(testPos, floor.GetMultiPointsPositions(true)) && MathEx.IsPointInPolygon(testPos, ceiling.GetMultiPointsPositions(true));
		}

		
		private bool FloorHasLitFire(Transform floor)
		{
			bool result = false;
			int childCount = floor.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = floor.transform.GetChild(i);
				if (child.CompareTag("fire"))
				{
					Fire2 componentInChildren = child.GetComponentInChildren<Fire2>();
					if (componentInChildren.Lit)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		
		private bool _hasActiveWarmth;
	}
}
