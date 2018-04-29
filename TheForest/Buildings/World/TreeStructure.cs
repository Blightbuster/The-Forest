using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Utils;
using TheForest.Utils.Enums;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[AddComponentMenu("Buildings/World/Tree Structure (ex: Tree House)")]
	public class TreeStructure : EntityBehaviour<ITreeHouseState>
	{
		
		private void Start()
		{
			if (!BoltNetwork.isClient)
			{
				if (!LevelSerializer.IsDeserializing)
				{
					base.StartCoroutine(this.OnDeserialized());
				}
			}
			else
			{
				base.enabled = false;
			}
		}

		
		private void OnDestroy()
		{
			if (this._tsl)
			{
				UnityEngine.Object.Destroy(this._tsl);
				this._tsl = null;
			}
			if (CoopPlayerCallbacks.AllTrees != null)
			{
				CoopTreeId coopTreeId = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this._treeId);
				if (coopTreeId)
				{
					LOD_Trees component = coopTreeId.GetComponent<LOD_Trees>();
					if (component)
					{
						component.RemoveTreeCutDownTarget(base.gameObject);
					}
				}
			}
		}

		
		public virtual IEnumerator OnDeserialized()
		{
			if (BoltNetwork.isRunning)
			{
				while (!this.entity.isAttached)
				{
					yield return null;
				}
			}
			if (this._treeId >= 0 && !this._tsl && (!BoltNetwork.isRunning || this.entity.isAttached))
			{
				yield return null;
				if (!this._tsl)
				{
					if (BoltNetwork.isRunning && this.entity.isAttached && this.entity.isOwner && this.entity.StateIs<ITreeHouseState>())
					{
						this.entity.GetState<ITreeHouseState>().TreeId = this._treeId;
					}
					CoopTreeId tid = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.<>f__this._treeId);
					if (!tid)
					{
						yield return YieldPresets.WaitThreeSeconds;
						tid = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.<>f__this._treeId);
					}
					if (tid && !this._tsl)
					{
						LOD_Trees lt = tid.GetComponent<LOD_Trees>();
						lt.AddTreeCutDownTarget(base.gameObject);
						if (this._cutLeaves)
						{
							this._tsl = lt.gameObject.AddComponent<TreeStructureLod>();
							this._tsl._lod = lt;
						}
					}
				}
			}
			yield break;
		}

		
		protected virtual IEnumerator OnTreeCutDown(GameObject trunk)
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
			foreach (Collider c in base.GetComponentsInChildren<Collider>())
			{
				c.enabled = false;
			}
			if (!BoltNetwork.isClient)
			{
				yield return YieldPresets.WaitPointHeightSeconds;
			}
			else
			{
				yield return YieldPresets.WaitPointSixSeconds;
			}
			foreach (StructureAnchor sa in base.GetComponentsInChildren<StructureAnchor>())
			{
				UnityEngine.Object.Destroy(sa.gameObject);
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
			if (this.entity.isOwner)
			{
				if (this.entity.StateIs<ITreeHouseState>() || this.entity.StateIs<IConstructionState>())
				{
					base.StartCoroutine(this.OnDeserialized());
				}
			}
			else if (this.entity.StateIs<ITreeHouseState>())
			{
				this.entity.GetState<ITreeHouseState>().AddCallback("TreeId", new PropertyCallbackSimple(this.OnReceivedTreeId));
			}
		}

		
		private void OnReceivedTreeId()
		{
			this._treeId = this.entity.GetState<ITreeHouseState>().TreeId;
			base.StartCoroutine(this.OnDeserialized());
			this.entity.GetState<ITreeHouseState>().RemoveCallback("TreeId", new PropertyCallbackSimple(this.OnReceivedTreeId));
		}

		
		
		
		public int TreeId
		{
			get
			{
				return this._treeId;
			}
			set
			{
				this._treeId = value;
			}
		}

		
		public bool _cutLeaves = true;

		
		[SerializeThis]
		protected int _treeId = -1;

		
		private TreeStructureLod _tsl;
	}
}
