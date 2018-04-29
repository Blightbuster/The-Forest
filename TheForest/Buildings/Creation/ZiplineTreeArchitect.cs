using System;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	public class ZiplineTreeArchitect : ZiplineArchitect
	{
		
		protected override void OnBuilt(GameObject built)
		{
			ZiplineTreeArchitect component = built.GetComponent<ZiplineTreeArchitect>();
			component._gate1.transform.position = this._gate1.transform.position;
			component._gate1.transform.rotation = this._gate1.transform.rotation;
			component._gate2.transform.position = this._gate2.transform.position;
			component._gate2.transform.rotation = this._gate2.transform.rotation;
			component._wasBuilt = true;
			component._treeStructure.TreeId = this._treeStructure.TreeId;
			component._treeStructure.TreeId2 = this._treeStructure.TreeId2;
		}

		
		private void RespawningRenderersFrom(GameObject respawned)
		{
			ZiplineArchitect component = respawned.GetComponent<ZiplineArchitect>();
			component._gate1.transform.parent = this._gate1.transform.parent;
			component._gate1.transform.position = this._gate1.transform.position;
			component._gate1.transform.rotation = this._gate1.transform.rotation;
			component._gate2.transform.parent = this._gate2.transform.parent;
			component._gate2.transform.position = this._gate2.transform.position;
			component._gate2.transform.rotation = this._gate2.transform.rotation;
			UnityEngine.Object.Destroy(this._gate1.gameObject);
			UnityEngine.Object.Destroy(this._gate2.gameObject);
			this._gate1 = component._gate1;
			this._gate2 = component._gate2;
		}

		
		protected override bool CheckLockGate()
		{
			bool flag = this.CheckLockOnTree();
			bool flag2 = !this._gate1.transform.parent;
			Transform transform = (!flag2) ? this._gate1.transform : this._gate2.transform;
			if (flag)
			{
				float num = Terrain.activeTerrain.SampleHeight(transform.position);
				if (LocalPlayer.Create.TargetTree)
				{
					transform.position = new Vector3(LocalPlayer.Create.TargetTree.position.x, Mathf.Clamp(LocalPlayer.Create.BuildingPlacer.AirBorneHeight, num + 5f, num + 40f), LocalPlayer.Create.TargetTree.position.z);
				}
				else
				{
					transform.position = new Vector3(transform.position.x, num - 1f, transform.position.z);
				}
			}
			else if (transform.transform.parent && !flag2)
			{
				transform.localPosition = Vector3.zero;
			}
			return flag;
		}

		
		private bool CheckLockOnTree()
		{
			bool flag = !this._gate1.transform.parent;
			bool flag2 = !this._gate2.transform.parent;
			LocalPlayer.Create.TargetTree = null;
			if (!flag || !flag2)
			{
				this.SearchTree();
			}
			bool flag3 = !flag2 && (flag || LocalPlayer.Create.TargetTree);
			if (flag3)
			{
				if (flag)
				{
					float num = Vector3.Distance(this.Gate1RopePosition, this.Gate2RopePosition);
					if (num < 20f || num > 470f)
					{
						return false;
					}
					float num2 = 1.5f;
					Vector3 normalized = (this.Gate2RopePosition - this.Gate1RopePosition).normalized;
					RaycastHit raycastHit;
					if (Physics.SphereCast(this.Gate1RopePosition + Vector3.down + normalized * (num2 * 2f), num2, normalized, out raycastHit, num - num2 * 4f, LocalPlayer.Create.BuildingPlacer.FloorLayers | LayerMask.GetMask(new string[]
					{
						"treeMid",
						"Blocker",
						"PickUp"
					}), QueryTriggerInteraction.Collide))
					{
						float num3 = Vector3.Distance(raycastHit.point, base.transform.position);
						if ((raycastHit.collider.gameObject != LocalPlayer.Create.TargetTree && raycastHit.collider.CompareTag("Tree")) || (num3 > (float)((!LocalPlayer.Create.TargetTree) ? 20 : 7) && raycastHit.collider.gameObject != Terrain.activeTerrain.gameObject) || (num3 > 100f && raycastHit.collider.gameObject == Terrain.activeTerrain.gameObject))
						{
							return false;
						}
					}
					if (Physics.SphereCast(this.Gate1RopePosition + normalized * (num2 * 2f), 0.35f, normalized, out raycastHit, num - num2 * 4f, LocalPlayer.Create.BuildingPlacer.FloorLayers | LayerMask.GetMask(new string[]
					{
						"treeMid",
						"Blocker",
						"PickUp"
					}), QueryTriggerInteraction.Collide))
					{
						return false;
					}
				}
				int num4 = -1;
				if (LocalPlayer.Create.TargetTree)
				{
					TreeHealth component;
					if (LocalPlayer.Create.TargetTree.CompareTag("conTree"))
					{
						component = LocalPlayer.Create.TargetTree.parent.GetComponent<TreeHealth>();
					}
					else
					{
						component = LocalPlayer.Create.TargetTree.GetComponent<TreeHealth>();
					}
					num4 = component.LodTree.GetComponentInChildren<CoopTreeId>().Id;
					if (num4 == this._treeStructure.TreeId)
					{
						return false;
					}
				}
				if (TheForest.Utils.Input.GetButtonDown("Fire1"))
				{
					if (!flag)
					{
						this._treeStructure.TreeId = num4;
						this._gate1.transform.parent = null;
					}
					else
					{
						this._treeStructure.TreeId2 = num4;
						this._gate2.transform.parent = null;
						if (this._ziplineRoot)
						{
							UnityEngine.Object.Destroy(this._ziplineRoot.gameObject);
						}
						this._ziplineRoot = base.CreateZipline(this.Gate1RopePosition, this.Gate2RopePosition);
					}
				}
			}
			return flag3;
		}

		
		private void SearchTree()
		{
			RaycastHit raycastHit;
			if (Physics.SphereCast(LocalPlayer.MainCamTr.position, 1f, LocalPlayer.MainCamTr.forward, out raycastHit, 12f, 1 << LayerMask.NameToLayer("treeMid")))
			{
				if (raycastHit.collider.CompareTag("conTree"))
				{
					LocalPlayer.Create.TargetTree = raycastHit.collider.transform;
				}
				else if (raycastHit.collider.CompareTag("Tree"))
				{
					LocalPlayer.Create.TargetTree = raycastHit.collider.transform;
				}
			}
		}

		
		protected override void CheckUnlockGate()
		{
			if (TheForest.Utils.Input.GetButtonDown("AltFire"))
			{
				if (!this._gate2.transform.parent)
				{
					this._treeStructure.TreeId2 = -1;
					this._gate2.transform.parent = base.transform;
					this._gate2.transform.localPosition = Vector3.zero;
					this._gate2.transform.localRotation = Quaternion.identity;
				}
				else if (!this._gate1.transform.parent)
				{
					this._treeStructure.TreeId = -1;
					this._gate1.transform.parent = base.transform;
					this._gate1.transform.localPosition = Vector3.zero;
					this._gate1.transform.localRotation = Quaternion.identity;
					if (this._gate2.activeSelf)
					{
						this._gate2.SetActive(false);
					}
				}
			}
		}

		
		
		protected override Vector3 Gate1RopePosition
		{
			get
			{
				return this._gate1.transform.position + this._gate1.transform.forward * 1.5f + this._gate1.transform.up * 1.5f;
			}
		}

		
		
		protected override Vector3 Gate2RopePosition
		{
			get
			{
				return this._gate2.transform.position + this._gate2.transform.forward * 1.5f + this._gate2.transform.up * 1.5f;
			}
		}

		
		
		
		public override IProtocolToken CustomToken
		{
			get
			{
				return new CoopZiplineTreeToken
				{
					p1 = this._gate1.transform.position,
					p2 = this._gate2.transform.position,
					treeId = this._treeStructure.TreeId,
					treeId2 = this._treeStructure.TreeId2
				};
			}
			set
			{
				CoopZiplineTreeToken coopZiplineTreeToken = (CoopZiplineTreeToken)value;
				this._gate1.transform.position = coopZiplineTreeToken.p1;
				this._gate2.transform.position = coopZiplineTreeToken.p2;
				this._treeStructure.TreeId = coopZiplineTreeToken.treeId;
				this._treeStructure.TreeId2 = coopZiplineTreeToken.treeId2;
				if (!this._wasBuilt)
				{
					this._wasPlaced = true;
				}
			}
		}

		
		public TreeStructureX2 _treeStructure;
	}
}
