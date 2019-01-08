using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;
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
				while (!base.entity.isAttached)
				{
					yield return null;
				}
			}
			if (this._treeId >= 0 && !this._tsl && (!BoltNetwork.isRunning || base.entity.isAttached))
			{
				yield return null;
				if (!this._tsl)
				{
					if (BoltNetwork.isRunning && base.entity.isAttached && base.entity.isOwner && base.entity.StateIs<ITreeHouseState>())
					{
						base.entity.GetState<ITreeHouseState>().TreeId = this._treeId;
					}
					CoopTreeId tid = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.$this._treeId);
					if (!tid)
					{
						yield return YieldPresets.WaitThreeSeconds;
						tid = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId i) => i.Id == this.$this._treeId);
					}
					if (tid && !this._tsl)
					{
						LOD_Trees component = tid.GetComponent<LOD_Trees>();
						component.AddTreeCutDownTarget(base.gameObject);
						if (this._cutLeaves)
						{
							this._tsl = component.gameObject.AddComponent<TreeStructureLod>();
							this._tsl._lod = component;
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
			foreach (StructureAnchor structureAnchor in base.GetComponentsInChildren<StructureAnchor>())
			{
				UnityEngine.Object.Destroy(structureAnchor.gameObject);
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
				BuildingHealth bh = base.GetComponent<BuildingHealth>();
				bh.Collapse(base.transform.position + Vector3.down);
			}
			yield break;
		}

		public override void Attached()
		{
			if (base.entity.isOwner)
			{
				if (base.entity.StateIs<ITreeHouseState>() || base.entity.StateIs<IConstructionState>())
				{
					base.StartCoroutine(this.OnDeserialized());
				}
			}
			else if (base.entity.StateIs<ITreeHouseState>())
			{
				base.entity.GetState<ITreeHouseState>().AddCallback("TreeId", new PropertyCallbackSimple(this.OnReceivedTreeId));
			}
		}

		private void OnReceivedTreeId()
		{
			this._treeId = base.entity.GetState<ITreeHouseState>().TreeId;
			base.StartCoroutine(this.OnDeserialized());
			base.entity.GetState<ITreeHouseState>().RemoveCallback("TreeId", new PropertyCallbackSimple(this.OnReceivedTreeId));
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
