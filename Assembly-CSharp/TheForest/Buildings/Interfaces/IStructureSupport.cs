using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Buildings.Interfaces
{
	public interface IStructureSupport
	{
		bool Enslaved { get; set; }

		float GetLevel();

		float GetHeight();

		List<Vector3> GetMultiPointsPositions(bool inherit = true);

		Vector3 SupportCenter { get; }
	}
}
