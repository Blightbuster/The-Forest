using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Buildings.World;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[AddComponentMenu("Buildings/Creation/Wall Defensive Chunk Architect")]
	[DoNotSerializePublic]
	public class WallDefensiveChunkArchitect : WallChunkArchitect
	{
		
		protected override IEnumerator OnDeserialized()
		{
			if (!this._initialized)
			{
				if (this._p1 == Vector3.zero)
				{
					this._p1 = base.transform.position;
				}
				if (this._p2 == Vector3.zero)
				{
					this._p2 = base.transform.position + base.transform.forward * (this._logWidth * 5f);
				}
				yield return base.StartCoroutine(this.<OnDeserialized>__BaseCallProxy0());
			}
			yield break;
		}

		
		protected override void CreateStructure(bool isRepair = false)
		{
			Renderer renderer = null;
			if (isRepair)
			{
				LOD_GroupToggle component = this._wallRoot.GetComponent<LOD_GroupToggle>();
				if (component)
				{
					renderer = component._levels[1].Renderers[0];
				}
				base.Clear();
				base.StartCoroutine(base.DelayedAwake(true));
			}
			Vector3 size = this._logRenderer.bounds.size;
			this._logLength = size.y;
			this._logWidth = size.z;
			int layer = LayerMask.NameToLayer("Prop");
			this._wallRoot = this.SpawnStructure();
			this._wallRoot.parent = base.transform;
			if (this._wasBuilt)
			{
				this._gridToken = InsideCheck.AddWallChunk(this._p1, this._p2, this._logLength);
				GameObject gameObject = this._wallRoot.gameObject;
				gameObject.tag = "structure";
				gameObject.layer = layer;
				BoxCollider boxCollider = this._wallRoot.gameObject.AddComponent<BoxCollider>();
				Vector3 vector = base.transform.InverseTransformVector(this._p2 - this._p1);
				boxCollider.size = new Vector3(this._logWidth, this._logLength, Mathf.Abs(vector.z));
				Vector3 center = boxCollider.size / 2f;
				center.x = 0f;
				center.y -= 1f;
				center.z -= this._logWidth / 2f;
				boxCollider.center = center;
				BuildingHealth component2 = base.GetComponent<BuildingHealth>();
				if (component2)
				{
					component2._renderersRoot = this._wallRoot.gameObject;
					if (BoltNetwork.isRunning)
					{
						component2.SetMpRandomDistortColliders(new Collider[]
						{
							boxCollider
						});
					}
				}
				this._wallRoot.gameObject.AddComponent<BuildingHealthHitRelay>();
				getStructureStrength getStructureStrength = this._wallRoot.gameObject.AddComponent<getStructureStrength>();
				getStructureStrength._type = getStructureStrength.structureType.wall;
				getStructureStrength._strength = getStructureStrength.strength.veryStrong;
				this._wallRoot.gameObject.AddComponent<gridObjectBlocker>();
				if (!renderer)
				{
					int logCost = this.GetLogCost();
					bool flag = logCost % 2 == 0;
					bool flag2 = this._p1.y > this._p2.y;
					Transform transform = new GameObject("DW LOD Skewer").transform;
					transform.parent = base.transform;
					transform.localEulerAngles = ((!flag2) ? new Vector3(0f, 180f, 0f) : Vector3.zero);
					transform.position = this._p1 + transform.forward * (((float)logCost * 0.5f - 0.5f) * (float)((!flag2) ? -1 : 1) * base.LogWidth);
					Transform transform2 = new GameObject("DW LOD").transform;
					transform2.gameObject.layer = 21;
					transform2.parent = transform;
					transform2.localPosition = Vector3.zero;
					Prefabs.Instance.Constructions._defensiveWallSkewLOD.SetSkew(transform, transform2, transform.eulerAngles.x);
					renderer = transform2.gameObject.AddComponent<MeshRenderer>();
					renderer.sharedMaterial = Prefabs.Instance.LogWallDefensiveExBuiltPrefabLOD1[0].sharedMaterial;
					MeshFilter meshFilter = transform2.gameObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = Prefabs.Instance.LogWallDefensiveExBuiltPrefabLOD1[Mathf.Clamp(logCost, 0, Prefabs.Instance.LogWallDefensiveExBuiltPrefabLOD1.Length - 1)].GetComponent<MeshFilter>().sharedMesh;
				}
				LOD_GroupToggle lod_GroupToggle = this._wallRoot.gameObject.AddComponent<LOD_GroupToggle>();
				lod_GroupToggle.enabled = false;
				lod_GroupToggle._levels = new LOD_GroupToggle.LodLevel[2];
				lod_GroupToggle._levels[0] = new LOD_GroupToggle.LodLevel
				{
					Renderers = this._wallRoot.GetComponentsInChildren<Renderer>(),
					VisibleDistance = 100f
				};
				lod_GroupToggle._levels[1] = new LOD_GroupToggle.LodLevel
				{
					Renderers = new Renderer[]
					{
						renderer
					},
					VisibleDistance = 10000f
				};
			}
		}

		
		public override Transform SpawnStructure()
		{
			Transform transform = new GameObject("WallChunk").transform;
			transform.transform.position = this._p1;
			Vector3 vector = this._p2 - this._p1;
			Vector3 normalized = Vector3.Scale(vector, new Vector3(1f, 0f, 1f)).normalized;
			float y = Mathf.Tan(Vector3.Angle(vector, normalized) * 0.0174532924f) * this._logWidth;
			Quaternion rotation = Quaternion.LookRotation(Vector3.forward);
			float num = Vector3.Distance(this._p1, this._p2);
			int num2 = Mathf.RoundToInt(num / this._logWidth);
			Vector3 b = normalized * num / (float)num2;
			b.y = y;
			if (vector.y < 0f)
			{
				b.y *= -1f;
			}
			Vector3 vector2 = this._p1;
			transform.position = this._p1;
			transform.LookAt(this._p2);
			for (int i = 0; i < num2; i++)
			{
				Transform transform2 = base.NewLog(vector2, rotation);
				transform2.parent = transform;
				vector2 += b;
			}
			return transform;
		}

		
		protected override Quaternion RandomizeLogRotation(Quaternion logRot)
		{
			return logRot * Quaternion.Euler(UnityEngine.Random.Range(-1.5f, 1.5f), (float)UnityEngine.Random.Range(0, 359), UnityEngine.Random.Range(-1.5f, 1.5f));
		}

		
		private bool CanTurnIntoGate()
		{
			return Vector3.Distance(this._p2, this._p1) > this._logWidth * 3.5f && Mathf.Abs(Vector3.Dot((this._p2 - this._p1).normalized, Vector3.up)) < this._doorAdditionMaxSlope;
		}

		
		protected bool IsDoor(WallChunkArchitect.Additions addition)
		{
			return addition >= WallChunkArchitect.Additions.Door1;
		}

		
		public override void ShowToggleAdditionIcon()
		{
			WallChunkArchitect.Additions additions = this.SegmentNextAddition(this._addition);
			if (additions != WallChunkArchitect.Additions.Wall)
			{
				if (additions == WallChunkArchitect.Additions.Door1)
				{
					if (Scene.HudGui.ToggleDefensiveWallIcon.activeSelf)
					{
						Scene.HudGui.ToggleDefensiveWallIcon.SetActive(false);
					}
					if (!Scene.HudGui.ToggleGate1Icon.activeSelf)
					{
						Scene.HudGui.ToggleGate1Icon.SetActive(true);
					}
				}
			}
			else
			{
				if (!Scene.HudGui.ToggleDefensiveWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleDefensiveWallIcon.SetActive(true);
				}
				if (Scene.HudGui.ToggleGate1Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate1Icon.SetActive(false);
				}
			}
		}

		
		public override void HideToggleAdditionIcon()
		{
			if (Scene.HudGui)
			{
				if (Scene.HudGui.ToggleDefensiveWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleDefensiveWallIcon.SetActive(false);
				}
				if (Scene.HudGui.ToggleGate1Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate1Icon.SetActive(false);
				}
			}
		}

		
		protected override WallChunkArchitect.Additions SegmentNextAddition(WallChunkArchitect.Additions addition)
		{
			if (addition == WallChunkArchitect.Additions.Wall && this.CanTurnIntoGate())
			{
				return WallChunkArchitect.Additions.Door1;
			}
			return WallChunkArchitect.Additions.Wall;
		}

		
		protected override void InitAdditionTrigger()
		{
			if (this.CanTurnIntoGate())
			{
				base.GetComponentInChildren<Craft_Structure>().gameObject.AddComponent<WallAdditionTrigger>();
			}
		}

		
		public override void UpdateAddition(WallChunkArchitect.Additions addition)
		{
			if (!this._wasBuilt && this.IsDoor(this._addition) && !this.IsDoor(addition))
			{
				UnityEngine.Object.Destroy(base.gameObject.GetComponent<WallDefensiveGateAddition>());
			}
			this._addition = addition;
			if (!this._wasBuilt && this.IsDoor(this._addition) && !BoltNetwork.isClient)
			{
				if (!base.gameObject.GetComponent<WallDefensiveGateAddition>())
				{
					base.gameObject.AddComponent<WallDefensiveGateAddition>();
				}
				else
				{
					base.gameObject.GetComponent<WallDefensiveGateAddition>().Start();
				}
			}
		}

		
		protected override int GetLogCost()
		{
			return this._wallRoot.childCount;
		}

		
		
		public override Transform BuiltLogPrefab
		{
			get
			{
				return Prefabs.Instance.LogWallDefensiveExBuiltPrefab;
			}
		}

		
		
		
		public virtual WallDefensiveChunkReinforcement Reinforcement { get; set; }

		
		public override List<Vector3> GetMultiPointsPositions(bool inherit = true)
		{
			return null;
		}
	}
}
