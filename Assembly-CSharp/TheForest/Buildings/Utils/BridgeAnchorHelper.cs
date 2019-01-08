using System;
using System.Collections.Generic;
using TheForest.Buildings.Creation;
using UnityEngine;

namespace TheForest.Buildings.Utils
{
	public class BridgeAnchorHelper
	{
		public static StructureAnchor GetAnchorFromHash(long anchorHash)
		{
			if (BridgeAnchorHelper.AnchorHashes == null)
			{
				StructureAnchor[] array = UnityEngine.Object.FindObjectsOfType<StructureAnchor>();
				BridgeAnchorHelper.AnchorHashes = new Dictionary<long, StructureAnchor>(array.Length);
				foreach (StructureAnchor structureAnchor in array)
				{
					BridgeAnchorHelper.AnchorHashes[structureAnchor.ToHash()] = structureAnchor;
				}
			}
			StructureAnchor result;
			if (!BridgeAnchorHelper.AnchorHashes.TryGetValue(anchorHash, out result))
			{
				result = null;
			}
			return result;
		}

		public static void Clear()
		{
			BridgeAnchorHelper.AnchorHashes = null;
		}

		private static Dictionary<long, StructureAnchor> AnchorHashes;
	}
}
