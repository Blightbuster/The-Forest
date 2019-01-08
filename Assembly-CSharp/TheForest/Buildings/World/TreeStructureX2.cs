using System;
using System.Collections;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Utils;
using TheForest.Utils.Enums;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.World
{
	[AddComponentMenu("Buildings/World/Tree Structure X2 (ex: Tree Zipline)")]
	public class TreeStructureX2 : TreeStructure
	{
		public override IEnumerator OnDeserialized()
		{
			if (BoltNetwork.isRunning)
			{
				while (!base.entity.isAttached)
				{
					yield return null;
				}
			}
			if (!BoltNetwork.isRunning || base.entity.isAttached)
			{
				yield return null;
				if (this._treeId >= 0)
				{
					CoopTreeId tid = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.$this._treeId);
					if (!tid)
					{
						yield return YieldPresets.WaitThreeSeconds;
						tid = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.$this._treeId);
					}
					if (tid)
					{
						LOD_Trees component = tid.GetComponent<LOD_Trees>();
						component.AddTreeCutDownTarget(base.gameObject);
					}
				}
				if (this._treeId2 >= 0)
				{
					CoopTreeId tid2 = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.$this._treeId2);
					if (!tid2)
					{
						yield return YieldPresets.WaitThreeSeconds;
						tid2 = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.$this._treeId2);
					}
					if (tid2)
					{
						LOD_Trees component2 = tid2.GetComponent<LOD_Trees>();
						component2.AddTreeCutDownTarget(base.gameObject);
					}
				}
			}
			yield break;
		}

		protected override IEnumerator OnTreeCutDown(GameObject trunk)
		{
			Transform fallingT;
			if (trunk.GetComponent<Rigidbody>())
			{
				fallingT = trunk.transform;
			}
			else
			{
				fallingT = trunk.GetComponentInChildren<Rigidbody>().transform;
			}
			base.transform.parent = fallingT;
			foreach (Collider collider in base.GetComponentsInChildren<Collider>())
			{
				collider.enabled = false;
			}
			if (!BoltNetwork.isClient)
			{
				yield return YieldPresets.WaitPointHeightSeconds;
			}
			else
			{
				yield return YieldPresets.WaitPointSixSeconds;
			}
			Craft_Structure ghost = base.GetComponentInChildren<Craft_Structure>();
			if (ghost && ghost.transform.parent == base.transform)
			{
				if (!BoltNetwork.isClient)
				{
					ghost.CancelBlueprintSafe();
				}
			}
			else
			{
				yield return YieldPresets.WaitThreeSeconds;
				CollapseStructure cs = base.gameObject.AddComponent<CollapseStructure>();
				cs._destructionForceMultiplier = 0.1f;
				cs._capsuleDirection = CapsuleDirections.Z;
			}
			yield break;
		}

		public override void Attached()
		{
		}

		public int TreeId2
		{
			get
			{
				return this._treeId2;
			}
			set
			{
				this._treeId2 = value;
			}
		}

		[SerializeThis]
		private int _treeId2 = -1;
	}
}
